using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Return dashboard summary statistics.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _dashboardService.GetSummaryAsync();
                return Ok(new { success = true, data = summary, message = "Dashboard summary retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard summary");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving dashboard summary" });
            }
        }

        /// <summary>
        /// Return top scorers ordered by goals and assists.
        /// </summary>
        [HttpGet("top-scorers")]
        [ProducesResponseType(typeof(IEnumerable<DashboardTopScorerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopScorers([FromQuery] int limit = 10)
        {
            try
            {
                var scorers = await _dashboardService.GetTopScorersAsync(limit);
                return Ok(new { success = true, data = scorers, message = "Top scorers retrieved successfully" });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top scorers");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving top scorers" });
            }
        }

        /// <summary>
        /// Return matches scheduled in the next number of days.
        /// </summary>
        [HttpGet("upcoming-matches")]
        [ProducesResponseType(typeof(IEnumerable<DashboardUpcomingMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUpcomingMatches([FromQuery] int days = 7)
        {
            try
            {
                var matches = await _dashboardService.GetUpcomingMatchesAsync(days);
                return Ok(new { success = true, data = matches, message = "Upcoming matches retrieved successfully" });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming matches");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving upcoming matches" });
            }
        }

        /// <summary>
        /// Return players with active injuries.
        /// </summary>
        [HttpGet("injured-players")]
        [ProducesResponseType(typeof(IEnumerable<DashboardInjuredPlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInjuredPlayers()
        {
            try
            {
                var injuredPlayers = await _dashboardService.GetInjuredPlayersAsync();
                return Ok(new { success = true, data = injuredPlayers, message = "Injured players retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving injured players");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving injured players" });
            }
        }

        /// <summary>
        /// Return active contracts expiring soon.
        /// </summary>
        [HttpGet("expiring-contracts")]
        [ProducesResponseType(typeof(IEnumerable<DashboardExpiringContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 90)
        {
            try
            {
                var contracts = await _dashboardService.GetExpiringContractsAsync(days);
                return Ok(new { success = true, data = contracts, message = "Expiring contracts retrieved successfully" });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring contracts");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving expiring contracts" });
            }
        }

        /// <summary>
        /// Return recent transfers.
        /// </summary>
        [HttpGet("recent-transfers")]
        [ProducesResponseType(typeof(IEnumerable<DashboardRecentTransferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecentTransfers([FromQuery] int days = 30)
        {
            try
            {
                var transfers = await _dashboardService.GetRecentTransfersAsync(days);
                return Ok(new { success = true, data = transfers, message = "Recent transfers retrieved successfully" });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transfers");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving recent transfers" });
            }
        }
    }
}
