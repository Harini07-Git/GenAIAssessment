using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SecureAPI.Models;

namespace SecureAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;
        private const int MaxFailedAttempts = 5;
        private static readonly Dictionary<string, int> _failedAttempts = new();
        private static readonly Dictionary<string, DateTime> _lockoutEndTime = new();

        public AuthService(IConfiguration configuration, IAuditLogService auditLogService)
        {
            _configuration = configuration;
            _auditLogService = auditLogService;
        }

        public async Task<AuthResponse> AuthenticateAsync(LoginRequest request)
        {
            // Check for account lockout
            if (IsAccountLocked(request.Email))
            {
                await _auditLogService.LogFailedLoginAttemptAsync(request.Email, "Account locked");
                throw new SecurityException("Account is locked. Please try again later.");
            }

            // In a real application, retrieve user from database
            // This is just for demonstration
            if (!ValidateUser(request.Email, request.Password))
            {
                IncrementFailedAttempts(request.Email);
                await _auditLogService.LogFailedLoginAttemptAsync(request.Email, "Invalid credentials");
                throw new SecurityException("Invalid credentials");
            }

            // Reset failed attempts on successful login
            ResetFailedAttempts(request.Email);

            var token = GenerateJwtToken(request.Email);
            var refreshToken = GenerateRefreshToken();

            await _auditLogService.LogSuccessfulLoginAsync(request.Email);

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            // Validate refresh token (in a real app, check against stored tokens)
            var principal = ValidateToken(refreshToken);
            if (principal == null)
            {
                throw new SecurityException("Invalid refresh token");
            }

            var email = principal.Identity?.Name;
            var newToken = GenerateJwtToken(email ?? string.Empty);
            var newRefreshToken = GenerateRefreshToken();

            await _auditLogService.LogTokenRefreshAsync(email ?? string.Empty);

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            // In a real application, invalidate the refresh token in the database
            await _auditLogService.LogLogoutAsync("user@example.com");
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsAccountLocked(string email)
        {
            if (_lockoutEndTime.TryGetValue(email, out var lockoutEnd))
            {
                if (DateTime.UtcNow < lockoutEnd)
                {
                    return true;
                }
                _lockoutEndTime.Remove(email);
            }
            return false;
        }

        private void IncrementFailedAttempts(string email)
        {
            if (!_failedAttempts.ContainsKey(email))
            {
                _failedAttempts[email] = 0;
            }

            _failedAttempts[email]++;

            if (_failedAttempts[email] >= MaxFailedAttempts)
            {
                _lockoutEndTime[email] = DateTime.UtcNow.AddMinutes(15);
            }
        }

        private void ResetFailedAttempts(string email)
        {
            _failedAttempts.Remove(email);
            _lockoutEndTime.Remove(email);
        }

        // Demo method - replace with actual database validation
        private bool ValidateUser(string email, string password)
        {
            // In a real application, retrieve the hashed password from the database
            var hashedPassword = HashPassword("demo123");
            return email == "demo@example.com" && VerifyPassword(password, hashedPassword);
        }
    }
}
