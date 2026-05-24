using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterRequest> registerValidator,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <param name="request">Registration request containing email, password, and user information</param>
        /// <returns>201 Created with user data and tokens on success, 400 for validation errors, 409 if email exists</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "Request body is required"
                    });
                }

                // Validate request
                var validationResult = await _registerValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Registration validation failed for email {Email}", request.Email);
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .Select(g => new ValidationError
                        {
                            Field = g.Key,
                            Message = g.First().ErrorMessage
                        })
                        .ToList();

                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                // Call registration service
                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                {
                    // Check if it's a duplicate email error (409) or other error (400)
                    if (result.Message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
                    {
                        return Conflict(result);
                    }

                    return BadRequest(result);
                }

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for email {Email}", request?.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new RegisterResponse
                {
                    Success = false,
                    Message = "An internal server error occurred during registration"
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid access token" });
            }

            var result = await _authService.LogoutAsync(userId);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred during logout" });
            }

            return Ok(new { success = true, message = "Logout successful. Tokens revoked." });
        }
    }
}
