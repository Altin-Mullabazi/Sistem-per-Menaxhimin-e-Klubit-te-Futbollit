using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FootballClubAPI.Services
{
    public interface ITransferService
    {
        Task<(IEnumerable<TransferDto> Transfers, int TotalCount)> GetTransfersAsync(int page, int pageSize, int? playerId, TransferType? type, DateTime? fromDate, DateTime? toDate);
        Task<TransferDto?> GetTransferByIdAsync(int id);
        Task<IEnumerable<TransferDto>> GetTransfersByPlayerAsync(int playerId);
        Task<TransferDto> CreateTransferAsync(CreateTransferDto createTransferDto);
        Task<TransferDto?> UpdateTransferAsync(int id, decimal transferFee, TransferType type);
        Task<bool> DeleteTransferAsync(int id);
    }

    public class TransferService : ITransferService
    {
        private const int DefaultPageSize = 10;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransferService> _logger;

        public TransferService(ApplicationDbContext context, ILogger<TransferService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(IEnumerable<TransferDto> Transfers, int TotalCount)> GetTransfersAsync(int page, int pageSize, int? playerId, TransferType? type, DateTime? fromDate, DateTime? toDate)
        {
            _logger.LogInformation(
                $"Retrieving transfers: page={page}, pageSize={pageSize}, playerId={playerId}, type={type}");

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .AsQueryable();


            if (playerId.HasValue)
            {
                query = query.Where(t => t.PlayerId == playerId.Value);
                _logger.LogInformation($"Filter by player: {playerId}");
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
                _logger.LogInformation($"Filter by type: {type}");
            }

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransferDate >= fromDate.Value.Date);
                _logger.LogInformation($"Filter from date: {fromDate}");
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.TransferDate <= toDate.Value.Date);
                _logger.LogInformation($"Filter to date: {toDate}");
            }

            var totalCount = await query.CountAsync();

            var transfers = await query
                .OrderByDescending(t => t.TransferDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToDto(t))
                .ToListAsync();

            _logger.LogInformation(
                $"Retrieved {transfers.Count} transfers. Total: {totalCount}, Pages: {(int)Math.Ceiling((double)totalCount / pageSize)}");

            return (transfers, totalCount);
        }

        public async Task<TransferDto?> GetTransferByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving transfer: ID={id}");

            var transfer = await _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
            {
                _logger.LogWarning($"Transfer not found: ID={id}");
                return null;
            }

            _logger.LogInformation($"Transfer retrieved: {transfer.Id}");
            return MapToDto(transfer);
        }

        public async Task<IEnumerable<TransferDto>> GetTransfersByPlayerAsync(int playerId)
        {
            _logger.LogInformation($"Retrieving transfers for player: {playerId}");

            var playerExists = await _context.Players.AnyAsync(p => p.Id == playerId);
            if (!playerExists)
            {
                _logger.LogWarning($"Player not found: {playerId}");
                throw new InvalidOperationException("Player not found");
            }

            var transfers = await _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.TransferDate)
                .Select(t => MapToDto(t))
                .ToListAsync();

            _logger.LogInformation($"Retrieved {transfers.Count} transfers for player {playerId}");
            return transfers;
        }

        public async Task<TransferDto> CreateTransferAsync(CreateTransferDto createTransferDto)
        {
            _logger.LogInformation($"Creating transfer for player {createTransferDto.PlayerId}");

            // ✅ Validation: Different clubs
            if (createTransferDto.FromClubId == createTransferDto.ToClubId)
            {
                _logger.LogWarning("Transfer rejected: FromClub and ToClub are the same");
                throw new InvalidOperationException("FromClubId and ToClubId must be different");
            }

            // ✅ Validation: Transfer fee >= 0
            if (createTransferDto.TransferFee < 0)
            {
                _logger.LogWarning($"Transfer rejected: Negative fee {createTransferDto.TransferFee}");
                throw new InvalidOperationException("Transfer fee must be greater than or equal to 0");
            }

            // ✅ Validation: Player exists
            var player = await _context.Players.FindAsync(createTransferDto.PlayerId);
            if (player == null)
            {
                _logger.LogWarning($"Transfer rejected: Player not found {createTransferDto.PlayerId}");
                throw new InvalidOperationException("Player not found");
            }

            // ✅ Validation: FromClub exists
            var fromClub = await _context.Clubs.FindAsync(createTransferDto.FromClubId);
            if (fromClub == null)
            {
                _logger.LogWarning($"Transfer rejected: FromClub not found {createTransferDto.FromClubId}");
                throw new InvalidOperationException("FromClub not found");
            }

            // ✅ Validation: ToClub exists
            var toClub = await _context.Clubs.FindAsync(createTransferDto.ToClubId);
            if (toClub == null)
            {
                _logger.LogWarning($"Transfer rejected: ToClub not found {createTransferDto.ToClubId}");
                throw new InvalidOperationException("ToClub not found");
            }

            var transfer = new Transfer
            {
                PlayerId = createTransferDto.PlayerId,
                FromClubId = createTransferDto.FromClubId,
                ToClubId = createTransferDto.ToClubId,
                TransferDate = createTransferDto.TransferDate,
                TransferFee = createTransferDto.TransferFee,
                Type = createTransferDto.Type,
                Status = TransferStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Transfer created successfully: {transfer.Id}");
            return await GetTransferByIdAsync(transfer.Id) ?? 
                throw new InvalidOperationException("Transfer was created but could not be loaded.");
        }

        public async Task<TransferDto?> UpdateTransferAsync(int id, decimal transferFee, TransferType type)
        {
            _logger.LogInformation($"Updating transfer: ID={id}");

            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
            {
                _logger.LogWarning($"Transfer not found for update: ID={id}");
                return null;
            }

            // ✅ Validation: Fee >= 0
            if (transferFee < 0)
            {
                _logger.LogWarning($"Update rejected: Negative fee {transferFee}");
                throw new InvalidOperationException("Transfer fee must be greater than or equal to 0");
            }

            transfer.TransferFee = transferFee;
            transfer.Type = type;
            transfer.UpdatedAt = DateTime.UtcNow;

            _context.Transfers.Update(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Transfer updated successfully: {transfer.Id}");
            return await GetTransferByIdAsync(transfer.Id);
        }

        public async Task<bool> DeleteTransferAsync(int id)
        {
            _logger.LogInformation($"Deleting transfer: ID={id}");

            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
            {
                _logger.LogWarning($"Transfer not found for deletion: ID={id}");
                return false;
            }

            _context.Transfers.Remove(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Transfer deleted successfully: {transfer.Id}");
            return true;
        }

        private static TransferDto MapToDto(Transfer transfer)
        {
            return new TransferDto
            {
                Id = transfer.Id,
                PlayerId = transfer.PlayerId,
                PlayerName = transfer.Player != null ? $"{transfer.Player.FirstName} {transfer.Player.LastName}" : string.Empty,
                FromClubId = transfer.FromClubId,
                FromClubName = transfer.FromClub?.Name ?? string.Empty,
                ToClubId = transfer.ToClubId,
                ToClubName = transfer.ToClub?.Name ?? string.Empty,
                TransferDate = transfer.TransferDate,
                TransferFee = transfer.TransferFee,
                Type = transfer.Type,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt,
                UpdatedAt = transfer.UpdatedAt
            };
        }
    }
}
