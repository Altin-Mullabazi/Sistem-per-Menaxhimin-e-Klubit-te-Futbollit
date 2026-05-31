using Xunit;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Tests.Services
{
    public class MatchEventServiceTests
    {
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            var user = new ApplicationUser 
            { 
                Id = "test-user", 
                UserName = "testuser", 
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var club1 = new Club { Id = 1, Name = "Team A" };
            var club2 = new Club { Id = 2, Name = "Team B" };

            var stadium = new Stadium { Id = 1, Name = "Stadium 1", City = "City A", Capacity = 20000, YearBuilt = 1990, UserId = "test-user" };
            
            var season = new Season 
            { 
                Id = 1, 
                Name = "Season 2025", 
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                UserId = "test-user"
            };

            var match = new Match
            {
                Id = 1,
                HomeClubId = 1,
                AwayClubId = 2,
                StadiumId = 1,
                SeasonId = 1,
                MatchDate = DateTime.UtcNow.AddDays(1),
                Status = "Scheduled",
                HomeScore = null,
                AwayScore = null
            };

            var player1 = new Player
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                JerseyNumber = 7,
                Position = "Forward",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Nationality = "Country",
                ClubId = 1
            };

            var player2 = new Player
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                JerseyNumber = 10,
                Position = "Midfielder",
                DateOfBirth = DateTime.UtcNow.AddYears(-23),
                Nationality = "Country",
                ClubId = 1
            };

            var matchEvent1 = new MatchEvent
            {
                Id = 1,
                MatchId = 1,
                PlayerId = 1,
                EventType = EventType.Goal,
                Minute = 15,
                Description = "Header from cross"
            };

            var matchEvent2 = new MatchEvent
            {
                Id = 2,
                MatchId = 1,
                PlayerId = 2,
                EventType = EventType.YellowCard,
                Minute = 30,
                Description = "Rough tackle"
            };

            var matchEvent3 = new MatchEvent
            {
                Id = 3,
                MatchId = 1,
                PlayerId = 1,
                EventType = EventType.Goal,
                Minute = 45,
                Description = null
            };

            context.Users.Add(user);
            context.Clubs.AddRange(club1, club2);
            context.Stadiums.Add(stadium);
            context.Seasons.Add(season);
            context.Matches.Add(match);
            context.Players.AddRange(player1, player2);
            context.MatchEvents.AddRange(matchEvent1, matchEvent2, matchEvent3);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetMatchEvents_ReturnsAllEventsForMatch()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, totalCount) = await service.GetMatchEventsAsync(1);

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(3, events.Count);
        }

        [Fact]
        public async Task GetMatchEvents_SortsByMinuteAscending()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, _) = await service.GetMatchEventsAsync(1);

            // Assert
            Assert.Equal(15, events[0].Minute);
            Assert.Equal(30, events[1].Minute);
            Assert.Equal(45, events[2].Minute);
        }

        [Fact]
        public async Task GetMatchEvents_ReturnsPaginatedResults()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, totalCount) = await service.GetMatchEventsAsync(1, 1, 2);

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetMatchEvents_IncludesPlayerName()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, _) = await service.GetMatchEventsAsync(1);

            // Assert
            Assert.NotEmpty(events[0].PlayerName);
            Assert.Equal("John Doe", events[0].PlayerName);
        }

        [Fact]
        public async Task GetMatchEventById_ReturnsEventDetails()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var matchEvent = await service.GetMatchEventByIdAsync(1);

            // Assert
            Assert.NotNull(matchEvent);
            Assert.Equal(1, matchEvent.Id);
            Assert.Equal("Goal", matchEvent.EventType);
            Assert.Equal(15, matchEvent.Minute);
        }

        [Fact]
        public async Task GetMatchEventById_ReturnsNullForNonExistent()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var matchEvent = await service.GetMatchEventByIdAsync(999);

            // Assert
            Assert.Null(matchEvent);
        }

        [Fact]
        public async Task GetPlayerEvents_ReturnsEventsForPlayer()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, totalCount) = await service.GetPlayerEventsAsync(1, 1);

            // Assert
            Assert.Equal(2, totalCount); // Player 1 has 2 events
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetPlayerEvents_SortsByMinute()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var (events, _) = await service.GetPlayerEventsAsync(1, 1);

            // Assert
            Assert.Equal(15, events[0].Minute);
            Assert.Equal(45, events[1].Minute);
        }

        [Fact]
        public async Task CreateMatchEvent_CreatesSuccessfully()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 1,
                PlayerId = 1,
                EventType = "Goal",
                Minute = 60,
                Description = "Penalty kick"
            };

            // Act
            var result = await service.CreateMatchEventAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Goal", result.EventType);
            Assert.Equal(60, result.Minute);
            Assert.Equal("Penalty kick", result.Description);
        }

        [Fact]
        public async Task CreateMatchEvent_ThrowsException_InvalidEventType()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 1,
                PlayerId = 1,
                EventType = "InvalidEvent",
                Minute = 60
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateMatchEventAsync(createDto));
        }

        [Fact]
        public async Task CreateMatchEvent_ThrowsException_MinuteTooHigh()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 1,
                PlayerId = 1,
                EventType = "Goal",
                Minute = 121 // Beyond max
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateMatchEventAsync(createDto));
        }

        [Fact]
        public async Task CreateMatchEvent_ThrowsException_MinuteTooLow()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 1,
                PlayerId = 1,
                EventType = "Goal",
                Minute = -1 // Negative
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateMatchEventAsync(createDto));
        }

        [Fact]
        public async Task CreateMatchEvent_ThrowsException_MatchNotFound()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 999,
                PlayerId = 1,
                EventType = "Goal",
                Minute = 60
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateMatchEventAsync(createDto));
        }

        [Fact]
        public async Task CreateMatchEvent_ThrowsException_PlayerNotFound()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var createDto = new CreateMatchEventDto
            {
                MatchId = 1,
                PlayerId = 999,
                EventType = "Goal",
                Minute = 60
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CreateMatchEventAsync(createDto));
        }

        [Fact]
        public async Task UpdateMatchEvent_UpdatesSuccessfully()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var updateDto = new UpdateMatchEventDto
            {
                EventType = "RedCard",
                Minute = 50,
                Description = "Updated description"
            };

            // Act
            var result = await service.UpdateMatchEventAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("RedCard", result.EventType);
            Assert.Equal(50, result.Minute);
            Assert.Equal("Updated description", result.Description);
        }

        [Fact]
        public async Task UpdateMatchEvent_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var updateDto = new UpdateMatchEventDto
            {
                Minute = 50
            };

            // Act
            var result = await service.UpdateMatchEventAsync(999, updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateMatchEvent_ThrowsException_InvalidEventType()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var updateDto = new UpdateMatchEventDto
            {
                EventType = "InvalidType"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.UpdateMatchEventAsync(1, updateDto));
        }

        [Fact]
        public async Task UpdateMatchEvent_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            var updateDto = new UpdateMatchEventDto
            {
                Minute = 20
                // Only update minute
            };

            // Act
            var result = await service.UpdateMatchEventAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Minute);
            Assert.Equal("Goal", result.EventType); // Should remain unchanged
        }

        [Fact]
        public async Task DeleteMatchEvent_DeletesSuccessfully()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var result = await service.DeleteMatchEventAsync(1);

            // Assert
            Assert.True(result);

            // Verify deletion
            var deletedEvent = await service.GetMatchEventByIdAsync(1);
            Assert.Null(deletedEvent);
        }

        [Fact]
        public async Task DeleteMatchEvent_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new MatchEventService(context);

            // Act
            var result = await service.DeleteMatchEventAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}
