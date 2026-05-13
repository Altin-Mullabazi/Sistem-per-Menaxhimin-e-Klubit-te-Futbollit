using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeasonsController : ControllerBase
    {
        private readonly ISeasonService _seasonService;
        private readonly ILogger<SeasonsController> _logger;

        public SeasonsController(ISeasonService seasonService, ILogger<SeasonsController> logger)
        {
            _seasonService = seasonService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSeasons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest(new { success = false, message = "Page must be at least 1" });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { success = false, message = "PageSize must be between 1 and 100" });

            try
            {
                var (seasons, totalCount) = await _seasonService.GetSeasonsAsync(page, pageSize);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    success = true,
                    data = seasons,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages
                    },
                    message = "Seasons retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving seasons: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving seasons" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSeasonById(int id)
        {
            try
            {
                var season = await _seasonService.GetSeasonByIdAsync(id);
                if (season == null)
                    return NotFound(new { success = false, message = "Season not found" });

                return Ok(new { success = true, data = season, message = "Season retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving season {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the season" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSeason([FromBody] CreateSeasonDto createSeasonDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var season = await _seasonService.CreateSeasonAsync(createSeasonDto);
                return CreatedAtAction(nameof(GetSeasonById), new { id = season.Id }, new { success = true, data = season, message = "Season created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Validation failed creating season: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating season: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating the season" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSeason(int id, [FromBody] UpdateSeasonDto updateSeasonDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var season = await _seasonService.UpdateSeasonAsync(id, updateSeasonDto);
                if (season == null)
                    return NotFound(new { success = false, message = "Season not found" });

                return Ok(new { success = true, data = season, message = "Season updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Validation failed updating season {Id}: {Message}", id, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating season {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the season" });
            }
        }
    }
}
