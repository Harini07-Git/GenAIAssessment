using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LegacyMigration.Authentication;

namespace LegacyMigration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationFactory _authFactory;

        public AuthController(IAuthenticationFactory authFactory)
        {
            _authFactory = authFactory;
        }

        /// <summary>
        /// Authenticates a user using either legacy or modern authentication
        /// based on feature flag configuration.
        /// 
        /// Migration Notes:
        /// - Updated from old FormsAuthentication to modern token-based auth
        /// - Uses feature flags to control which implementation is used
        /// - Maintains same API contract for backward compatibility
        /// - New implementation uses standard JWT tokens
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var authService = await _authFactory.GetAuthServiceAsync();
            
            var isValid = await authService.ValidateUserAsync(request.Username, request.Password);
            if (!isValid)
            {
                return Unauthorized();
            }

            var token = await authService.GenerateTokenAsync(request.Username);
            return Ok(new { token });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
