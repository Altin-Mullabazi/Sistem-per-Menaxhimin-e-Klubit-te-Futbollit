using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendAPI.Tests.Services
{
    public class SeasonServiceTests
    {
        private static ApplicationDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static SeasonService CreateService(ApplicationDbContext context)
        {
            var logger = new Mock<ILogger<SeasonService>>().Object;
            return new SeasonService(context, logger);
        }

        [Fact]
        public async Task GetSeasonsAsync_WithValidPagination_ReturnsPagedSeasons()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            context.Seasons.AddRange(
                new Season { Name = "2024/2025", StartDate = new DateTime(2024, 8, 1), EndDate = new DateTime(2025, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Season { Name = "2025/2026", StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2026, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Season { Name = "2026/2027", StartDate = new DateTime(2026, 8, 1), EndDate = new DateTime(2027, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var (seasons, totalCount) = await service.GetSeasonsAsync(page: 1, pageSize: 2);

            Assert.Equal(2, seasons.Count());
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public async Task GetSeasonsAsync_WithPageGreater1_ReturnsCorrectPage()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            context.Seasons.AddRange(
                new Season { Name = "2024/2025", StartDate = new DateTime(2024, 8, 1), EndDate = new DateTime(2025, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Season { Name = "2025/2026", StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2026, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Season { Name = "2026/2027", StartDate = new DateTime(2026, 8, 1), EndDate = new DateTime(2027, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var (seasons, totalCount) = await service.GetSeasonsAsync(page: 2, pageSize: 2);

            Assert.Single(seasons);
            Assert.Equal("2024/2025", seasons.Single().Name);
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public async Task GetSeasonByIdAsync_WithValidId_ReturnsSeasonDto()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var season = new Season { Name = "2024/2025", StartDate = new DateTime(2024, 8, 1), EndDate = new DateTime(2025, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetSeasonByIdAsync(season.Id);

            Assert.NotNull(result);
            Assert.Equal(season.Name, result!.Name);
        }

        [Fact]
        public async Task GetSeasonByIdAsync_WithInvalidId_ReturnsNull()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var result = await service.GetSeasonByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateSeasonAsync_WithValidData_CreatesSuccessfully()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);
            var dto = new CreateSeasonDto
            {
                Name = "2027/2028",
                StartDate = new DateTime(2027, 8, 1),
                EndDate = new DateTime(2028, 5, 31),
                Description = "A new season"
            };

            var result = await service.CreateSeasonAsync(dto);
            var seasons = await context.Seasons.ToListAsync();

            Assert.Equal(dto.Name, result.Name);
            Assert.Single(seasons);
            Assert.Equal(dto.Description, seasons.Single().Description);
        }

        [Fact]
        public async Task CreateSeasonAsync_StartDateAfterEndDate_ThrowsException()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);
            var dto = new CreateSeasonDto
            {
                Name = "2027/2028",
                StartDate = new DateTime(2028, 6, 1),
                EndDate = new DateTime(2028, 5, 31)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSeasonAsync(dto));
        }

        [Fact]
        public async Task CreateSeasonAsync_DuplicateName_ThrowsException()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            context.Seasons.Add(new Season { Name = "2027/2028", StartDate = new DateTime(2027, 8, 1), EndDate = new DateTime(2028, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateSeasonDto
            {
                Name = "2027/2028",
                StartDate = new DateTime(2028, 8, 1),
                EndDate = new DateTime(2029, 5, 31)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSeasonAsync(dto));
        }

        [Fact]
        public async Task CreateSeasonAsync_OverlappingDates_ThrowsException()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            context.Seasons.Add(new Season { Name = "2027/2028", StartDate = new DateTime(2027, 8, 1), EndDate = new DateTime(2028, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateSeasonDto
            {
                Name = "2028/2029",
                StartDate = new DateTime(2028, 5, 1),
                EndDate = new DateTime(2029, 5, 31)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSeasonAsync(dto));
        }

        [Fact]
        public async Task UpdateSeasonAsync_WithValidData_UpdatesSuccessfully()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var season = new Season { Name = "2027/2028", StartDate = new DateTime(2027, 8, 1), EndDate = new DateTime(2028, 5, 31), Description = "Old season", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new UpdateSeasonDto
            {
                Name = "2027/2028 Updated",
                StartDate = new DateTime(2027, 8, 15),
                EndDate = new DateTime(2028, 6, 15),
                Description = "Updated season"
            };

            var result = await service.UpdateSeasonAsync(season.Id, dto);
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result!.Name);
            Assert.Equal(dto.Description, result.Description);
        }

        [Fact]
        public async Task UpdateSeasonAsync_InvalidDateRange_ThrowsException()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var season = new Season { Name = "2027/2028", StartDate = new DateTime(2027, 8, 1), EndDate = new DateTime(2028, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new UpdateSeasonDto
            {
                Name = "2027/2028",
                StartDate = new DateTime(2028, 6, 1),
                EndDate = new DateTime(2028, 5, 31)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateSeasonAsync(season.Id, dto));
        }

        [Fact]
        public async Task DeleteSeasonAsync_WithValidId_DeletesSuccessfully()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var season = new Season { Name = "2027/2028", StartDate = new DateTime(2027, 8, 1), EndDate = new DateTime(2028, 5, 31), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var deleted = await service.DeleteSeasonAsync(season.Id);

            Assert.True(deleted);
            Assert.Empty(await context.Seasons.ToListAsync());
        }

        [Fact]
        public async Task DeleteSeasonAsync_WithInvalidId_ReturnsFalse()
        {
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            var service = CreateService(context);

            var deleted = await service.DeleteSeasonAsync(999);

            Assert.False(deleted);
        }
    }
}
