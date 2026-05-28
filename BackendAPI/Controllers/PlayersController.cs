using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of players with filters, search, and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="search">Search by player name</param>
        /// <param name="clubId">Filter by club ID</param>
        /// <param name="position">Filter by position</param>
        /// <param name="sortBy">Sort by: createdAt (default), jersey, name, position</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? clubId = null,
            [FromQuery] string? position = null,
            [FromQuery] string? sortBy = "createdAt")
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var (players, totalCount) = await _playerService.GetPlayersWithPaginationAsync(
                    page, pageSize, search, clubId, position, sortBy);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    success = true,
                    data = players,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    },
                    message = "Players retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving players: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving players" });
            }
        }

        /// <summary>
        /// Get specific player with detailed information
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayerById(int id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);
                if (player == null)
                    return NotFound(new { success = false, message = "Player not found" });

                return Ok(new { success = true, data = player, message = "Player retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the player" });
            }
        }

        /// <summary>
        /// Advanced search with multiple filters
        /// </summary>
        /// <param name="name">Search by player name</param>
        /// <param name="clubId">Filter by club ID</param>
        /// <param name="position">Filter by position</param>
        [HttpGet("search/advanced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchPlayers(
            [FromQuery] string? name = null,
            [FromQuery] int? clubId = null,
            [FromQuery] string? position = null)
        {
            try
            {
                var players = await _playerService.SearchPlayersAsync(name, clubId, position);
                return Ok(new { success = true, data = players, message = "Search results retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching players: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while searching players" });
            }
        }

        /// <summary>
        /// Get all players in a specific club
        /// </summary>
        [HttpGet("club/{clubId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayersByClub(int clubId)
        {
            try
            {
                var players = await _playerService.GetPlayersByClubAsync(clubId);
                if (players == null || !players.Any())
                    return NotFound(new { success = false, message = "No players found for this club" });

                return Ok(new { success = true, data = players, message = "Players retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving club players: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving club players" });
            }
        }

        /// <summary>
        /// Create a new player (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
                // Validate jersey number
                if (createPlayerDto.JerseyNumber < 1 || createPlayerDto.JerseyNumber > 99)
                    return BadRequest(new { success = false, message = "Jersey number must be between 1 and 99" });

                var player = await _playerService.CreatePlayerAsync(createPlayerDto);
                return CreatedAtAction(nameof(GetPlayerById), new { id = player.Id }, 
                    new { success = true, data = player, message = "Player created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the player" });
            }
        }

        /// <summary>
        /// Update player (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePlayer(int id, [FromBody] UpdatePlayerDto updatePlayerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
                // Validate jersey number
                if (updatePlayerDto.JerseyNumber < 1 || updatePlayerDto.JerseyNumber > 99)
                    return BadRequest(new { success = false, message = "Jersey number must be between 1 and 99" });

                var player = await _playerService.UpdatePlayerAsync(id, updatePlayerDto);
                if (player == null)
                    return NotFound(new { success = false, message = "Player not found" });

                return Ok(new { success = true, data = player, message = "Player updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the player" });
            }
        }

        /// <summary>
        /// Delete player (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                var result = await _playerService.DeletePlayerAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Player not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the player" });
            }
        }
    }
}
