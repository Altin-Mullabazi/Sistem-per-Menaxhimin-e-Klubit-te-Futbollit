using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

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

        public TransferService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<TransferDto> Transfers, int TotalCount)> GetTransfersAsync(int page, int pageSize, int? playerId, TransferType? type, DateTime? fromDate, DateTime? toDate)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;

            var query = _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .AsQueryable();

            if (playerId.HasValue)
                query = query.Where(t => t.PlayerId == playerId.Value);

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransferDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(t => t.TransferDate <= toDate.Value.Date);

            var totalCount = await query.CountAsync();

            var transfers = await query
                .OrderByDescending(t => t.TransferDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToDto(t))
                .ToListAsync();

            return (transfers, totalCount);
        }

        public async Task<TransferDto?> GetTransferByIdAsync(int id)
        {
            var transfer = await _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .FirstOrDefaultAsync(t => t.Id == id);

            return transfer == null ? null : MapToDto(transfer);
        }

        public async Task<IEnumerable<TransferDto>> GetTransfersByPlayerAsync(int playerId)
        {
            var transfers = await _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromClub)
                .Include(t => t.ToClub)
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.TransferDate)
                .Select(t => MapToDto(t))
                .ToListAsync();

            return transfers;
        }

        public async Task<TransferDto> CreateTransferAsync(CreateTransferDto createTransferDto)
        {
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

            return await GetTransferByIdAsync(transfer.Id) ?? throw new InvalidOperationException("Transfer was created but could not be loaded.");
        }

        public async Task<TransferDto?> UpdateTransferAsync(int id, decimal transferFee, TransferType type)
        {
            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
                return null;

            transfer.TransferFee = transferFee;
            transfer.Type = type;
            transfer.UpdatedAt = DateTime.UtcNow;

            _context.Transfers.Update(transfer);
            await _context.SaveChangesAsync();

            return await GetTransferByIdAsync(transfer.Id);
        }

        public async Task<bool> DeleteTransferAsync(int id)
        {
            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
                return false;

            _context.Transfers.Remove(transfer);
            await _context.SaveChangesAsync();
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
