using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackendAPI.Tests.Services
{
    public class TransferServiceTests
    {
        private static ApplicationDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static TransferService CreateService(ApplicationDbContext context)
        {
            return new TransferService(context, NullLogger<TransferService>.Instance);
        }

        [Fact]
        public async Task GetTransfersAsync_FiltersSortsAndPaginates()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (playerOne, playerTwo, clubOne, clubTwo) = SeedBaseData(context);
            context.Transfers.AddRange(
                CreateTransfer(playerOne.Id, clubOne.Id, clubTwo.Id, DateTime.UtcNow.Date.AddDays(-2), 100, TransferType.Permanent),
                CreateTransfer(playerOne.Id, clubTwo.Id, clubOne.Id, DateTime.UtcNow.Date.AddDays(-1), 200, TransferType.Loan),
                CreateTransfer(playerTwo.Id, clubOne.Id, clubTwo.Id, DateTime.UtcNow.Date.AddDays(-3), 300, TransferType.Loan));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var (transfers, totalCount) = await service.GetTransfersAsync(
                page: 1,
                pageSize: 1,
                playerId: playerOne.Id,
                type: TransferType.Loan,
                fromDate: DateTime.UtcNow.Date.AddDays(-5),
                toDate: DateTime.UtcNow.Date);

            Assert.Equal(1, totalCount);
            Assert.Single(transfers);
            Assert.Equal(TransferType.Loan, transfers.Single().Type);
            Assert.Equal(playerOne.Id, transfers.Single().PlayerId);
        }

        [Fact]
        public async Task GetTransfersByPlayerAsync_ReturnsNewestFirst()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, _, clubOne, clubTwo) = SeedBaseData(context);
            var older = CreateTransfer(player.Id, clubOne.Id, clubTwo.Id, DateTime.UtcNow.Date.AddDays(-10), 100, TransferType.Permanent);
            var newer = CreateTransfer(player.Id, clubTwo.Id, clubOne.Id, DateTime.UtcNow.Date.AddDays(-1), 200, TransferType.Loan);
            context.Transfers.AddRange(older, newer);
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetTransfersByPlayerAsync(player.Id);

            Assert.Equal(newer.Id, result.First().Id);
            Assert.Equal(older.Id, result.Last().Id);
        }

        [Fact]
        public async Task CreateTransferAsync_RejectsSameClubAndNegativeFee()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, _, club, _) = SeedBaseData(context);
            var service = CreateService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateTransferAsync(new CreateTransferDto
                {
                    PlayerId = player.Id,
                    FromClubId = club.Id,
                    ToClubId = club.Id,
                    TransferDate = DateTime.UtcNow.Date,
                    TransferFee = 0,
                    Type = TransferType.Permanent
                }));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateTransferAsync(new CreateTransferDto
                {
                    PlayerId = player.Id,
                    FromClubId = club.Id,
                    ToClubId = context.Clubs.Single(c => c.Id != club.Id).Id,
                    TransferDate = DateTime.UtcNow.Date,
                    TransferFee = -1,
                    Type = TransferType.Loan
                }));
        }

        [Fact]
        public async Task UpdateAndDeleteTransferAsync_Work()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, _, clubOne, clubTwo) = SeedBaseData(context);
            var transfer = CreateTransfer(player.Id, clubOne.Id, clubTwo.Id, DateTime.UtcNow.Date, 100, TransferType.Permanent);
            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var updated = await service.UpdateTransferAsync(transfer.Id, 250, TransferType.FreeTransfer);
            var deleted = await service.DeleteTransferAsync(transfer.Id);

            Assert.NotNull(updated);
            Assert.Equal(250m, updated!.TransferFee);
            Assert.Equal(TransferType.FreeTransfer, updated.Type);
            Assert.True(deleted);
            Assert.Null(await service.GetTransferByIdAsync(transfer.Id));
        }

        private static (Player PlayerOne, Player PlayerTwo, Club ClubOne, Club ClubTwo) SeedBaseData(ApplicationDbContext context)
        {
            var playerOne = new Player
            {
                FirstName = "First",
                LastName = "Player",
                JerseyNumber = 7,
                Position = "Forward",
                DateOfBirth = new DateTime(1998, 1, 1),
                Nationality = "Country"
            };

            var playerTwo = new Player
            {
                FirstName = "Second",
                LastName = "Player",
                JerseyNumber = 8,
                Position = "Midfielder",
                DateOfBirth = new DateTime(1999, 1, 1),
                Nationality = "Country"
            };

            var clubOne = new Club
            {
                Name = "Club One",
                City = "City One",
                FoundedYear = 1900,
                UserId = "club-one-user"
            };

            var clubTwo = new Club
            {
                Name = "Club Two",
                City = "City Two",
                FoundedYear = 1901,
                UserId = "club-two-user"
            };

            context.Players.AddRange(playerOne, playerTwo);
            context.Clubs.AddRange(clubOne, clubTwo);
            context.SaveChanges();

            return (playerOne, playerTwo, clubOne, clubTwo);
        }

        private static Transfer CreateTransfer(
            int playerId,
            int fromClubId,
            int toClubId,
            DateTime date,
            decimal fee,
            TransferType type)
        {
            return new Transfer
            {
                PlayerId = playerId,
                FromClubId = fromClubId,
                ToClubId = toClubId,
                TransferDate = date,
                TransferFee = fee,
                Type = type,
                Status = TransferStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
