using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Controllers;
using FootballClubAPI.Data;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendAPI.Tests.Services
{
    public class DashboardServiceTests
    {
        private static ApplicationDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static DashboardService CreateService(ApplicationDbContext context)
        {
            return new DashboardService(new DashboardRepository(context));
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsCorrectTotals()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var summary = await service.GetSummaryAsync();

            Assert.Equal(2, summary.TotalClubs);
            Assert.Equal(3, summary.TotalPlayers);
            Assert.Equal(4, summary.TotalMatches);
            Assert.Equal(1, summary.TotalStaff);
            Assert.Equal(3, summary.TotalInjuries);
            Assert.Equal(3, summary.TotalContracts);
        }

        [Fact]
        public async Task GetTopScorersAsync_OrdersByAggregatedGoalsThenAssists()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var scorers = await service.GetTopScorersAsync(limit: 2);

            Assert.Equal(2, scorers.Count);
            Assert.Equal("First Striker", scorers[0].PlayerName);
            Assert.Equal(5, scorers[0].Goals);
            Assert.Equal(3, scorers[0].Assists);
            Assert.Equal("Second Winger", scorers[1].PlayerName);
            Assert.Equal(5, scorers[1].Goals);
            Assert.Equal(1, scorers[1].Assists);
        }

        [Fact]
        public async Task GetUpcomingMatchesAsync_FiltersDateRangeAndFinishedMatches()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var matches = await service.GetUpcomingMatchesAsync(days: 7);

            Assert.Equal(2, matches.Count);
            Assert.True(matches[0].Date <= matches[1].Date);
            Assert.DoesNotContain(matches, match => match.Date > DateTime.UtcNow.Date.AddDays(8));
            Assert.DoesNotContain(matches, match => match.Date == DateTime.UtcNow.Date.AddDays(3));
        }

        [Fact]
        public async Task GetInjuredPlayersAsync_ReturnsActiveOnlyNewestFirst()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var injuries = await service.GetInjuredPlayersAsync();

            Assert.Equal(2, injuries.Count);
            Assert.DoesNotContain(injuries, injury => injury.Status == InjuryStatus.Recovered.ToString());
            Assert.True(injuries[0].InjuryDate >= injuries[1].InjuryDate);
        }

        [Fact]
        public async Task GetExpiringContractsAsync_ReturnsActiveFutureContractsWithinRange()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var contracts = await service.GetExpiringContractsAsync(days: 90);

            Assert.Single(contracts);
            Assert.Equal("First Striker", contracts[0].PlayerName);
            Assert.True(contracts[0].ContractEndDate >= DateTime.UtcNow.Date);
            Assert.True(contracts[0].ContractEndDate < DateTime.UtcNow.Date.AddDays(91));
        }

        [Fact]
        public async Task GetRecentTransfersAsync_ReturnsTransfersWithinRangeNewestFirst()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var transfers = await service.GetRecentTransfersAsync(days: 30);

            Assert.Equal(2, transfers.Count);
            Assert.True(transfers[0].TransferDate >= transfers[1].TransferDate);
            Assert.All(transfers, transfer => Assert.True(transfer.TransferDate >= DateTime.UtcNow.Date.AddDays(-30)));
        }

        [Fact]
        public async Task DashboardAggregations_AreAccurateAcrossRelatedRows()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            SeedDashboardData(context);
            var service = CreateService(context);

            var scorers = await service.GetTopScorersAsync(limit: 10);

            var firstStriker = scorers.Single(scorer => scorer.PlayerName == "First Striker");
            Assert.Equal(5, firstStriker.Goals);
            Assert.Equal(3, firstStriker.Assists);
            Assert.Equal("Club A", firstStriker.Club);
        }

        [Fact]
        public void DashboardController_RequiresAuthentication()
        {
            var authorizeAttribute = typeof(DashboardController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .SingleOrDefault();

            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public async Task DashboardEndpoints_ReturnEmptyStatePayloads()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var summary = await service.GetSummaryAsync();

            Assert.Equal(0, summary.TotalClubs);
            Assert.Empty(await service.GetTopScorersAsync());
            Assert.Empty(await service.GetUpcomingMatchesAsync());
            Assert.Empty(await service.GetInjuredPlayersAsync());
            Assert.Empty(await service.GetExpiringContractsAsync());
            Assert.Empty(await service.GetRecentTransfersAsync());
        }

        [Fact]
        public async Task DashboardQueryValidation_RejectsInvalidParameters()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetTopScorersAsync(0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetUpcomingMatchesAsync(0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetExpiringContractsAsync(366));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetRecentTransfersAsync(-1));
        }

        private static void SeedDashboardData(ApplicationDbContext context)
        {
            var today = DateTime.UtcNow.Date;

            var clubA = new Club
            {
                Name = "Club A",
                City = "City A",
                FoundedYear = 1900,
                UserId = "club-user-a"
            };

            var clubB = new Club
            {
                Name = "Club B",
                City = "City B",
                FoundedYear = 1905,
                UserId = "club-user-b"
            };

            var stadium = new Stadium
            {
                Name = "Main Stadium",
                City = "City A",
                Capacity = 20000,
                YearBuilt = 1999,
                UserId = "stadium-user"
            };

            var season = new Season
            {
                Name = "Season 2026",
                StartDate = today.AddMonths(-1),
                EndDate = today.AddMonths(8),
                UserId = "season-user"
            };

            context.Clubs.AddRange(clubA, clubB);
            context.Stadiums.Add(stadium);
            context.Seasons.Add(season);
            context.LegacyUsers.AddRange(
                new User
                {
                    Username = "staff",
                    Email = "staff@test.com",
                    PasswordHash = "hash",
                    FirstName = "Staff",
                    LastName = "Member",
                    Role = "Staff"
                },
                new User
                {
                    Username = "fan",
                    Email = "fan@test.com",
                    PasswordHash = "hash",
                    FirstName = "Fan",
                    LastName = "Member",
                    Role = "Fan"
                });
            context.SaveChanges();

            var playerOne = new Player
            {
                FirstName = "First",
                LastName = "Striker",
                JerseyNumber = 9,
                Position = "Forward",
                DateOfBirth = today.AddYears(-25),
                Nationality = "Country",
                ClubId = clubA.Id
            };

            var playerTwo = new Player
            {
                FirstName = "Second",
                LastName = "Winger",
                JerseyNumber = 11,
                Position = "Forward",
                DateOfBirth = today.AddYears(-23),
                Nationality = "Country",
                ClubId = clubB.Id
            };

            var playerThree = new Player
            {
                FirstName = "Third",
                LastName = "Keeper",
                JerseyNumber = 1,
                Position = "Goalkeeper",
                DateOfBirth = today.AddYears(-28),
                Nationality = "Country",
                ClubId = clubA.Id
            };

            context.Players.AddRange(playerOne, playerTwo, playerThree);
            context.SaveChanges();

            var upcomingSoon = new Match
            {
                HomeClubId = clubA.Id,
                AwayClubId = clubB.Id,
                StadiumId = stadium.Id,
                SeasonId = season.Id,
                MatchDate = today.AddDays(2).AddHours(18),
                Status = "Scheduled"
            };

            var upcomingBoundary = new Match
            {
                HomeClubId = clubB.Id,
                AwayClubId = clubA.Id,
                StadiumId = stadium.Id,
                SeasonId = season.Id,
                MatchDate = today.AddDays(7).AddHours(20),
                Status = "Scheduled"
            };

            var finishedMatch = new Match
            {
                HomeClubId = clubA.Id,
                AwayClubId = clubB.Id,
                StadiumId = stadium.Id,
                SeasonId = season.Id,
                MatchDate = today.AddDays(3),
                Status = "Finished"
            };

            var farFuture = new Match
            {
                HomeClubId = clubA.Id,
                AwayClubId = clubB.Id,
                StadiumId = stadium.Id,
                SeasonId = season.Id,
                MatchDate = today.AddDays(20),
                Status = "Scheduled"
            };

            context.Matches.AddRange(upcomingSoon, upcomingBoundary, finishedMatch, farFuture);
            context.SaveChanges();

            context.PlayerStats.AddRange(
                new PlayerStats { PlayerId = playerOne.Id, MatchId = upcomingSoon.Id, GoalsScored = 2, Assists = 1, MinutesPlayed = 90 },
                new PlayerStats { PlayerId = playerOne.Id, MatchId = upcomingBoundary.Id, GoalsScored = 3, Assists = 2, MinutesPlayed = 90 },
                new PlayerStats { PlayerId = playerTwo.Id, MatchId = upcomingSoon.Id, GoalsScored = 5, Assists = 1, MinutesPlayed = 90 },
                new PlayerStats { PlayerId = playerThree.Id, MatchId = upcomingSoon.Id, GoalsScored = 0, Assists = 0, MinutesPlayed = 90 });

            context.Injuries.AddRange(
                new Injury { PlayerId = playerOne.Id, InjuryType = "Hamstring", InjuryDate = today.AddDays(-2), Status = InjuryStatus.Active },
                new Injury { PlayerId = playerTwo.Id, InjuryType = "Ankle", InjuryDate = today.AddDays(-1), Status = InjuryStatus.Recovering },
                new Injury { PlayerId = playerThree.Id, InjuryType = "Shoulder", InjuryDate = today.AddDays(-10), Status = InjuryStatus.Recovered });

            context.Contracts.AddRange(
                new Contract
                {
                    PlayerId = playerOne.Id,
                    ClubId = clubA.Id,
                    Position = "Forward",
                    Salary = 1000,
                    StartDate = today.AddYears(-1),
                    EndDate = today.AddDays(30),
                    Status = ContractStatus.Active,
                    UpdatedAt = today
                },
                new Contract
                {
                    PlayerId = playerTwo.Id,
                    ClubId = clubB.Id,
                    Position = "Forward",
                    Salary = 1000,
                    StartDate = today.AddYears(-1),
                    EndDate = today.AddDays(150),
                    Status = ContractStatus.Active,
                    UpdatedAt = today
                },
                new Contract
                {
                    PlayerId = playerThree.Id,
                    ClubId = clubA.Id,
                    Position = "Goalkeeper",
                    Salary = 1000,
                    StartDate = today.AddYears(-2),
                    EndDate = today.AddDays(-5),
                    Status = ContractStatus.Expired,
                    UpdatedAt = today
                });

            context.Transfers.AddRange(
                new Transfer
                {
                    PlayerId = playerOne.Id,
                    FromClubId = clubB.Id,
                    ToClubId = clubA.Id,
                    TransferDate = today.AddDays(-3),
                    TransferFee = 50000,
                    Status = TransferStatus.Completed
                },
                new Transfer
                {
                    PlayerId = playerTwo.Id,
                    FromClubId = clubA.Id,
                    ToClubId = clubB.Id,
                    TransferDate = today.AddDays(-20),
                    TransferFee = 0,
                    Status = TransferStatus.Completed
                },
                new Transfer
                {
                    PlayerId = playerThree.Id,
                    FromClubId = clubB.Id,
                    ToClubId = clubA.Id,
                    TransferDate = today.AddDays(-60),
                    TransferFee = 1000,
                    Status = TransferStatus.Completed
                });

            context.SaveChanges();
        }
    }
}
