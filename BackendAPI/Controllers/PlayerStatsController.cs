using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayerStatsController : ControllerBase
    {
        private readonly IPlayerStatsService _playerStatsService;
        private readonly ILogger<PlayerStatsController> _logger;

        public PlayerStatsController(IPlayerStatsService playerStatsService, ILogger<PlayerStatsController> logger)
        {
            _playerStatsService = playerStatsService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerStats(int page = 1, int pageSize = 10, int? playerId = null, int? matchId = null, string? sortBy = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { success = false, message = "Invalid pagination: page >= 1, pageSize 1-100" });

            try
            {
                var (stats, totalCount) = await _playerStatsService.GetPlayerStatsAsync(page, pageSize, playerId, matchId, sortBy);
                return Ok(new
                {
                    success = true,
                    data = stats,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    message = "Player stats retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving player stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving player stats" });
            }
        }

        [HttpGet("top-assists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopAssists(int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { success = false, message = "Limit must be between 1 and 100" });

            try
            {
                var topAssists = await _playerStatsService.GetTopAssistsAsync(limit);
                return Ok(new { success = true, data = topAssists, message = "Top assists retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving top assists: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving top assists" });
            }
        }

        [HttpGet("top-scorers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopScorers(int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { success = false, message = "Limit must be between 1 and 100" });

            try
            {
                var topScorers = await _playerStatsService.GetTopScorersAsync(limit);
                return Ok(new { success = true, data = topScorers, message = "Top scorers retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving top scorers: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving top scorers" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePlayerStats([FromBody] CreatePlayerStatsDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var stats = await _playerStatsService.CreatePlayerStatsAsync(createDto);
                return CreatedAtAction(nameof(GetPlayerStats), new { page = 1 }, new { success = true, data = stats, message = "Player stats created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error creating player stats: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating player stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating player stats" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePlayerStats(int id, [FromBody] UpdatePlayerStatsDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var stats = await _playerStatsService.UpdatePlayerStatsAsync(id, updateDto);
                if (stats == null)
                    return NotFound(new { success = false, message = "Player stats not found" });

                return Ok(new { success = true, data = stats, message = "Player stats updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error updating player stats: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating player stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating player stats" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlayerStats(int id)
        {
            try
            {
                var deleted = await _playerStatsService.DeletePlayerStatsAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Player stats not found" });

                return Ok(new { success = true, message = "Player stats deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting player stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting player stats" });
            }
        }
    }
}
