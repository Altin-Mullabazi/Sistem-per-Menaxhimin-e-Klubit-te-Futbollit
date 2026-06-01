using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendAPI.Tests.Roles
{
    public class RolesServiceTests
    {
        private static ServiceProvider BuildProvider(string databaseName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            services.AddLogging();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IRoleService, RoleService>();

            return services.BuildServiceProvider();
        }

        private static async Task SeedUserRoleAsync(IServiceProvider services, string email, string roleName)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                Role = roleName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, "Pass@word1");
            Assert.True(createResult.Succeeded);
            await userManager.AddToRoleAsync(user, roleName);
        }

        [Fact]
        public async Task GetRolesAsync_ReturnsSeededBuiltInRoles()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var roles = await service.GetRolesAsync();

            Assert.Equal(4, roles.Count);
            Assert.Contains(roles, role => role.Name == "Admin");
            Assert.Contains(roles, role => role.Name == "Manager");
            Assert.Contains(roles, role => role.Name == "Coach");
            Assert.Contains(roles, role => role.Name == "User");
        }

        [Fact]
        public async Task CreateRoleAsync_WithUniqueName_CreatesRole()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var (result, role) = await service.CreateRoleAsync(new CreateRoleDto { Name = "Physio" });

            Assert.True(result.Succeeded);
            Assert.NotNull(role);
            Assert.Equal("Physio", role!.Name);
            Assert.NotNull(await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().FindByNameAsync("Physio"));
        }

        [Fact]
        public async Task CreateRoleAsync_WithDuplicateName_ReturnsValidationError()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var result = await service.CreateRoleAsync(new CreateRoleDto { Name = "Coach" });

            Assert.False(result.Result.Succeeded);
            Assert.Contains(result.Result.Errors, error => error.Code == "DuplicateRoleName");
        }

        [Fact]
        public async Task UpdateRoleAsync_WithBuiltInRole_ReturnsValidationError()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var builtIn = await context.Roles.FirstAsync(role => role.Name == "User");
            var service = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var result = await service.UpdateRoleAsync(builtIn.Id, new UpdateRoleDto { Name = "StandardUser" });

            Assert.False(result.Result.Succeeded);
            Assert.Contains(result.Result.Errors, error => error.Code == "BuiltInRole");
        }

        [Fact]
        public async Task DeleteRoleAsync_WithAssignedUsers_ReturnsValidationError()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            await roleManager.CreateAsync(new IdentityRole { Name = "AcademyStaff", NormalizedName = "ACADEMYSTAFF" });
            await SeedUserRoleAsync(scope.ServiceProvider, "staff@test.com", "AcademyStaff");

            var role = await roleManager.FindByNameAsync("AcademyStaff");
            var service = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var result = await service.DeleteRoleAsync(role!.Id);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Code == "RoleAssignedToUsers");
        }
    }
}