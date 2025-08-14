using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Threading;
using Xunit;
using PaymentGateway.Services;
using PaymentGateway.Models;

namespace PaymentGateway.Tests
{
    public class PaymentGatewayServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<PaymentGatewayService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly PaymentGatewayService _service;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public PaymentGatewayServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<PaymentGatewayService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x.Value).Returns("https://api.payment-gateway.com/");
            _mockConfiguration.Setup(x => x["PaymentGateway:BaseUrl"])
                            .Returns("https://api.payment-gateway.com/");

            var client = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.payment-gateway.com/")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient("PaymentGateway"))
                                .Returns(client);

            _service = new PaymentGatewayService(
                _mockHttpClientFactory.Object,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task ProcessPaymentAsync_SuccessfulPayment_ReturnsPaymentResponse()
        {
            // Arrange
            var request = new PaymentRequest
            {
                OrderId = "order-123",
                Amount = 100.00m,
                Currency = "USD",
                CardNumber = "4111111111111111",
                CardHolderName = "John Doe",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                Cvv = "123"
            };

            var expectedResponse = new PaymentResponse
            {
                TransactionId = "tx-123",
                Status = "success",
                Timestamp = DateTime.UtcNow,
                AuthorizationCode = "AUTH123"
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedResponse))
                });

            // Act
            var result = await _service.ProcessPaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.TransactionId, result.TransactionId);
            Assert.Equal(expectedResponse.Status, result.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ApiError_ThrowsApiCommunicationException()
        {
            // Arrange
            var request = new PaymentRequest
            {
                OrderId = "order-123",
                Amount = 100.00m,
                Currency = "USD",
                CardNumber = "4111111111111111"
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApiCommunicationException>(
                () => _service.ProcessPaymentAsync(request));
        }

        [Fact]
        public async Task ProcessPaymentAsync_RetryLogic_ExecutesCorrectly()
        {
            // Arrange
            var request = new PaymentRequest
            {
                OrderId = "order-123",
                Amount = 100.00m,
                Currency = "USD",
                CardNumber = "4111111111111111"
            };

            var callCount = 0;
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns<HttpRequestMessage, CancellationToken>((req, token) =>
                {
                    callCount++;
                    if (callCount < 3)
                    {
                        throw new HttpRequestException("Network error");
                    }

                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(
                            new PaymentResponse { TransactionId = "tx-123", Status = "success" }))
                    });
                });

            // Act
            var result = await _service.ProcessPaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, callCount); // Verify that it retried twice before succeeding
            Assert.Equal("success", result.Status);
        }
    }
}
