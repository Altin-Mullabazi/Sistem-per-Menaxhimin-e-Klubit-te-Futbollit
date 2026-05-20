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
        private readonly ILogger<TransfersController> _logger;
        private const int DefaultPageSize = 10;

        public TransfersController(ITransferService transferService, ILogger<TransfersController> logger)
        {
            _transferService = transferService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated transfers with filters
        /// Query Parameters:
        /// - page: Page number (default: 1)
        /// - pageSize: Items per page (default: 10, max: 100)
        /// - playerId: Filter by player ID (optional)
        /// - type: Filter by transfer type (optional)
        /// - fromDate: Filter transfers from this date (optional)
        /// - toDate: Filter transfers until this date (optional)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransfers(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = DefaultPageSize,
            [FromQuery] int? playerId = null, 
            [FromQuery] TransferType? type = null,
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Validate pagination
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "Invalid pagination: page >= 1, pageSize 1-100" 
                    });

                var (transfers, totalCount) = await _transferService.GetTransfersAsync(
                    page, pageSize, playerId, type, fromDate, toDate);

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
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while retrieving transfers" 
                });
            }
        }

        /// <summary>
        /// Get specific transfer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransferById(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "Transfer ID must be a positive number" 
                    });

                var transfer = await _transferService.GetTransferByIdAsync(id);
                if (transfer == null)
                    return NotFound(new 
                    { 
                        success = false, 
                        message = "Transfer not found" 
                    });

                return Ok(new 
                { 
                    success = true, 
                    data = transfer, 
                    message = "Transfer retrieved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving transfer: {ex.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while retrieving the transfer" 
                });
            }
        }

        /// <summary>
        /// Get all transfers for a specific player (sorted by date, newest first)
        /// </summary>
        [HttpGet("player/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransfersByPlayer(int playerId)
        {
            try
            {
                if (playerId < 1)
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "Player ID must be a positive number" 
                    });

                var transfers = await _transferService.GetTransfersByPlayerAsync(playerId);

                return Ok(new 
                { 
                    success = true, 
                    data = transfers, 
                    message = "Player transfers retrieved successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new 
                { 
                    success = false, 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving player transfers: {ex.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while retrieving player transfers" 
                });
            }
        }

        /// <summary>
        /// Create a new transfer
        /// Required: PlayerId, FromClubId, ToClubId, TransferDate, TransferFee, Type
        /// Validation: FromClubId ≠ ToClubId, TransferFee >= 0
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto createTransferDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var transfer = await _transferService.CreateTransferAsync(createTransferDto);

                return CreatedAtAction(
                    nameof(GetTransferById), 
                    new { id = transfer.Id }, 
                    new 
                    { 
                        success = true, 
                        data = transfer, 
                        message = "Transfer created successfully" 
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error creating transfer: {ex.Message}");
                return BadRequest(new 
                { 
                    success = false, 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating transfer: {ex.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while creating the transfer" 
                });
            }
        }

        /// <summary>
        /// Update transfer (TransferFee and Type only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateTransfer(int id, [FromBody] UpdateTransferDto updateTransferDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id < 1)
                return BadRequest(new 
                { 
                    success = false, 
                    message = "Transfer ID must be a positive number" 
                });

            try
            {
                var transfer = await _transferService.UpdateTransferAsync(
                    id, 
                    updateTransferDto.TransferFee, 
                    updateTransferDto.Type);

                if (transfer == null)
                    return NotFound(new 
                    { 
                        success = false, 
                        message = "Transfer not found" 
                    });

                return Ok(new 
                { 
                    success = true, 
                    data = transfer, 
                    message = "Transfer updated successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation error updating transfer: {ex.Message}");
                return BadRequest(new 
                { 
                    success = false, 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating transfer: {ex.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while updating the transfer" 
                });
            }
        }

        /// <summary>
        /// Delete a transfer (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTransfer(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new 
                    { 
                        success = false, 
                        message = "Transfer ID must be a positive number" 
                    });

                var deleted = await _transferService.DeleteTransferAsync(id);

                if (!deleted)
                    return NotFound(new 
                    { 
                        success = false, 
                        message = "Transfer not found" 
                    });

                return Ok(new 
                { 
                    success = true, 
                    message = "Transfer deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting transfer: {ex.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "An error occurred while deleting the transfer" 
                });
            }
        }
    }
}
