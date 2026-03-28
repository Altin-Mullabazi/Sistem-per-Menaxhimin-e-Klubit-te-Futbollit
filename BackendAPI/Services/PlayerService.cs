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
                Age = createPlayerDto.Age,
                Position = createPlayerDto.Position,
                ClubName = createPlayerDto.ClubName,
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
            player.Age = updatePlayerDto.Age;
            player.Position = updatePlayerDto.Position;
            player.ClubName = updatePlayerDto.ClubName;
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
                Age = player.Age,
                Position = player.Position,
                ClubName = player.ClubName,
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}
