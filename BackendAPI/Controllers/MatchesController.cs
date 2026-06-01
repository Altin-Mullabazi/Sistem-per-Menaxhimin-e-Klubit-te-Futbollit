using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(IMatchService matchService, ILogger<MatchesController> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        /// <summary>
        /// Get total matches count
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMatchesCount()
        {
            try
            {
                var count = await _matchService.GetMatchesCountAsync();
                return Ok(new { success = true, data = new { count }, message = "Matches count retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving matches count: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving matches count" });
            }
        }

        /// <summary>
        /// Get upcoming matches in next N days
        /// </summary>
        /// <param name="days">Number of days to look ahead (default: 7)</param>
        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcomingMatches(int days = 7)
        {
            try
            {
                var matches = await _matchService.GetUpcomingMatchesAsync(days);
                return Ok(new { success = true, data = matches, message = "Upcoming matches retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving upcoming matches: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving upcoming matches" });
            }
        }

        /// <summary>
        /// Get paginated matches list with filters and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="clubId">Filter by club ID (optional)</param>
        /// <param name="seasonId">Filter by season ID (optional)</param>
        /// <param name="status">Filter by status (optional)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMatches(int page = 1, int pageSize = 10, int? clubId = null, int? seasonId = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { success = false, message = "Invalid pagination: page >= 1, pageSize 1-100" });
                }

                var (matches, totalCount) = await _matchService.GetMatchesAsync(page, pageSize, clubId, seasonId, status, startDate, endDate);
                return Ok(new
                {
                    success = true,
                    data = matches,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    message = "Matches retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving matches: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving matches" });
            }
        }

        /// <summary>
        /// Get specific match by ID with detailed information
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatchById(int id)
        {
            try
            {
                var match = await _matchService.GetMatchByIdAsync(id);
                if (match == null)
                    return NotFound(new { success = false, message = "Match not found" });

                return Ok(new { success = true, data = match, message = "Match retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving match: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the match" });
            }
        }

        /// <summary>
        /// Create a new match
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchDto createMatchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var match = await _matchService.CreateMatchAsync(createMatchDto);
                return CreatedAtAction(nameof(GetMatchById), new { id = match.Id }, new { success = true, data = match, message = "Match created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error creating match: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating match: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the match" });
            }
        }

        /// <summary>
        /// Update match scores and status
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMatch(int id, [FromBody] UpdateMatchDto updateMatchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var match = await _matchService.UpdateMatchAsync(id, updateMatchDto);
                if (match == null)
                    return NotFound(new { success = false, message = "Match not found" });

                return Ok(new { success = true, data = match, message = "Match updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error updating match: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating match: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the match" });
            }
        }

        /// <summary>
        /// Delete a match
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            try
            {
                var result = await _matchService.DeleteMatchAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Match not found" });

                return Ok(new { success = true, message = "Match deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting match: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the match" });
            }
        }
    }
}
