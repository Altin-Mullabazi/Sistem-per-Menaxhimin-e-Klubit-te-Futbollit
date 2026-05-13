using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _clubService;
        private readonly ILogger<ClubsController> _logger;

        public ClubsController(IClubService clubService, ILogger<ClubsController> logger)
        {
            _clubService = clubService;
            _logger = logger;
        }

        /// <summary>
        /// Get all clubs with pagination, search, filter and sort
        /// 
        /// Query Parameters:
        /// - page: Page number (default: 1)
        /// - pageSize: Items per page, 1-100 (default: 10)
        /// - search: Search by club name
        /// - city: Filter by city
        /// - sortBy: Sort by 'name', 'city', or 'founded' (default: name)
        /// 
        /// Validation Strategy:
        /// - Controller: Pre-validation for UX (fail fast)
        /// - Service: Business logic validation (defensive)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllClubs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? city = null,
            [FromQuery] string sortBy = "name")
        {
            try
            {
                // ✅ Pre-validation for user feedback (fail fast)
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(new { success = false, message = "Invalid pagination: page >= 1, pageSize 1-100" });

                var result = await _clubService.GetAllClubsAsync(page, pageSize, search, city, sortBy);
                return Ok(new { success = true, data = result, message = "Clubs retrieved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving clubs: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving clubs" });
            }
        }

        /// <summary>
        /// Get specific club by ID with players
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClubById(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Club ID must be a positive number" });

                var club = await _clubService.GetClubByIdAsync(id);
                if (club == null)
                    return NotFound(new { success = false, message = "Club not found" });

                return Ok(new { success = true, data = club, message = "Club retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving club: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the club" });
            }
        }

        /// <summary>
        /// Create a new club (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto createClubDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var club = await _clubService.CreateClubAsync(createClubDto);
                return CreatedAtAction(nameof(GetClubById), new { id = club.Id }, 
                    new { success = true, data = club, message = "Club created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error creating club: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating club: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the club" });
            }
        }

        /// <summary>
        /// Update an existing club (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateClub(int id, [FromBody] UpdateClubDto updateClubDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id < 1)
                return BadRequest(new { success = false, message = "Club ID must be a positive number" });

            try
            {
                var club = await _clubService.UpdateClubAsync(id, updateClubDto);
                if (club == null)
                    return NotFound(new { success = false, message = "Club not found" });

                return Ok(new { success = true, data = club, message = "Club updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error updating club: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating club: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the club" });
            }
        }

        /// <summary>
        /// Delete a club (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteClub(int id)
        {
            if (id < 1)
                return BadRequest(new { success = false, message = "Club ID must be a positive number" });

            try
            {
                var result = await _clubService.DeleteClubAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Club not found" });

                return Ok(new { success = true, message = "Club deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting club: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the club" });
            }
        }

        /// <summary>
        /// Get total clubs count
        /// </summary>
        [HttpGet("count/total")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClubsCount()
        {
            try
            {
                var count = await _clubService.GetClubsCountAsync();
                return Ok(new { success = true, data = new { count = count }, message = "Club count retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving club count: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving club count" });
            }
        }
    }
}
