using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchEventsController : ControllerBase
    {
        private readonly IMatchEventService _matchEventService;
        private readonly ILogger<MatchEventsController> _logger;

        public MatchEventsController(IMatchEventService matchEventService, ILogger<MatchEventsController> logger)
        {
            _matchEventService = matchEventService;
            _logger = logger;
        }

        /// <summary>
        /// Get events for a specific player in a match
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="matchId">Match ID (required query parameter)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet("player/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPlayerEvents(int playerId, int matchId, int page = 1, int pageSize = 10)
        {
            if (matchId <= 0)
            {
                return BadRequest(new { success = false, message = "matchId query parameter is required and must be greater than 0" });
            }

            try
            {
                var (events, totalCount) = await _matchEventService.GetPlayerEventsAsync(playerId, matchId, page, pageSize);
                return Ok(new
                {
                    success = true,
                    data = events,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    message = "Player events retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving player events: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving player events" });
            }
        }

        /// <summary>
        /// Get specific match event by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatchEventById(int id)
        {
            try
            {
                var matchEvent = await _matchEventService.GetMatchEventByIdAsync(id);
                if (matchEvent == null)
                    return NotFound(new { success = false, message = "Match event not found" });

                return Ok(new { success = true, data = matchEvent, message = "Match event retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving match event: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the match event" });
            }
        }

        /// <summary>
        /// Get list of events for a match, paginated and sorted by minute
        /// </summary>
        /// <param name="matchId">Match ID (required query parameter)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMatchEvents(int matchId, int page = 1, int pageSize = 10)
        {
            if (matchId <= 0)
            {
                return BadRequest(new { success = false, message = "matchId query parameter is required and must be greater than 0" });
            }

            try
            {
                var (events, totalCount) = await _matchEventService.GetMatchEventsAsync(matchId, page, pageSize);
                return Ok(new
                {
                    success = true,
                    data = events,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    message = "Match events retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving match events: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving match events" });
            }
        }

        /// <summary>
        /// Create a new match event
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMatchEvent([FromBody] CreateMatchEventDto createEventDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var matchEvent = await _matchEventService.CreateMatchEventAsync(createEventDto);
                return CreatedAtAction(nameof(GetMatchEventById), new { id = matchEvent.Id }, new { success = true, data = matchEvent, message = "Match event created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error creating match event: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating match event: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the match event" });
            }
        }

        /// <summary>
        /// Update a match event
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMatchEvent(int id, [FromBody] UpdateMatchEventDto updateEventDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var matchEvent = await _matchEventService.UpdateMatchEventAsync(id, updateEventDto);
                if (matchEvent == null)
                    return NotFound(new { success = false, message = "Match event not found" });

                return Ok(new { success = true, data = matchEvent, message = "Match event updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error updating match event: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating match event: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the match event" });
            }
        }

        /// <summary>
        /// Delete a match event
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMatchEvent(int id)
        {
            try
            {
                var result = await _matchEventService.DeleteMatchEventAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Match event not found" });

                return Ok(new { success = true, message = "Match event deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting match event: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the match event" });
            }
        }
    }
}
