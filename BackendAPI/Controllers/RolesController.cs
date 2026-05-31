using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRoles()
        {
            if (!IsAuthenticatedAdmin())
            {
                return GetAccessFailureResult();
            }

            try
            {
                var roles = await _roleService.GetRolesAsync();
                return Ok(new { success = true, data = roles, message = "Roles retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while retrieving roles" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            if (!IsAuthenticatedAdmin())
            {
                return GetAccessFailureResult();
            }

            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { success = false, message = "Role not found" });
                }

                return Ok(new { success = true, data = role, message = "Role retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while retrieving the role" });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
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
                var (result, role) = await _roleService.CreateRoleAsync(createRoleDto);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return CreatedAtAction(nameof(GetRoleById), new { id = role!.Id }, new { success = true, data = role, message = "Role created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role {RoleName}", createRoleDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while creating the role" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
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
                var (result, role) = await _roleService.UpdateRoleAsync(id, updateRoleDto);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return Ok(new { success = true, data = role, message = "Role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while updating the role" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (!IsAuthenticatedAdmin())
            {
                return GetAccessFailureResult();
            }

            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result.Succeeded)
                {
                    return MapIdentityFailure(result);
                }

                return Ok(new { success = true, message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while deleting the role" });
            }
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

        private IActionResult MapIdentityFailure(IdentityResult result)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));

            if (result.Errors.Any(error => string.Equals(error.Code, "NotFound", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { success = false, message = errors });
            }

            return BadRequest(new { success = false, message = errors });
        }
    }
}