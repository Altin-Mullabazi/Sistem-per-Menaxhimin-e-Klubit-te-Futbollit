using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var result = await _userService.GetUsersAsync(page, pageSize, search);
                return Ok(new
                {
                    success = true,
                    data = result.Items,
                    pagination = new
                    {
                        currentPage = result.Page,
                        pageSize = result.PageSize,
                        totalCount = result.TotalCount,
                        totalPages = result.TotalPages
                    },
                    message = "Users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                if (!CanAccessUser(id))
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, data = user, message = "User retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while retrieving the user" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            try
            {
                var (result, user) = await _userService.CreateUserAsync(createUserDto);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return CreatedAtAction(nameof(GetUserById), new { id = user!.Id }, new { success = true, data = user, message = "User created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email {Email}", createUserDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while creating the user" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            try
            {
                if (!CanAccessUser(id))
                {
                    return Forbid();
                }

                var (result, user) = await _userService.UpdateUserAsync(id, updateUserDto);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return Ok(new { success = true, data = user, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while updating the user" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!string.IsNullOrWhiteSpace(currentUserId) && string.Equals(currentUserId, id, StringComparison.Ordinal))
                {
                    return BadRequest(new { success = false, message = "You cannot delete your own account" });
                }

                var result = await _userService.DeleteUserAsync(id);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return Ok(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while deleting the user" });
            }
        }

        private bool CanAccessUser(string id)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUserId = GetCurrentUserId();
            return !string.IsNullOrWhiteSpace(currentUserId) && string.Equals(currentUserId, id, StringComparison.Ordinal);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private IActionResult MapIdentityFailure(IdentityResult result)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));

            if (result.Errors.Any(error => string.Equals(error.Code, "NotFound", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { success = false, message = errors });
            }

            if (result.Errors.Any(error => string.Equals(error.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { success = false, message = errors });
            }

            if (result.Errors.Any(error => string.Equals(error.Code, "InvalidRole", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { success = false, message = errors });
            }

            return BadRequest(new { success = false, message = errors });
        }
    }
}