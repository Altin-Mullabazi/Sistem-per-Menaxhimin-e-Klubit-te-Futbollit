using Xunit;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FootballClubAPI.Tests.Services
{
    public class InjuryServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IInjuryService _injuryService;

        public InjuryServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _injuryService = new InjuryService(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test players
            var player1 = new Player
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                JerseyNumber = 10,
                Position = "Forward",
                DateOfBirth = new DateTime(1995, 1, 1),
                Nationality = "USA"
            };

            var player2 = new Player
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                JerseyNumber = 5,
                Position = "Defender",
                DateOfBirth = new DateTime(1996, 2, 2),
                Nationality = "UK"
            };

            _context.Players.Add(player1);
            _context.Players.Add(player2);
            _context.SaveChanges();

            // Add test injuries
            var injury1 = new Injury
            {
                PlayerId = 1,
                InjuryType = "Muscle Strain",
                InjuryDate = DateTime.UtcNow.Date.AddDays(-10),
                Status = InjuryStatus.Active,
                Notes = "Right hamstring"
            };

            var injury2 = new Injury
            {
                PlayerId = 1,
                InjuryType = "ACL Tear",
                InjuryDate = DateTime.UtcNow.Date.AddDays(-30),
                RecoveryDate = DateTime.UtcNow.Date.AddDays(-5),
                Status = InjuryStatus.Recovered,
                Notes = "Left knee - surgery completed"
            };

            var injury3 = new Injury
            {
                PlayerId = 2,
                InjuryType = "Sprain",
                InjuryDate = DateTime.UtcNow.Date.AddDays(-5),
                Status = InjuryStatus.Recovering,
                Notes = "Ankle sprain"
            };

            _context.Injuries.Add(injury1);
            _context.Injuries.Add(injury2);
            _context.Injuries.Add(injury3);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetInjuriesAsync_ReturnsAllInjuries_WithoutFilters()
        {
            // Act
            var result = await _injuryService.GetInjuriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public async Task GetInjuriesAsync_FiltersByPlayerId()
        {
            // Act
            var result = await _injuryService.GetInjuriesAsync(playerId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, injury => Assert.Equal(1, injury.PlayerId));
        }

        [Fact]
        public async Task GetInjuriesAsync_FiltersByStatus()
        {
            // Act
            var result = await _injuryService.GetInjuriesAsync(status: "Active");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Active", result.Data[0].Status);
        }

        [Fact]
        public async Task GetInjuriesAsync_SortsByDateNewestFirst()
        {
            // Act
            var result = await _injuryService.GetInjuriesAsync(sortBy: "date");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            // Most recent injury should be first
            Assert.True(result.Data[0].InjuryDate >= result.Data[1].InjuryDate);
        }

        [Fact]
        public async Task GetInjuriesAsync_Paginates()
        {
            // Act
            var page1 = await _injuryService.GetInjuriesAsync(page: 1, pageSize: 2);
            var page2 = await _injuryService.GetInjuriesAsync(page: 2, pageSize: 2);

            // Assert
            Assert.Equal(2, page1.Data.Count);
            Assert.Single(page2.Data);
            Assert.Equal(3, page1.TotalItems);
            Assert.Equal(2, page1.TotalPages);
        }

        [Fact]
        public async Task GetActiveInjuriesAsync_ReturnsOnlyActiveInjuries()
        {
            // Act
            var result = await _injuryService.GetActiveInjuriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count); // Active and Recovering
            Assert.All(result.Data, injury => Assert.True(
                injury.Status == "Active" || injury.Status == "Recovering"
            ));
        }

        [Fact]
        public async Task GetInjuryByIdAsync_ReturnsInjury()
        {
            // Act
            var result = await _injuryService.GetInjuryByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.PlayerId);
            Assert.Equal("John Doe", result.PlayerName);
        }

        [Fact]
        public async Task GetInjuryByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var result = await _injuryService.GetInjuryByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateInjuryAsync_CreatesNewInjury()
        {
            // Arrange
            var createDto = new CreateInjuryDto
            {
                PlayerId = 1,
                InjuryType = "Concussion",
                InjuryDate = DateTime.UtcNow.Date,
                Notes = "Head injury"
            };

            // Act
            var result = await _injuryService.CreateInjuryAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(1, result.PlayerId);
            Assert.Equal("Concussion", result.InjuryType);
            Assert.Equal("Active", result.Status);
            Assert.Equal("John Doe", result.PlayerName);
        }

        [Fact]
        public async Task CreateInjuryAsync_ThrowsException_WhenPlayerNotFound()
        {
            // Arrange
            var createDto = new CreateInjuryDto
            {
                PlayerId = 999,
                InjuryType = "Injury",
                InjuryDate = DateTime.UtcNow.Date
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _injuryService.CreateInjuryAsync(createDto)
            );
        }

        [Fact]
        public async Task CreateInjuryAsync_ThrowsException_WhenInjuryDateInFuture()
        {
            // Arrange
            var createDto = new CreateInjuryDto
            {
                PlayerId = 1,
                InjuryType = "Injury",
                InjuryDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _injuryService.CreateInjuryAsync(createDto)
            );
        }

        [Fact]
        public async Task UpdateInjuryAsync_UpdatesStatus()
        {
            // Arrange
            var updateDto = new UpdateInjuryDto
            {
                Status = "Recovered"
            };

            // Act
            var result = await _injuryService.UpdateInjuryAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Recovered", result.Status);
        }

        [Fact]
        public async Task UpdateInjuryAsync_UpdatesRecoveryDate()
        {
            // Arrange
            var recoveryDate = DateTime.UtcNow.Date;
            var updateDto = new UpdateInjuryDto
            {
                RecoveryDate = recoveryDate
            };

            // Act
            var result = await _injuryService.UpdateInjuryAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(recoveryDate, result.RecoveryDate);
        }

        [Fact]
        public async Task UpdateInjuryAsync_UpdatesNotes()
        {
            // Arrange
            var updateDto = new UpdateInjuryDto
            {
                Notes = "Updated: Player is recovering well"
            };

            // Act
            var result = await _injuryService.UpdateInjuryAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated: Player is recovering well", result.Notes);
        }

        [Fact]
        public async Task UpdateInjuryAsync_ThrowsException_WhenRecoveryDateBeforeInjuryDate()
        {
            // Arrange
            var updateDto = new UpdateInjuryDto
            {
                RecoveryDate = DateTime.UtcNow.Date.AddDays(-20)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _injuryService.UpdateInjuryAsync(1, updateDto)
            );
        }

        [Fact]
        public async Task UpdateInjuryAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var updateDto = new UpdateInjuryDto
            {
                Status = "Recovered"
            };

            // Act
            var result = await _injuryService.UpdateInjuryAsync(999, updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteInjuryAsync_DeletesInjury()
        {
            // Act
            var result = await _injuryService.DeleteInjuryAsync(1);

            // Assert
            Assert.True(result);

            // Verify deletion
            var injury = await _injuryService.GetInjuryByIdAsync(1);
            Assert.Null(injury);
        }

        [Fact]
        public async Task DeleteInjuryAsync_ReturnsFalse_WhenNotFound()
        {
            // Act
            var result = await _injuryService.DeleteInjuryAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}
