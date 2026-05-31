using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrainingSessionsController : ControllerBase
    {
        private readonly ITrainingService _trainingService;
        private readonly ILogger<TrainingSessionsController> _logger;

        public TrainingSessionsController(ITrainingService trainingService, ILogger<TrainingSessionsController> logger)
        {
            _trainingService = trainingService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrainingSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? clubId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { success = false, message = "Invalid pagination: page >= 1, pageSize 1-100" });
            }

            try
            {
                var (sessions, totalCount) = await _trainingService.GetTrainingSessionsAsync(page, pageSize, clubId, startDate, endDate);
                return Ok(new
                {
                    success = true,
                    data = sessions,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    message = "Training sessions retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving training sessions: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving training sessions" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrainingSessionById(int id)
        {
            try
            {
                var session = await _trainingService.GetTrainingSessionByIdAsync(id);
                if (session == null)
                    return NotFound(new { success = false, message = "Training session not found" });

                return Ok(new { success = true, data = session, message = "Training session retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving training session {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the training session" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTrainingSession([FromBody] CreateTrainingSessionDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var session = await _trainingService.CreateTrainingSessionAsync(createDto);
                return CreatedAtAction(nameof(GetTrainingSessionById), new { id = session.Id }, new { success = true, data = session, message = "Training session created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Validation failed creating training session: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating training session: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating the training session" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTrainingSession(int id, [FromBody] UpdateTrainingSessionDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var session = await _trainingService.UpdateTrainingSessionAsync(id, updateDto);
                if (session == null)
                    return NotFound(new { success = false, message = "Training session not found" });

                return Ok(new { success = true, data = session, message = "Training session updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Validation failed updating training session {Id}: {Message}", id, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating training session {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the training session" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTrainingSession(int id)
        {
            try
            {
                var deleted = await _trainingService.DeleteTrainingSessionAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Training session not found" });

                return Ok(new { success = true, message = "Training session deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting training session {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the training session" });
            }
        }
    }
}
