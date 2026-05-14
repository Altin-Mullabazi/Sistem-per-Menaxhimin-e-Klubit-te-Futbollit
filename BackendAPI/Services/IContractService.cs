using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface IContractService
    {
        Task<PaginatedResponse<ContractDto>> GetContractsAsync(ContractQueryParameters parameters);
        Task<ContractDto?> GetContractByIdAsync(int id);
        Task<PaginatedResponse<ContractDto>> GetActiveContractsAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponse<ContractDto>> GetExpiringContractsAsync(int days, int page = 1, int pageSize = 10);
        Task<ContractDto> CreateContractAsync(CreateContractDto createContractDto);
        Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto updateContractDto);
        Task<bool> DeleteContractAsync(int id);
    }
}