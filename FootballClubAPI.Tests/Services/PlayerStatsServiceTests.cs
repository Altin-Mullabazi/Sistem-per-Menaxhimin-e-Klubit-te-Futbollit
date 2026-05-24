using Xunit;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Tests.Services
{
    public class PlayerStatsServiceTests
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
            var clubA = new Club { Name = "Club A", City = "City", FoundedYear = 1900 };
            var clubB = new Club { Name = "Club B", City = "City", FoundedYear = 1905 };
            var stadium = new Stadium { Name = "Main Stadium", City = "City", Capacity = 20000 };
            var season = new Season
            {
                Name = "Season 2025",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                UserId = "seed-user"
            };

            context.Clubs.AddRange(clubA, clubB);
            context.Stadiums.Add(stadium);
            context.Seasons.Add(season);
            context.SaveChanges();

            var playerA = new Player { FirstName = "John", LastName = "Doe", JerseyNumber = 7, Position = "Forward", DateOfBirth = DateTime.UtcNow.AddYears(-25), Nationality = "Country", ClubId = clubA.Id };
            var playerB = new Player { FirstName = "Jane", LastName = "Smith", JerseyNumber = 10, Position = "Midfielder", DateOfBirth = DateTime.UtcNow.AddYears(-23), Nationality = "Country", ClubId = clubB.Id };

            context.Players.AddRange(playerA, playerB);
            context.SaveChanges();

            var match = new Match
            {
                HomeClubId = clubA.Id,
                AwayClubId = clubB.Id,
                StadiumId = stadium.Id,
                SeasonId = season.Id,
                MatchDate = DateTime.UtcNow,
                Status = "Scheduled"
            };

            context.Matches.Add(match);
            context.SaveChanges();

            context.PlayerStats.AddRange(
                new PlayerStats { PlayerId = playerA.Id, MatchId = match.Id, GoalsScored = 2, Assists = 1, YellowCards = 1, RedCards = 0, MinutesPlayed = 90 },
                new PlayerStats { PlayerId = playerB.Id, MatchId = match.Id, GoalsScored = 4, Assists = 2, YellowCards = 0, RedCards = 0, MinutesPlayed = 90 }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetPlayerStats_ReturnsPaginatedResultsSortedByGoalsDesc()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);

            var (stats, totalCount) = await service.GetPlayerStatsAsync(1, 1);

            Assert.Equal(2, totalCount);
            Assert.Single(stats);
            Assert.Equal(4, stats[0].GoalsScored);
            Assert.Equal("Jane Smith", stats[0].PlayerName);
        }

        [Fact]
        public async Task GetTopScorers_ReturnsCorrectRanking()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);

            var topScorers = await service.GetTopScorersAsync(2);

            Assert.Equal(2, topScorers.Count);
            Assert.Equal("Jane Smith", topScorers[0].PlayerName);
            Assert.Equal(4, topScorers[0].GoalsScored);
            Assert.Equal("Club B", topScorers[0].ClubName);
        }

        [Fact]
        public async Task CreatePlayerStats_CreatesSuccessfully()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);

            var match = await context.Matches.FirstAsync();
            var freshPlayer = new Player
            {
                FirstName = "New",
                LastName = "Player",
                JerseyNumber = 11,
                Position = "Forward",
                DateOfBirth = DateTime.UtcNow.AddYears(-22),
                Nationality = "Country",
                ClubId = match.HomeClubId
            };

            context.Players.Add(freshPlayer);
            await context.SaveChangesAsync();

            var createDto = new CreatePlayerStatsDto
            {
                PlayerId = freshPlayer.Id,
                MatchId = match.Id,
                GoalsScored = 1,
                Assists = 0,
                YellowCards = 0,
                RedCards = 0,
                MinutesPlayed = 45
            };

            var stats = await service.CreatePlayerStatsAsync(createDto);

            Assert.NotNull(stats);
            Assert.Equal(freshPlayer.Id, stats.PlayerId);
            Assert.Equal(match.Id, stats.MatchId);
            Assert.Equal(1, stats.GoalsScored);
        }

        [Fact]
        public async Task CreatePlayerStats_ThrowsInvalidOperationException_ForInvalidValues()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);
            var match = await context.Matches.FirstAsync();
            var player = await context.Players.FirstAsync(p => p.ClubId == match.HomeClubId);

            var createDto = new CreatePlayerStatsDto
            {
                PlayerId = player.Id,
                MatchId = match.Id,
                GoalsScored = -1,
                Assists = 0,
                YellowCards = 0,
                RedCards = 0,
                MinutesPlayed = 45
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePlayerStatsAsync(createDto));
        }

        [Fact]
        public async Task UpdatePlayerStats_UpdatesSuccessfully()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);
            var stats = await context.PlayerStats.FirstAsync();
            var originalGoals = stats.GoalsScored;
            var originalAssists = stats.Assists;

            var updateDto = new UpdatePlayerStatsDto
            {
                GoalsScored = originalGoals + 1,
                Assists = originalAssists + 1,
                YellowCards = stats.YellowCards,
                RedCards = stats.RedCards,
                MinutesPlayed = stats.MinutesPlayed
            };

            var updated = await service.UpdatePlayerStatsAsync(stats.Id, updateDto);

            Assert.NotNull(updated);
            Assert.Equal(originalGoals + 1, updated!.GoalsScored);
            Assert.Equal(originalAssists + 1, updated.Assists);
        }

        [Fact]
        public async Task DeletePlayerStats_DeletesSuccessfully()
        {
            var context = CreateInMemoryDbContext();
            SeedTestData(context);
            var service = new PlayerStatsService(context);
            var stats = await context.PlayerStats.FirstAsync();

            var deleted = await service.DeletePlayerStatsAsync(stats.Id);

            Assert.True(deleted);
            Assert.Null(await context.PlayerStats.FindAsync(stats.Id));
        }
    }
}
