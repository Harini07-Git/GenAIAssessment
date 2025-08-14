using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLayer.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace BusinessLayer.Tests.Services
{
    public class LoginServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly IConfiguration _config;
        private readonly LoginService _service;
        private readonly string _jwtSecret = "supersecretkey1234567890";
        private readonly string _jwtIssuer = "TestIssuer";
        private readonly string _jwtAudience = "TestAudience";
        private readonly int _jwtExpiryMinutes = 1;

        public LoginServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> {
                {"Jwt:Secret", _jwtSecret},
                {"Jwt:Issuer", _jwtIssuer},
                {"Jwt:Audience", _jwtAudience},
                {"Jwt:ExpiryMinutes", _jwtExpiryMinutes.ToString()}
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            _service = new LoginService(_userRepoMock.Object, _config);
        }

        [Fact]
        public async Task LoginAsync_Success_ReturnsToken()
        {
            var password = "Password123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepoMock.Setup(r => r.GetUserByUsernameAsync("user1")).ReturnsAsync(new User { Id = 1, Username = "user1", PasswordHash = hash, Role = "Admin" });

            var token = await _service.LoginAsync("user1", password);
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task LoginAsync_Fail_WrongPassword_Throws()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            _userRepoMock.Setup(r => r.GetUserByUsernameAsync("user1")).ReturnsAsync(new User { Id = 1, Username = "user1", PasswordHash = hash, Role = "Admin" });

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _service.LoginAsync("user1", "wrongpass"));
        }

        [Fact]
        public async Task ValidateToken_Expired_ThrowsTokenExpiredException()
        {
            var password = "Password123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepoMock.Setup(r => r.GetUserByUsernameAsync("user1")).ReturnsAsync(new User { Id = 1, Username = "user1", PasswordHash = hash, Role = "Admin" });
            var shortExpiryConfig = new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string> {
                {"Jwt:Secret", _jwtSecret},
                {"Jwt:Issuer", _jwtIssuer},
                {"Jwt:Audience", _jwtAudience},
                {"Jwt:ExpiryMinutes", "0"}
            }).Build();
            var service = new LoginService(_userRepoMock.Object, shortExpiryConfig);
            var token = await service.LoginAsync("user1", password);
            await Task.Delay(1100); // Wait for token to expire
            Assert.Throws<TokenExpiredException>(() => service.ValidateToken(token, out _));
        }

        [Fact]
        public async Task HasRequiredRole_Success_And_Failure()
        {
            var password = "Password123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepoMock.Setup(r => r.GetUserByUsernameAsync("user1")).ReturnsAsync(new User { Id = 1, Username = "user1", PasswordHash = hash, Role = "Admin" });
            var token = await _service.LoginAsync("user1", password);
            _service.ValidateToken(token, out var principal);
            Assert.True(_service.HasRequiredRole(principal, "Admin"));
            Assert.Throws<UnauthorizedAccessException>(() => _service.HasRequiredRole(principal, "User"));
        }
    }
}
