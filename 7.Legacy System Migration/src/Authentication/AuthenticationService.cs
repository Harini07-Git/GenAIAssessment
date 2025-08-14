using Microsoft.FeatureManagement;

namespace LegacyMigration.Authentication
{
    // Common interface that both implementations will use
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<string> GenerateTokenAsync(string username);
    }

    // Legacy implementation
    public class LegacyAuthService : IAuthService
    {
        // Old implementation using deprecated methods
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            // Legacy implementation using old authentication methods
            return await Task.FromResult(true);
        }

        public async Task<string> GenerateTokenAsync(string username)
        {
            // Old token generation logic
            return await Task.FromResult("legacy-token");
        }
    }

    // Modern implementation
    public class ModernAuthService : IAuthService
    {
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            // Modern implementation using current best practices
            return await Task.FromResult(true);
        }

        public async Task<string> GenerateTokenAsync(string username)
        {
            // Modern JWT token generation
            return await Task.FromResult("modern-jwt-token");
        }
    }

    // Factory to handle switching between implementations
    public interface IAuthenticationFactory
    {
        Task<IAuthService> GetAuthServiceAsync();
    }

    public class AuthenticationFactory : IAuthenticationFactory
    {
        private readonly ILegacyCompatibilityLayer _compatLayer;
        private readonly LegacyAuthService _legacyAuth;
        private readonly ModernAuthService _modernAuth;

        public AuthenticationFactory(
            ILegacyCompatibilityLayer compatLayer,
            LegacyAuthService legacyAuth,
            ModernAuthService modernAuth)
        {
            _compatLayer = compatLayer;
            _legacyAuth = legacyAuth;
            _modernAuth = modernAuth;
        }

        public async Task<IAuthService> GetAuthServiceAsync()
        {
            // Use feature flag to determine which implementation to use
            return await _compatLayer.IsFeatureEnabledAsync("UseNewAuthenticationSystem")
                ? _modernAuth
                : _legacyAuth;
        }
    }
}
