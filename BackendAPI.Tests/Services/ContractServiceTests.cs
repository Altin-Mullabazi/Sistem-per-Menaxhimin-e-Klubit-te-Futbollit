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
    public class ContractServiceTests
    {
        private static ApplicationDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ContractService CreateService(ApplicationDbContext context)
        {
            return new ContractService(context);
        }

        /// <summary>
        /// Helper to create test data
        /// </summary>
        private static (Player, Club) CreateTestPlayerAndClub(ApplicationDbContext context)
        {
            var player = new Player
            {
                FirstName = "John",
                LastName = "Doe",
                JerseyNumber = 10,
                Position = "Forward",
                DateOfBirth = new DateTime(1990, 1, 1),
                Nationality = "England",
                Height = 185.5m,
                Weight = 82.0m
            };

            var club = new Club
            {
                Name = "Test Club",
                City = "London",
                FoundedYear = 2000,
                Budget = 1000000m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = "test-user-id"
            };

            context.Players.Add(player);
            context.Clubs.Add(club);
            context.SaveChanges();

            return (player, club);
        }

        // ========================================
        // SCENARIO 1: Create first active contract
        // ========================================
        [Fact]
        public async Task CreateContractAsync_WithIsActiveTrue_CreatesActiveContract()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);
            var createDto = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var result = await service.CreateContractAsync(createDto);

            Assert.NotNull(result);
            Assert.Equal(player.Id, result.PlayerId);
            Assert.Equal(club.Id, result.ClubId);
            Assert.True(result.IsActive);
            Assert.Equal(50000, result.Salary);
        }

        // ========================================
        // SCENARIO 2: Query active contracts
        // ========================================
        [Fact]
        public async Task GetActiveContractsAsync_AfterCreatingOne_ReturnsSingleContract()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);
            var createDto = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            await service.CreateContractAsync(createDto);

            var result = await service.GetActiveContractsAsync(page: 1, pageSize: 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.True(result.Items.First().IsActive);
        }

        // ========================================
        // SCENARIO 3: Create second active contract (AUTO-DEACTIVATION)
        // ========================================
        [Fact]
        public async Task CreateContractAsync_WithSecondActiveForSamePlayer_DeactivatesPrevious()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create first contract
            var createDto1 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract1 = await service.CreateContractAsync(createDto1);
            int contract1Id = contract1.Id;

            // Create second contract (should deactivate first)
            var createDto2 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2028, 1, 1),
                EndDate = new DateTime(2030, 12, 31),
                Salary = 60000,
                Position = "Forward",
                IsActive = true
            };

            var contract2 = await service.CreateContractAsync(createDto2);

            // Verify first contract is deactivated
            var contract1Updated = await service.GetContractByIdAsync(contract1Id);
            Assert.False(contract1Updated.IsActive);

            // Verify second contract is active
            Assert.True(contract2.IsActive);
        }

        // ========================================
        // SCENARIO 4: Query active contracts - only new one
        // ========================================
        [Fact]
        public async Task GetActiveContractsAsync_AfterAutoDeactivation_ReturnsOnlyNewContract()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create first contract
            var createDto1 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            await service.CreateContractAsync(createDto1);

            // Create second contract
            var createDto2 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2028, 1, 1),
                EndDate = new DateTime(2030, 12, 31),
                Salary = 60000,
                Position = "Forward",
                IsActive = true
            };

            var contract2 = await service.CreateContractAsync(createDto2);

            // Query active contracts
            var result = await service.GetActiveContractsAsync(page: 1, pageSize: 10);

            // Should have exactly 1 active contract
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(contract2.Id, result.Items.First().Id);
        }

        // ========================================
        // SCENARIO 5: Create inactive contract - doesn't affect active one
        // ========================================
        [Fact]
        public async Task CreateContractAsync_WithIsActiveFalse_DoesNotDeactivateExisting()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create first active contract
            var createDto1 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract1 = await service.CreateContractAsync(createDto1);
            int contract1Id = contract1.Id;

            // Create second inactive contract
            var createDto2 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2028, 1, 1),
                EndDate = new DateTime(2030, 12, 31),
                Salary = 60000,
                Position = "Forward",
                IsActive = false
            };

            var contract2 = await service.CreateContractAsync(createDto2);

            // Verify first contract is still active
            var contract1Updated = await service.GetContractByIdAsync(contract1Id);
            Assert.True(contract1Updated.IsActive);

            // Verify second contract is inactive
            Assert.False(contract2.IsActive);
        }

        // ========================================
        // SCENARIO 6: Query all contracts
        // ========================================
        [Fact]
        public async Task GetContractsAsync_WithMultipleContracts_ReturnsAll()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create three contracts
            for (int i = 0; i < 3; i++)
            {
                var createDto = new CreateContractDto
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    StartDate = new DateTime(2026 + (i * 2), 1, 1),
                    EndDate = new DateTime(2028 + (i * 2), 12, 31),
                    Salary = 50000 + (i * 10000),
                    Position = "Forward",
                    IsActive = (i == 2) // Only the last one should be active
                };

                await service.CreateContractAsync(createDto);
            }

            // Query all contracts
            var parameters = new ContractQueryParameters
            {
                PlayerId = player.Id,
                Page = 1,
                PageSize = 100
            };

            var result = await service.GetContractsAsync(parameters);

            // Should return all 3 contracts
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count());
        }

        // ========================================
        // SCENARIO 7: Delete contract
        // ========================================
        [Fact]
        public async Task DeleteContractAsync_WithValidId_DeletesSuccessfully()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            var createDto = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract = await service.CreateContractAsync(createDto);

            // Delete the contract
            var deleteResult = await service.DeleteContractAsync(contract.Id);
            Assert.True(deleteResult);

            // Verify it's deleted
            var retrieved = await service.GetContractByIdAsync(contract.Id);
            Assert.Null(retrieved);
        }

        // ========================================
        // SCENARIO 8: Update contract - activate inactive
        // ========================================
        [Fact]
        public async Task UpdateContractAsync_ActivatingInactiveContract_DeactivatesPrevious()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create first active contract
            var createDto1 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract1 = await service.CreateContractAsync(createDto1);
            int contract1Id = contract1.Id;

            // Create second inactive contract
            var createDto2 = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2028, 1, 1),
                EndDate = new DateTime(2030, 12, 31),
                Salary = 60000,
                Position = "Forward",
                IsActive = false
            };

            var contract2 = await service.CreateContractAsync(createDto2);
            int contract2Id = contract2.Id;

            // Update second contract to be active
            var updateDto = new UpdateContractDto
            {
                EndDate = new DateTime(2030, 12, 31),
                Salary = 60000,
                Position = "Forward",
                IsActive = true
            };

            await service.UpdateContractAsync(contract2Id, updateDto);

            // Verify first contract is deactivated
            var contract1Updated = await service.GetContractByIdAsync(contract1Id);
            Assert.False(contract1Updated.IsActive);

            // Verify second contract is active
            var contract2Updated = await service.GetContractByIdAsync(contract2Id);
            Assert.True(contract2Updated.IsActive);
        }

        // ========================================
        // SCENARIO 9: Update contract - deactivate active
        // ========================================
        [Fact]
        public async Task UpdateContractAsync_DeactivatingActiveContract_Succeeds()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            var createDto = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract = await service.CreateContractAsync(createDto);

            // Update to deactivate
            var updateDto = new UpdateContractDto
            {
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = false
            };

            var updated = await service.UpdateContractAsync(contract.Id, updateDto);

            Assert.False(updated.IsActive);
        }

        // ========================================
        // ADDITIONAL TESTS: Validation and Edge Cases
        // ========================================

        [Fact]
        public async Task GetContractByIdAsync_WithInvalidId_ReturnsNull()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var result = await service.GetContractByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteContractAsync_WithInvalidId_ReturnsFalse()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var result = await service.DeleteContractAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateContractAsync_WithInvalidId_ReturnsNull()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var updateDto = new UpdateContractDto
            {
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var result = await service.UpdateContractAsync(999, updateDto);

            Assert.Null(result);
        }

        // ========================================
        // CRITICAL RULE TEST: Only ONE active contract per player
        // ========================================

        [Fact]
        public async Task MultipleActiveContracts_CanNeverExist_ForSamePlayer()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create 5 active contracts - only last one should be active
            for (int i = 0; i < 5; i++)
            {
                var createDto = new CreateContractDto
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    StartDate = new DateTime(2026 + i, 1, 1),
                    EndDate = new DateTime(2027 + i, 12, 31),
                    Salary = 50000 + (i * 5000),
                    Position = "Forward",
                    IsActive = true
                };

                await service.CreateContractAsync(createDto);
            }

            // Query active contracts for this player
            var activeResult = await service.GetActiveContractsAsync(page: 1, pageSize: 100);

            // Should have exactly 1 active contract
            var activeForPlayer = activeResult.Items.Where(c => c.PlayerId == player.Id).ToList();
            Assert.Single(activeForPlayer);
        }

        [Fact]
        public async Task GetContractsAsync_FilterByPlayerId_ReturnsOnlyForThatPlayer()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player1, club) = CreateTestPlayerAndClub(context);

            // Create second player
            var player2 = new Player
            {
                FirstName = "Jane",
                LastName = "Smith",
                JerseyNumber = 11,
                Position = "Midfielder",
                DateOfBirth = new DateTime(1995, 5, 5),
                Nationality = "France",
                Height = 170.0m,
                Weight = 65.0m
            };
            context.Players.Add(player2);
            context.SaveChanges();

            var service = CreateService(context);

            // Create contract for player 1
            var createDto1 = new CreateContractDto
            {
                PlayerId = player1.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };
            await service.CreateContractAsync(createDto1);

            // Create contract for player 2
            var createDto2 = new CreateContractDto
            {
                PlayerId = player2.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 40000,
                Position = "Midfielder",
                IsActive = true
            };
            await service.CreateContractAsync(createDto2);

            // Query for player 1 only
            var parameters = new ContractQueryParameters
            {
                PlayerId = player1.Id,
                Page = 1,
                PageSize = 100
            };

            var result = await service.GetContractsAsync(parameters);

            Assert.Single(result.Items);
            Assert.Equal(player1.Id, result.Items.First().PlayerId);
        }

        [Fact]
        public async Task GetActiveContractsAsync_Pagination_Works()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var players = new Player[5];
            for (int i = 0; i < 5; i++)
            {
                players[i] = new Player
                {
                    FirstName = $"Player{i}",
                    LastName = "Test",
                    JerseyNumber = i + 1,
                    Position = "Forward",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Nationality = "England",
                    Height = 180.0m,
                    Weight = 75.0m
                };
            }

            var club = new Club
            {
                Name = "Test Club",
                City = "London",
                FoundedYear = 2000,
                Budget = 1000000m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = "test-user-id"
            };

            context.Players.AddRange(players);
            context.Clubs.Add(club);
            context.SaveChanges();

            var service = CreateService(context);

            // Create active contracts for all players
            foreach (var player in players)
            {
                var createDto = new CreateContractDto
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2028, 12, 31),
                    Salary = 50000,
                    Position = "Forward",
                    IsActive = true
                };
                await service.CreateContractAsync(createDto);
            }

            // Get first page with pageSize 2
            var page1 = await service.GetActiveContractsAsync(page: 1, pageSize: 2);
            Assert.Equal(2, page1.Items.Count());
            Assert.Equal(5, page1.TotalCount);

            // Get second page
            var page2 = await service.GetActiveContractsAsync(page: 2, pageSize: 2);
            Assert.Equal(2, page2.Items.Count());

            // Get third page
            var page3 = await service.GetActiveContractsAsync(page: 3, pageSize: 2);
            Assert.Single(page3.Items);
        }

        [Fact]
        public async Task GetExpiringContractsAsync_ReturnsContractsExpiringWithinDays()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            // Create contract expiring in 30 days
            var expiringSoon = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2024, 1, 1),
                EndDate = DateTime.UtcNow.AddDays(30),
                Salary = 50000,
                Position = "Forward",
                IsActive = true
            };

            var contract1 = await service.CreateContractAsync(expiringSoon);

            // Create another contract expiring in 200 days (far in the future)
            var player2 = new Player
            {
                FirstName = "Far",
                LastName = "Future",
                JerseyNumber = 20,
                Position = "Defender",
                DateOfBirth = new DateTime(1992, 1, 1),
                Nationality = "Germany",
                Height = 190.0m,
                Weight = 85.0m
            };
            context.Players.Add(player2);
            context.SaveChanges();

            var notExpiringSoon = new CreateContractDto
            {
                PlayerId = player2.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2024, 1, 1),
                EndDate = DateTime.UtcNow.AddDays(200),
                Salary = 60000,
                Position = "Defender",
                IsActive = true
            };

            await service.CreateContractAsync(notExpiringSoon);

            // Query for contracts expiring within 90 days
            var result = await service.GetExpiringContractsAsync(days: 90, page: 1, pageSize: 100);

            // Should only return the first contract
            Assert.Single(result.Items);
            Assert.Equal(contract1.Id, result.Items.First().Id);
        }

        [Fact]
        public async Task ContractDto_MapsCorrectly_FromContract()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var (player, club) = CreateTestPlayerAndClub(context);

            var service = CreateService(context);

            var createDto = new CreateContractDto
            {
                PlayerId = player.Id,
                ClubId = club.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2028, 12, 31),
                Salary = 75500.50m,
                Position = "Striker",
                IsActive = true
            };

            var contractDto = await service.CreateContractAsync(createDto);

            // Verify all properties are mapped correctly
            Assert.NotEqual(0, contractDto.Id);
            Assert.Equal(player.Id, contractDto.PlayerId);
            Assert.Equal(club.Id, contractDto.ClubId);
            Assert.Equal(new DateTime(2026, 1, 1), contractDto.StartDate);
            Assert.Equal(new DateTime(2028, 12, 31), contractDto.EndDate);
            Assert.Equal(75500.50m, contractDto.Salary);
            Assert.Equal("Striker", contractDto.Position);
            Assert.True(contractDto.IsActive);

            // Verify navigation properties
            Assert.NotNull(contractDto.Player);
            Assert.Equal(player.FirstName, contractDto.Player.FirstName);
            Assert.NotNull(contractDto.Club);
            Assert.Equal(club.Name, contractDto.Club.Name);
        }
    }
}
