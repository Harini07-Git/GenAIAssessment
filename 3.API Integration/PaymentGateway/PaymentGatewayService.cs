using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace PaymentGateway.Services
{
    /// <summary>
    /// Service responsible for handling payment gateway operations including processing payments,
    /// refunds and checking payment status through third-party API integration.
    /// </summary>
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PaymentGatewayService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        /// <summary>
        /// Initializes a new instance of the PaymentGatewayService class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory for creating HttpClient instances.</param>
        /// <param name="logger">Logger for recording service operations.</param>
        /// <param name="configuration">Configuration for service settings.</param>
        public PaymentGatewayService(
            IHttpClientFactory httpClientFactory,
            ILogger<PaymentGatewayService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, 
                            "Retry {RetryCount} after {TimeSpan}s delay due to: {Message}", 
                            retryCount, timeSpan.TotalSeconds, exception.Message);
                    });
        }

        /// <summary>
        /// Processes a payment request through the payment gateway API.
        /// </summary>
        /// <param name="request">The payment request details.</param>
        /// <returns>A PaymentResponse containing the transaction result.</returns>
        /// <exception cref="PaymentProcessingException">Thrown when payment processing fails.</exception>
        /// <exception cref="ApiCommunicationException">Thrown when API communication fails.</exception>
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for order {OrderId}", request.OrderId);
                
                var maskedRequest = MaskSensitiveData(request);
                _logger.LogDebug("Payment request: {@Request}", maskedRequest);

                using var client = CreateHttpClient();
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var content = JsonSerializer.Serialize(request);
                    var httpResponse = await client.PostAsync("payments", 
                        new StringContent(content, System.Text.Encoding.UTF8, "application/json"));
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var paymentResponse = await DeserializeResponseAsync<PaymentResponse>(response);
                _logger.LogInformation("Payment processed successfully for order {OrderId}", request.OrderId);
                
                return paymentResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API communication error while processing payment for order {OrderId}", request.OrderId);
                throw new ApiCommunicationException("Failed to communicate with payment gateway", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing failed for order {OrderId}", request.OrderId);
                throw new PaymentProcessingException("Failed to process payment", ex);
            }
        }

        /// <summary>
        /// Processes a refund request through the payment gateway API.
        /// </summary>
        /// <param name="request">The refund request details.</param>
        /// <returns>A RefundResponse containing the refund result.</returns>
        /// <exception cref="RefundProcessingException">Thrown when refund processing fails.</exception>
        /// <exception cref="ApiCommunicationException">Thrown when API communication fails.</exception>
        public async Task<RefundResponse> ProcessRefundAsync(RefundRequest request)
        {
            try
            {
                _logger.LogInformation("Processing refund for transaction {TransactionId}", request.TransactionId);
                
                using var client = CreateHttpClient();
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var content = JsonSerializer.Serialize(request);
                    var httpResponse = await client.PostAsync("refunds", 
                        new StringContent(content, System.Text.Encoding.UTF8, "application/json"));
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var refundResponse = await DeserializeResponseAsync<RefundResponse>(response);
                _logger.LogInformation("Refund processed successfully for transaction {TransactionId}", 
                    request.TransactionId);
                
                return refundResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API communication error while processing refund for transaction {TransactionId}", 
                    request.TransactionId);
                throw new ApiCommunicationException("Failed to communicate with payment gateway", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund processing failed for transaction {TransactionId}", 
                    request.TransactionId);
                throw new RefundProcessingException("Failed to process refund", ex);
            }
        }

        /// <summary>
        /// Retrieves the current status of a payment transaction.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to check.</param>
        /// <returns>A PaymentStatusResponse containing the current status.</returns>
        /// <exception cref="ApiCommunicationException">Thrown when API communication fails.</exception>
        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                _logger.LogInformation("Retrieving payment status for transaction {TransactionId}", transactionId);
                
                using var client = CreateHttpClient();
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await client.GetAsync($"payments/{transactionId}/status");
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var statusResponse = await DeserializeResponseAsync<PaymentStatusResponse>(response);
                _logger.LogInformation("Successfully retrieved status for transaction {TransactionId}: {Status}", 
                    transactionId, statusResponse.Status);
                
                return statusResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API communication error while retrieving status for transaction {TransactionId}", 
                    transactionId);
                throw new ApiCommunicationException("Failed to communicate with payment gateway", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve payment status for transaction {TransactionId}", 
                    transactionId);
                throw new ApiCommunicationException("Failed to retrieve payment status", ex);
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient("PaymentGateway");
            client.BaseAddress = new Uri(_configuration["PaymentGateway:BaseUrl"]);
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private PaymentRequest MaskSensitiveData(PaymentRequest request)
        {
            return new PaymentRequest
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                CardNumber = $"****-****-****-{request.CardNumber.Substring(request.CardNumber.Length - 4)}",
                // Mask other sensitive fields as needed
            };
        }
    }
}
