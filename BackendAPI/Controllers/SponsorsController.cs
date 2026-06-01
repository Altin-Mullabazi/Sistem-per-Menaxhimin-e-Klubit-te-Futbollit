using FootballClubAPI.DTOs;
using FootballClubAPI.Data;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorsController : ControllerBase
    {
        private readonly ISponsorService _sponsorService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SponsorsController> _logger;

        public SponsorsController(
            ISponsorService sponsorService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<SponsorsController> logger)
        {
            _sponsorService = sponsorService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all sponsors with pagination
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSponsors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _sponsorService.GetSponsorsAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Sponsors retrieved successfully",
                    pagination = new
                    {
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalCount = result.TotalCount,
                        totalPages = result.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving sponsors: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving sponsors" });
            }
        }

        /// <summary>
        /// Get sponsor by ID with clubs
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSponsorById(int id)
        {
            try
            {
                var sponsor = await _sponsorService.GetSponsorByIdAsync(id);
                if (sponsor == null)
                    return NotFound(new { success = false, message = "Sponsor not found" });

                return Ok(new { success = true, data = sponsor, message = "Sponsor retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving sponsor {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the sponsor" });
            }
        }

        /// <summary>
        /// Create a new sponsor
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSponsor([FromBody] CreateSponsorDto createSponsorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var legacyUserId = await ResolveLegacyUserIdAsync(currentUserId);
                var sponsor = await _sponsorService.CreateSponsorAsync(createSponsorDto, legacyUserId);
                return CreatedAtAction(nameof(GetSponsorById), new { id = sponsor.Id }, new { success = true, data = sponsor, message = "Sponsor created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating sponsor: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating the sponsor" });
            }
        }

        /// <summary>
        /// Update sponsor
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSponsor(int id, [FromBody] UpdateSponsorDto updateSponsorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });

            try
            {
                var sponsor = await _sponsorService.UpdateSponsorAsync(id, updateSponsorDto);
                if (sponsor == null)
                    return NotFound(new { success = false, message = "Sponsor not found" });

                return Ok(new { success = true, data = sponsor, message = "Sponsor updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating sponsor {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the sponsor" });
            }
        }

        /// <summary>
        /// Delete sponsor
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSponsor(int id)
        {
            try
            {
                var result = await _sponsorService.DeleteSponsorAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Sponsor not found" });

                return Ok(new { success = true, message = "Sponsor deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting sponsor {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the sponsor" });
            }
        }

        private async Task<string?> ResolveLegacyUserIdAsync(string? currentUserId)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return await _context.LegacyUsers.Select(user => user.Id).FirstOrDefaultAsync();
            }

            var applicationUser = await _userManager.FindByIdAsync(currentUserId);
            if (applicationUser != null)
            {
                var legacyUser = await _context.LegacyUsers.FirstOrDefaultAsync(user =>
                    user.Id == applicationUser.Id ||
                    user.Email == applicationUser.Email ||
                    user.Username == applicationUser.UserName);

                if (legacyUser != null)
                {
                    return legacyUser.Id;
                }
            }

            return await _context.LegacyUsers.Select(user => user.Id).FirstOrDefaultAsync();
        }
    }
}