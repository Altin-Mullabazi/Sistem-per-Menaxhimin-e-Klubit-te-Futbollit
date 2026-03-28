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
        /// Get all players
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPlayers()
        {
            try
            {
                var players = await _playerService.GetAllPlayersAsync();
                return Ok(new { success = true, data = players, message = "Players retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving players: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving players" });
            }
        }

        /// <summary>
        /// Get player by ID
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
        /// Create a new player
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var player = await _playerService.CreatePlayerAsync(createPlayerDto);
                return CreatedAtAction(nameof(GetPlayerById), new { id = player.Id }, new { success = true, data = player, message = "Player created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the player" });
            }
        }

        /// <summary>
        /// Update player
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlayer(int id, [FromBody] UpdatePlayerDto updatePlayerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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
        /// Delete player
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                var result = await _playerService.DeletePlayerAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Player not found" });

                return Ok(new { success = true, message = "Player deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting player: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the player" });
            }
        }
    }
}
