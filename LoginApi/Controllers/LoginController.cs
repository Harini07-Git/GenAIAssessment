using Microsoft.AspNetCore.Mvc;
using LoginApi.Models;
using LoginApi.Services;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILoginService loginService, ILogger<LoginController> logger)
        {
            _loginService = loginService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserLogin>> GetLogin(int id)
        {
            try
            {
                var login = await _loginService.GetLoginByIdAsync(id);
                if (login == null)
                {
                    return NotFound($"Login with ID {id} not found");
                }
                return Ok(login);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login with ID {Id}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserLogin>>> GetAllLogins()
        {
            try
            {
                var logins = await _loginService.GetAllLoginsAsync();
                return Ok(logins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all logins");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserLogin>> CreateLogin([FromBody] UserLogin login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdLogin = await _loginService.CreateLoginAsync(login);
                return CreatedAtAction(nameof(GetLogin), new { id = createdLogin.Id }, createdLogin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new login");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserLogin>> UpdateLogin(int id, [FromBody] UserLogin login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedLogin = await _loginService.UpdateLoginAsync(id, login);
                if (updatedLogin == null)
                {
                    return NotFound($"Login with ID {id} not found");
                }
                return Ok(updatedLogin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login with ID {Id}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteLogin(int id)
        {
            try
            {
                var result = await _loginService.DeleteLoginAsync(id);
                if (!result)
                {
                    return NotFound($"Login with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting login with ID {Id}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
