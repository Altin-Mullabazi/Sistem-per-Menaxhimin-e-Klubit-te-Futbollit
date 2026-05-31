using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/user-roles")]
    [Authorize(Roles = "Admin")]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(IUserRoleService userRoleService, ILogger<UserRolesController> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            if (!IsAuthenticatedAdmin())
            {
                return GetAccessFailureResult();
            }

            try
            {
                var userRoles = await _userRoleService.GetUserRolesAsync(userId);
                if (userRoles == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new
                {
                    success = true,
                    data = userRoles.Roles,
                    user = new
                    {
                        userRoles.UserId,
                        userRoles.UserName,
                        userRoles.Email
                    },
                    message = "User roles retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while retrieving user roles" });
            }
        }

        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleAssignmentDto request)
        {
            if (!IsAuthenticatedAdmin())
            {
                return GetAccessFailureResult();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            try
            {
                var (result, assignment) = await _userRoleService.AssignRoleAsync(request.UserId, request.RoleId);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return Ok(new
                {
                    success = true,
                    data = assignment,
                    message = assignment?.Message ?? "Role assigned successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while assigning the role" });
            }
        }

        private IActionResult MapIdentityFailure(IdentityResult result)
        {
            var message = string.Join("; ", result.Errors.Select(error => error.Description));

            if (result.Errors.Any(error => string.Equals(error.Code, "NotFound", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { success = false, message });
            }

            if (result.Errors.Any(error => string.Equals(error.Code, "DuplicateRoleAssignment", StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { success = false, message });
            }

            if (result.Errors.Any(error => string.Equals(error.Code, "InvalidRole", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { success = false, message });
            }

            return BadRequest(new { success = false, message });
        }

        private bool IsAuthenticatedAdmin()
        {
            return User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        }

        private IActionResult GetAccessFailureResult()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Forbid();
            }

            return Unauthorized();
        }
    }
}