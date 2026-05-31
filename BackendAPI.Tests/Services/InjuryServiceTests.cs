using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendAPI.Tests.Services
{
    public class InjuryServiceTests
    {
        private static ApplicationDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static InjuryService CreateService(ApplicationDbContext context)
        {
            return new InjuryService(context);
        }

        [Fact]
        public async Task GetInjuriesAsync_FiltersByPlayerAndStatusAndSortsByDate()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var player = new Player
            {
                FirstName = "Test",
                LastName = "Player",
                JerseyNumber = 10,
                Position = "Midfielder",
                DateOfBirth = new DateTime(1995, 1, 1),
                Nationality = "Country"
            };
            context.Players.Add(player);
            context.SaveChanges();

            context.Injuries.AddRange(
                new Injury
                {
                    PlayerId = player.Id,
                    InjuryType = "Sprain",
                    InjuryDate = DateTime.UtcNow.Date.AddDays(-5),
                    Status = InjuryStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Injury
                {
                    PlayerId = player.Id,
                    InjuryType = "Strain",
                    InjuryDate = DateTime.UtcNow.Date.AddDays(-1),
                    Status = InjuryStatus.Recovering,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetInjuriesAsync(page: 1, pageSize: 10, playerId: player.Id, status: "Recovering", sortBy: "date");

            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Data);
            Assert.Equal("Strain", result.Data.Single().InjuryType);
        }

        [Fact]
        public async Task GetActiveInjuriesAsync_ExcludesRecoveredAndPaginates()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var player = new Player
            {
                FirstName = "Active",
                LastName = "Player",
                JerseyNumber = 11,
                Position = "Defender",
                DateOfBirth = new DateTime(1996, 1, 1),
                Nationality = "Country"
            };
            context.Players.Add(player);
            context.SaveChanges();

            context.Injuries.AddRange(
                new Injury
                {
                    PlayerId = player.Id,
                    InjuryType = "Knee",
                    InjuryDate = DateTime.UtcNow.Date.AddDays(-2),
                    Status = InjuryStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Injury
                {
                    PlayerId = player.Id,
                    InjuryType = "Ankle",
                    InjuryDate = DateTime.UtcNow.Date.AddDays(-10),
                    Status = InjuryStatus.Recovered,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetActiveInjuriesAsync(page: 1, pageSize: 10);

            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Data);
            Assert.Equal(InjuryStatus.Active.ToString(), result.Data.Single().Status);
        }

        [Fact]
        public async Task CreateInjuryAsync_RejectsFutureDate()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var player = new Player
            {
                FirstName = "Future",
                LastName = "Player",
                JerseyNumber = 12,
                Position = "Forward",
                DateOfBirth = new DateTime(1994, 1, 1),
                Nationality = "Country"
            };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateInjuryAsync(new CreateInjuryDto
            {
                PlayerId = player.Id,
                InjuryType = "Hamstring",
                InjuryDate = DateTime.UtcNow.Date.AddDays(1),
                Notes = "Future injury"
            }));
        }

        [Fact]
        public async Task CreateInjuryAsync_RejectsMissingPlayer()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateInjuryAsync(new CreateInjuryDto
            {
                PlayerId = 999,
                InjuryType = "Sprain",
                InjuryDate = DateTime.UtcNow.Date,
                Notes = "Missing player"
            }));
        }

        [Fact]
        public async Task UpdateInjuryAsync_SetsRecoveryDateAndStatus()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var player = new Player
            {
                FirstName = "Update",
                LastName = "Player",
                JerseyNumber = 13,
                Position = "Midfielder",
                DateOfBirth = new DateTime(1993, 1, 1),
                Nationality = "Country"
            };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            var injury = new Injury
            {
                PlayerId = player.Id,
                InjuryType = "Back",
                InjuryDate = DateTime.UtcNow.Date.AddDays(-5),
                Status = InjuryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Injuries.Add(injury);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var recoveryDate = DateTime.UtcNow.Date;
            var updated = await service.UpdateInjuryAsync(injury.Id, new UpdateInjuryDto
            {
                RecoveryDate = recoveryDate,
                Status = InjuryStatus.Recovered,
                Notes = "Recovered fully"
            });

            Assert.NotNull(updated);
            Assert.Equal(recoveryDate, updated!.RecoveryDate);
            Assert.Equal(InjuryStatus.Recovered.ToString(), updated.Status);
            Assert.Equal("Recovered fully", updated.Notes);
        }

        [Fact]
        public async Task UpdateInjuryAsync_ThrowsWhenRecoveryBeforeInjury()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var player = new Player
            {
                FirstName = "Invalid",
                LastName = "Player",
                JerseyNumber = 14,
                Position = "Defender",
                DateOfBirth = new DateTime(1992, 1, 1),
                Nationality = "Country"
            };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            var injury = new Injury
            {
                PlayerId = player.Id,
                InjuryType = "Concussion",
                InjuryDate = DateTime.UtcNow.Date,
                Status = InjuryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Injuries.Add(injury);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateInjuryAsync(injury.Id, new UpdateInjuryDto
            {
                RecoveryDate = DateTime.UtcNow.Date.AddDays(-1),
                Status = InjuryStatus.Recovered
            }));
        }
    }
}
