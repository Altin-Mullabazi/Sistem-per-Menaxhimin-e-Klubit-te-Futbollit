using FootballClubAPI.DTOs;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballClubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
        {
            _contractService = contractService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated contracts with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContracts([FromQuery] ContractQueryParameters parameters)
        {
            try
            {
                var result = await _contractService.GetContractsAsync(parameters);
                return Ok(new { success = true, data = result, message = "Contracts retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving contracts: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving contracts" });
            }
        }

        /// <summary>
        /// Get contract by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContractById(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);
                if (contract == null)
                {
                    return NotFound(new { success = false, message = "Contract not found" });
                }
                return Ok(new { success = true, data = contract, message = "Contract retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving contract {id}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the contract" });
            }
        }

        /// <summary>
        /// Get active contracts
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveContracts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _contractService.GetActiveContractsAsync(page, pageSize);
                return Ok(new { success = true, data = result, message = "Active contracts retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving active contracts: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving active contracts" });
            }
        }

        /// <summary>
        /// Get expiring contracts within specified days
        /// </summary>
        [HttpGet("expiring")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 90, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _contractService.GetExpiringContractsAsync(days, page, pageSize);
                return Ok(new { success = true, data = result, message = "Expiring contracts retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving expiring contracts: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving expiring contracts" });
            }
        }

        /// <summary>
        /// Create a new contract
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractDto createContractDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid contract data", errors = ModelState });
                }

                // Additional validation
                if (createContractDto.StartDate >= createContractDto.EndDate)
                {
                    return BadRequest(new { success = false, message = "Start date must be before end date" });
                }

                if (createContractDto.Salary <= 0)
                {
                    return BadRequest(new { success = false, message = "Salary must be greater than 0" });
                }

                var contract = await _contractService.CreateContractAsync(createContractDto);
                return CreatedAtAction(nameof(GetContractById), new { id = contract.Id },
                    new { success = true, data = contract, message = "Contract created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating contract: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the contract" });
            }
        }

        /// <summary>
        /// Update an existing contract
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] UpdateContractDto updateContractDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid contract data", errors = ModelState });
                }

                // Additional validation
                if (updateContractDto.Salary <= 0)
                {
                    return BadRequest(new { success = false, message = "Salary must be greater than 0" });
                }

                var contract = await _contractService.UpdateContractAsync(id, updateContractDto);
                if (contract == null)
                {
                    return NotFound(new { success = false, message = "Contract not found" });
                }

                return Ok(new { success = true, data = contract, message = "Contract updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating contract {id}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the contract" });
            }
        }

        /// <summary>
        /// Delete a contract
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var result = await _contractService.DeleteContractAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Contract not found" });
                }

                return Ok(new { success = true, message = "Contract deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting contract {id}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the contract" });
            }
        }
    }
}