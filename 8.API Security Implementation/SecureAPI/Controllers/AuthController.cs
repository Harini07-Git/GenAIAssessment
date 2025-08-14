using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureAPI.Models;
using SecureAPI.Services;
using System.Security.Claims;

namespace SecureAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditLogService _auditLogService;

        public AuthController(IAuthService authService, IAuditLogService auditLogService)
        {
            _authService = authService;
            _auditLogService = auditLogService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(request);
                return Ok(response);
            }
            catch (SecurityException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "An error occurred while processing your request." });
            }
        }

        [Authorize]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(response);
            }
            catch (SecurityException)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            await _authService.RevokeTokenAsync(refreshToken);
            return Ok(new { message = "Successfully logged out" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok(new { message = $"Hello {username}, this is a protected endpoint!" });
        }
    }
}
