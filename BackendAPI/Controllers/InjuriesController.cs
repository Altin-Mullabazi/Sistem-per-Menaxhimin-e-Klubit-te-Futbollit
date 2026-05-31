using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InjuriesController : ControllerBase
    {
        private readonly IInjuryService _injuryService;
        private readonly ILogger<InjuriesController> _logger;

        public InjuriesController(IInjuryService injuryService, ILogger<InjuriesController> logger)
        {
            _injuryService = injuryService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of injuries with optional filtering and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="playerId">Filter by player ID</param>
        /// <param name="status">Filter by status (Active, Recovering, Recovered)</param>
        /// <param name="sortBy">Sort by field: date (default), player</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetInjuries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? playerId = null,
            [FromQuery] string? status = null,
            [FromQuery] string sortBy = "date")
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and pageSize must be greater than 0" });

                var result = await _injuryService.GetInjuriesAsync(page, pageSize, playerId, status, sortBy);
                return Ok(new { success = true, data = result, message = "Injuries retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving injuries: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving injuries" });
            }
        }

        /// <summary>
        /// Get all active injuries (Status != Recovered)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveInjuries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and pageSize must be greater than 0" });

                var result = await _injuryService.GetActiveInjuriesAsync(page, pageSize);
                return Ok(new { success = true, data = result, message = "Active injuries retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving active injuries: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving active injuries" });
            }
        }

        /// <summary>
        /// Get injury by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInjuryById(int id)
        {
            try
            {
                var injury = await _injuryService.GetInjuryByIdAsync(id);
                if (injury == null)
                    return NotFound(new { success = false, message = "Injury not found" });

                return Ok(new { success = true, data = injury, message = "Injury retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving injury: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the injury" });
            }
        }

        /// <summary>
        /// Create a new injury
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateInjury([FromBody] CreateInjuryDto createInjuryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var injury = await _injuryService.CreateInjuryAsync(createInjuryDto);
                return CreatedAtAction(nameof(GetInjuryById), new { id = injury.Id }, 
                    new { success = true, data = injury, message = "Injury created successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating injury: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating injury: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the injury" });
            }
        }

        /// <summary>
        /// Update an injury
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateInjury(int id, [FromBody] UpdateInjuryDto updateInjuryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var injury = await _injuryService.UpdateInjuryAsync(id, updateInjuryDto);
                if (injury == null)
                    return NotFound(new { success = false, message = "Injury not found" });

                return Ok(new { success = true, data = injury, message = "Injury updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error updating injury: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating injury: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the injury" });
            }
        }

        /// <summary>
        /// Delete an injury
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteInjury(int id)
        {
            try
            {
                var result = await _injuryService.DeleteInjuryAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Injury not found" });

                return Ok(new { success = true, message = "Injury deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting injury: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the injury" });
            }
        }
    }
}
