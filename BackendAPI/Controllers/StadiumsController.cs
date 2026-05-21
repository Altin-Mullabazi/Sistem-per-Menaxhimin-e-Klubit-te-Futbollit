using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StadiumsController : ControllerBase
    {
        private readonly IStadiumService _stadiumService;
        private readonly ILogger<StadiumsController> _logger;

        public StadiumsController(IStadiumService stadiumService, ILogger<StadiumsController> logger)
        {
            _stadiumService = stadiumService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of stadiums with optional filters and search
        /// Filter by city, search by stadium name
        /// Includes clubs using each stadium
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="search">Search by stadium name</param>
        /// <param name="city">Filter by city</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStadiums(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? city = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var (stadiums, totalCount) = await _stadiumService.GetStadiumsWithPaginationAsync(
                    page, pageSize, search, city);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    success = true,
                    data = stadiums,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    },
                    message = "Stadiums retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving stadiums: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving stadiums" });
            }
        }

        /// <summary>
        /// Get specific stadium with detailed information
        /// Includes all clubs using this stadium and matches scheduled there
        /// </summary>
        /// <param name="id">Stadium ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStadiumById(int id)
        {
            try
            {
                var stadium = await _stadiumService.GetStadiumByIdAsync(id);
                if (stadium == null)
                    return NotFound(new { success = false, message = "Stadium not found" });

                return Ok(new { success = true, data = stadium, message = "Stadium retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving stadium: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the stadium" });
            }
        }

        /// <summary>
        /// Create a new stadium
        /// Admin only - requires Authorization header with admin role
        /// Validates: Name, City, Capacity (must be > 0), YearBuilt
        /// </summary>
        /// <param name="createStadiumDto">Stadium data</param>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateStadium([FromBody] CreateStadiumDto createStadiumDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid stadium data", errors = ModelState });

                if (createStadiumDto.Capacity <= 0)
                    return BadRequest(new { success = false, message = "Capacity must be greater than 0" });

                var stadium = await _stadiumService.CreateStadiumAsync(createStadiumDto);

                return CreatedAtAction(nameof(GetStadiumById), new { id = stadium.Id }, new
                {
                    success = true,
                    data = stadium,
                    message = "Stadium created successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating stadium: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the stadium" });
            }
        }

        /// <summary>
        /// Update an existing stadium
        /// Admin only - requires Authorization header with admin role
        /// Validates: Capacity (must be > 0)
        /// </summary>
        /// <param name="id">Stadium ID to update</param>
        /// <param name="updateStadiumDto">Updated stadium data</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStadium(int id, [FromBody] UpdateStadiumDto updateStadiumDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid stadium data", errors = ModelState });

                if (updateStadiumDto.Capacity <= 0)
                    return BadRequest(new { success = false, message = "Capacity must be greater than 0" });

                var stadium = await _stadiumService.UpdateStadiumAsync(id, updateStadiumDto);
                if (stadium == null)
                    return NotFound(new { success = false, message = "Stadium not found" });

                return Ok(new { success = true, data = stadium, message = "Stadium updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating stadium: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the stadium" });
            }
        }

        /// <summary>
        /// Delete a stadium
        /// Admin only - requires Authorization header with admin role
        /// Validation: Cannot delete stadium if it has scheduled matches
        /// </summary>
        /// <param name="id">Stadium ID to delete</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStadium(int id)
        {
            try
            {
                var (success, message) = await _stadiumService.DeleteStadiumAsync(id);

                if (!success)
                {
                    if (message == "Stadium not found")
                        return NotFound(new { success = false, message = message });
                    
                    return BadRequest(new { success = false, message = message });
                }

                return Ok(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting stadium: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the stadium" });
            }
        }
    }
}
