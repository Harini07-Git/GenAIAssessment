using Xunit;
using Moq;
using System.Threading.Tasks;
using LegacyMigration.Authentication;
using LegacyMigration.Compatibility;

namespace LegacyMigration.Tests
{
    public class AuthenticationTests
    {
        private readonly Mock<ILegacyCompatibilityLayer> _mockCompatLayer;
        private readonly LegacyAuthService _legacyAuth;
        private readonly ModernAuthService _modernAuth;
        private readonly AuthenticationFactory _factory;

        public AuthenticationTests()
        {
            _mockCompatLayer = new Mock<ILegacyCompatibilityLayer>();
            _legacyAuth = new LegacyAuthService();
            _modernAuth = new ModernAuthService();
            _factory = new AuthenticationFactory(_mockCompatLayer.Object, _legacyAuth, _modernAuth);
        }

        [Fact]
        public async Task WhenFeatureFlagEnabled_UsesModernAuth()
        {
            // Arrange
            _mockCompatLayer
                .Setup(x => x.IsFeatureEnabledAsync("UseNewAuthenticationSystem"))
                .ReturnsAsync(true);

            // Act
            var authService = await _factory.GetAuthServiceAsync();

            // Assert
            Assert.IsType<ModernAuthService>(authService);
        }

        [Fact]
        public async Task WhenFeatureFlagDisabled_UsesLegacyAuth()
        {
            // Arrange
            _mockCompatLayer
                .Setup(x => x.IsFeatureEnabledAsync("UseNewAuthenticationSystem"))
                .ReturnsAsync(false);

            // Act
            var authService = await _factory.GetAuthServiceAsync();

            // Assert
            Assert.IsType<LegacyAuthService>(authService);
        }

        [Theory]
        [InlineData(true)]  // Test modern implementation
        [InlineData(false)] // Test legacy implementation
        public async Task AuthenticationWorks_RegardlessOfImplementation(bool useModern)
        {
            // Arrange
            _mockCompatLayer
                .Setup(x => x.IsFeatureEnabledAsync("UseNewAuthenticationSystem"))
                .ReturnsAsync(useModern);

            var authService = await _factory.GetAuthServiceAsync();

            // Act
            var isValid = await authService.ValidateUserAsync("testuser", "password");
            var token = await authService.GenerateTokenAsync("testuser");

            // Assert
            Assert.True(isValid);
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }
    }
}
