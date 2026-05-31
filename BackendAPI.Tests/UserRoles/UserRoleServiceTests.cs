using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FootballClubAPI.Controllers;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackendAPI.Tests.UserRoles
{
    public class UserRoleServiceTests
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

            services.AddScoped<IUserRoleService, UserRoleService>();

            return services.BuildServiceProvider();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in RoleConstants.BuiltInRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, string email)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                Role = RoleConstants.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, "Pass@word1");
            Assert.True(createResult.Succeeded);
            return user;
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsAllAssignedRoles()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var user = await CreateUserAsync(userManager, "multi@test.com");
            await userManager.AddToRoleAsync(user, RoleConstants.Admin);
            await userManager.AddToRoleAsync(user, RoleConstants.Manager);

            var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
            var result = await service.GetUserRolesAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.UserId);
            Assert.Equal("multi@test.com", result.Email);
            Assert.Contains(RoleConstants.Admin, result.Roles);
            Assert.Contains(RoleConstants.Manager, result.Roles);
        }

        [Fact]
        public async Task AssignRoleAsync_AssignsRoleToUser()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var user = await CreateUserAsync(userManager, "assign@test.com");
            var managerRole = await roleManager.FindByNameAsync(RoleConstants.Manager);
            Assert.NotNull(managerRole);

            var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
            var (result, assignment) = await service.AssignRoleAsync(user.Id, managerRole!.Id);

            Assert.True(result.Succeeded);
            Assert.NotNull(assignment);
            Assert.Equal(RoleConstants.Manager, assignment!.RoleName);
            Assert.True(await userManager.IsInRoleAsync(user, RoleConstants.Manager));
        }

        [Fact]
        public async Task AssignRoleAsync_WithDuplicateRole_ReturnsConflictSignal()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var user = await CreateUserAsync(userManager, "duplicate@test.com");
            await userManager.AddToRoleAsync(user, RoleConstants.Coach);

            var coachRole = await roleManager.FindByNameAsync(RoleConstants.Coach);
            Assert.NotNull(coachRole);

            var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
            var (result, assignment) = await service.AssignRoleAsync(user.Id, coachRole!.Id);

            Assert.False(result.Succeeded);
            Assert.Null(assignment);
            Assert.Contains(result.Errors, error => error.Code == "DuplicateRoleAssignment");
        }

        [Fact]
        public async Task AssignRoleAsync_WithMissingUser_ReturnsNotFoundSignal()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var role = await roleManager.FindByNameAsync(RoleConstants.User);
            Assert.NotNull(role);

            var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
            var (result, assignment) = await service.AssignRoleAsync(Guid.NewGuid().ToString(), role!.Id);

            Assert.False(result.Succeeded);
            Assert.Null(assignment);
            Assert.Contains(result.Errors, error => error.Code == "NotFound");
        }
    }
}