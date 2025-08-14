using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SecureAPI.Models;
using SecureAPI.Services;
using Xunit;

namespace SecureAPI.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<ILogger<AuditLogService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockLogger = new Mock<ILogger<AuditLogService>>();

            // Setup configuration
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("YourSuperSecretKeyHereThatIsAtLeast32CharactersLong");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("YourIssuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("YourAudience");

            _authService = new AuthService(_mockConfiguration.Object, _mockAuditLogService.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "demo@example.com",
                Password = "demo123"
            };

            // Act
            var result = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
            Assert.True(result.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidCredentials_ThrowsSecurityException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "wrong@example.com",
                Password = "wrongpassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => 
                _authService.AuthenticateAsync(request));
        }

        [Fact]
        public void HashPassword_GeneratesValidHash()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hashedPassword = _authService.HashPassword(password);

            // Assert
            Assert.True(_authService.VerifyPassword(password, hashedPassword));
            Assert.False(_authService.VerifyPassword("WrongPassword", hashedPassword));
        }
    }
}
