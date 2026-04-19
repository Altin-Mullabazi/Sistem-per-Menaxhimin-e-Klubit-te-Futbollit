using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
        Task<PlayerDto?> GetPlayerByIdAsync(int id);
        Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto);
        Task<PlayerDto?> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto);
        Task<bool> DeletePlayerAsync(int id);
    }

    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
        {
            var players = await _context.Players
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return players.Select(p => MapToDto(p)).ToList();
        }

        public async Task<PlayerDto?> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            return player == null ? null : MapToDto(player);
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
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}
