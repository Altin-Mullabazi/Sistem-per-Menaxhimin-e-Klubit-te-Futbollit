using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransfersController> _logger;
        private const int DefaultPageSize = 10;

        public TransfersController(ITransferService transferService, ApplicationDbContext context, ILogger<TransfersController> logger)
        {
            _transferService = transferService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransfers([FromQuery] int page = 1, [FromQuery] int pageSize = DefaultPageSize,
            [FromQuery] int? playerId = null, [FromQuery] TransferType? type = null,
            [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var (transfers, totalCount) = await _transferService.GetTransfersAsync(page, pageSize, playerId, type, fromDate, toDate);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    success = true,
                    data = transfers,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages
                    },
                    message = "Transfers retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving transfers: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving transfers" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransferById(int id)
        {
            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id);
                if (transfer == null)
                    return NotFound(new { success = false, message = "Transfer not found" });

                return Ok(new { success = true, data = transfer, message = "Transfer retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving transfer: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the transfer" });
            }
        }

        [HttpGet("player/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransfersByPlayer(int playerId)
        {
            try
            {
                var transfers = await _transferService.GetTransfersByPlayerAsync(playerId);
                return Ok(new { success = true, data = transfers, message = "Player transfers retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving player transfers: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving player transfers" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto createTransferDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createTransferDto.FromClubId == createTransferDto.ToClubId)
                return BadRequest(new { success = false, message = "FromClubId and ToClubId must be different" });

            if (createTransferDto.TransferFee < 0)
                return BadRequest(new { success = false, message = "Transfer fee must be greater than or equal to 0" });

            var playerExists = await _context.Players.AnyAsync(p => p.Id == createTransferDto.PlayerId);
            var fromClubExists = await _context.Clubs.AnyAsync(c => c.Id == createTransferDto.FromClubId);
            var toClubExists = await _context.Clubs.AnyAsync(c => c.Id == createTransferDto.ToClubId);

            if (!playerExists || !fromClubExists || !toClubExists)
                return BadRequest(new { success = false, message = "Player and clubs must exist" });

            try
            {
                var transfer = await _transferService.CreateTransferAsync(createTransferDto);
                return CreatedAtAction(nameof(GetTransferById), new { id = transfer.Id }, new { success = true, data = transfer, message = "Transfer created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating transfer: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the transfer" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTransfer(int id, [FromBody] UpdateTransferDto updateTransferDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (updateTransferDto.TransferFee < 0)
                return BadRequest(new { success = false, message = "Transfer fee must be greater than or equal to 0" });

            try
            {
                var transfer = await _transferService.UpdateTransferAsync(id, updateTransferDto.TransferFee, updateTransferDto.Type);
                if (transfer == null)
                    return NotFound(new { success = false, message = "Transfer not found" });

                return Ok(new { success = true, data = transfer, message = "Transfer updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating transfer: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the transfer" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTransfer(int id)
        {
            try
            {
                var deleted = await _transferService.DeleteTransferAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Transfer not found" });

                return Ok(new { success = true, message = "Transfer deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting transfer: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the transfer" });
            }
        }
    }
}
