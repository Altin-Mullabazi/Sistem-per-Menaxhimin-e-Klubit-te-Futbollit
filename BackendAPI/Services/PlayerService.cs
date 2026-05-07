using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IPlayerService
    {
        // 1. Get paginated players with filters, search, and sorting
        Task<(IEnumerable<PlayerDto> players, int totalCount)> GetPlayersWithPaginationAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchName = null, 
            int? clubId = null, 
            string? position = null, 
            string? sortBy = "createdAt");

        // 2. Get player by ID with detailed info
        Task<PlayerDetailDto?> GetPlayerByIdAsync(int id);

        // 3. Search players with advanced filters
        Task<IEnumerable<PlayerDto>> SearchPlayersAsync(string? name = null, int? clubId = null, string? position = null);

        // 4. Get all players in a club
        Task<IEnumerable<PlayerDto>> GetPlayersByClubAsync(int clubId);

        // 5. Create player
        Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto);

        // 6. Update player
        Task<PlayerDto?> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto);

        // 7. Delete player
        Task<bool> DeletePlayerAsync(int id);
    }

    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<PlayerDto> players, int totalCount)> GetPlayersWithPaginationAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchName = null, 
            int? clubId = null, 
            string? position = null, 
            string? sortBy = "createdAt")
        {
            var query = _context.Players.Include(p => p.Club).AsQueryable();

            // Apply filters
            if (clubId.HasValue)
                query = query.Where(p => p.ClubId == clubId);

            if (!string.IsNullOrEmpty(position))
                query = query.Where(p => p.Position.ToLower() == position.ToLower());

            // Apply search
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.FirstName.ToLower().Contains(searchName.ToLower()) || 
                                         p.LastName.ToLower().Contains(searchName.ToLower()));

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "jersey" => query.OrderBy(p => p.JerseyNumber),
                "name" => query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName),
                "position" => query.OrderBy(p => p.Position),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (players.Select(MapToDto).ToList(), totalCount);
        }

        public async Task<PlayerDetailDto?> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.Club)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return null;

            // Get related data
            var contracts = await _context.Contracts
                .Where(c => c.PlayerId == id)
                .ToListAsync();

            var transfers = await _context.Transfers
                .Where(t => t.PlayerId == id)
                .ToListAsync();

            var stats = await _context.PlayerStats
                .FirstOrDefaultAsync(s => s.PlayerId == id);

            return MapToDetailDto(player, contracts, transfers, stats);
        }

        public async Task<IEnumerable<PlayerDto>> SearchPlayersAsync(string? name = null, int? clubId = null, string? position = null)
        {
            var query = _context.Players.Include(p => p.Club).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.FirstName.ToLower().Contains(name.ToLower()) || 
                                         p.LastName.ToLower().Contains(name.ToLower()));

            if (clubId.HasValue)
                query = query.Where(p => p.ClubId == clubId);

            if (!string.IsNullOrEmpty(position))
                query = query.Where(p => p.Position.ToLower() == position.ToLower());

            var players = await query.OrderBy(p => p.FirstName).ToListAsync();
            return players.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<PlayerDto>> GetPlayersByClubAsync(int clubId)
        {
            var players = await _context.Players
                .Where(p => p.ClubId == clubId)
                .Include(p => p.Club)
                .OrderBy(p => p.JerseyNumber)
                .ToListAsync();

            return players.Select(MapToDto).ToList();
        }

        public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto)
        {
            var player = new Player
            {
                FirstName = createPlayerDto.FirstName,
                LastName = createPlayerDto.LastName,
                JerseyNumber = createPlayerDto.JerseyNumber,
                Position = createPlayerDto.Position,
                DateOfBirth = createPlayerDto.DateOfBirth,
                Nationality = createPlayerDto.Nationality,
                Height = createPlayerDto.Height,
                Weight = createPlayerDto.Weight,
                Status = string.IsNullOrEmpty(createPlayerDto.Status) ? null : Enum.Parse<PlayerStatus>(createPlayerDto.Status),
                MarketValue = createPlayerDto.MarketValue,
                ClubId = createPlayerDto.ClubId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return MapToDto(player);
        }

        public async Task<PlayerDto?> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return null;

            player.FirstName = updatePlayerDto.FirstName;
            player.LastName = updatePlayerDto.LastName;
            player.JerseyNumber = updatePlayerDto.JerseyNumber;
            player.Position = updatePlayerDto.Position;
            player.DateOfBirth = updatePlayerDto.DateOfBirth;
            player.Nationality = updatePlayerDto.Nationality;
            player.Height = updatePlayerDto.Height;
            player.Weight = updatePlayerDto.Weight;
            player.Status = string.IsNullOrEmpty(updatePlayerDto.Status) ? null : Enum.Parse<PlayerStatus>(updatePlayerDto.Status);
            player.MarketValue = updatePlayerDto.MarketValue;
            player.ClubId = updatePlayerDto.ClubId;
            player.UpdatedAt = DateTime.UtcNow;

            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return MapToDto(player);
        }

        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return false;

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return true;
        }

        private static PlayerDto MapToDto(Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                JerseyNumber = player.JerseyNumber,
                Position = player.Position,
                DateOfBirth = player.DateOfBirth,
                Nationality = player.Nationality,
                Height = player.Height,
                Weight = player.Weight,
                Status = player.Status?.ToString(),
                MarketValue = player.MarketValue,
                ClubId = player.ClubId,
                Club = player.Club == null ? null : new ClubDto
                {
                    Id = player.Club.Id,
                    Name = player.Club.Name,
                    City = player.Club.City,
                    FoundedYear = player.Club.FoundedYear
                },
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }

        private static PlayerDetailDto MapToDetailDto(Player player, List<Contract> contracts, List<Transfer> transfers, PlayerStats? stats)
        {
            return new PlayerDetailDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                JerseyNumber = player.JerseyNumber,
                Position = player.Position,
                DateOfBirth = player.DateOfBirth,
                Nationality = player.Nationality,
                Height = player.Height,
                Weight = player.Weight,
                Status = player.Status?.ToString(),
                MarketValue = player.MarketValue,
                ClubId = player.ClubId,
                Club = player.Club == null ? null : new ClubDto
                {
                    Id = player.Club.Id,
                    Name = player.Club.Name,
                    City = player.Club.City,
                    FoundedYear = player.Club.FoundedYear
                },
                Contracts = contracts.Select(c => new ContractDto
                {
                    Id = c.Id,
                    PlayerId = c.PlayerId,
                    ClubId = c.ClubId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Salary = c.Salary
                }).ToList(),
                Transfers = transfers.Select(t => new TransferDto
                {
                    Id = t.Id,
                    PlayerId = t.PlayerId,
                    FromClubId = t.FromClubId,
                    ToClubId = t.ToClubId,
                    TransferDate = t.TransferDate,
                    TransferFee = t.TransferFee
                }).ToList(),
                Stats = stats == null ? null : new PlayerStatsDto
                {
                    Id = stats.Id,
                    PlayerId = stats.PlayerId,
                    MatchId = stats.MatchId,
                    MinutesPlayed = stats.MinutesPlayed,
                    GoalsScored = stats.GoalsScored,
                    Assists = stats.Assists,
                    YellowCards = stats.YellowCards,
                    RedCards = stats.RedCards,
                    Rating = stats.Rating
                },
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}
