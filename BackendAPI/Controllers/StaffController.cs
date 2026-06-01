using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaff([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? clubId = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var (staff, totalCount) = await _staffService.GetStaffAsync(page, pageSize, search, clubId);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    success = true,
                    data = staff,
                    pagination = new { currentPage = page, pageSize, totalCount, totalPages },
                    message = "Staff retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving staff" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id);
                if (staff == null) return NotFound(new { success = false, message = "Staff not found" });
                return Ok(new { success = true, data = staff, message = "Staff retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving staff" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
                var staff = await _staffService.CreateStaffAsync(createDto);
                return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, new { success = true, data = staff, message = "Staff created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                return StatusCode(500, new { success = false, message = ex.Message ?? "An error occurred while creating staff" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
                var staff = await _staffService.UpdateStaffAsync(id, updateDto);
                if (staff == null) return NotFound(new { success = false, message = "Staff not found" });
                return Ok(new { success = true, data = staff, message = "Staff updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff");
                return StatusCode(500, new { success = false, message = "An error occurred while updating staff" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);
                if (!result) return NotFound(new { success = false, message = "Staff not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting staff" });
            }
        }
    }
}
