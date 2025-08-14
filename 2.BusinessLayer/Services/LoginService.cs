using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace BusinessLayer.Services
{
    /// <summary>
    /// Provides user authentication and authorization services.
    /// </summary>
    public class LoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiryMinutes;

        public LoginService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtSecret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
            _jwtExpiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");
        }

        /// <summary>
        /// Authenticates the user and returns a JWT token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>JWT token string.</returns>
        /// <exception cref="InvalidCredentialsException">Thrown if credentials are invalid.</exception>
        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidCredentialsException("Invalid username or password.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Invalidates the JWT token (stateless, so this is a no-op or can be implemented with a blacklist).
        /// </summary>
        /// <param name="token">The JWT token to invalidate.</param>
        public Task LogoutAsync(string token)
        {
            // For stateless JWT, implement token blacklist if needed.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a JWT token and returns the authenticated principal.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="userPrincipal">The authenticated ClaimsPrincipal.</param>
        /// <returns>True if valid, false otherwise.</returns>
        /// <exception cref="TokenExpiredException">Thrown if token is expired.</exception>
        public bool ValidateToken(string token, out ClaimsPrincipal userPrincipal)
        {
            userPrincipal = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            try
            {
                userPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _jwtIssuer,
                    ValidAudience = _jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new TokenExpiredException("Token has expired.");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the user has the required role.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user.</param>
        /// <param name="requiredRole">The required role.</param>
        /// <returns>True if user has the role, false otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if user does not have the required role.</exception>
        public bool HasRequiredRole(ClaimsPrincipal user, string requiredRole)
        {
            if (user.IsInRole(requiredRole))
                return true;
            throw new UnauthorizedAccessException($"User does not have required role: {requiredRole}");
        }
    }

    /// <summary>
    /// Interface for user repository.
    /// </summary>
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
    }

    /// <summary>
    /// User entity.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }

    public class TokenExpiredException : Exception
    {
        public TokenExpiredException(string message) : base(message) { }
    }
}
