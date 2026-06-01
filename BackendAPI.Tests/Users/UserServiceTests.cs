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

namespace BackendAPI.Tests.Users
{
    public class UserServiceTests
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

            services.AddScoped<IUserService, UserService>();

            return services.BuildServiceProvider();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { "Admin", "Manager", "Coach", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            string email,
            string firstName,
            string lastName,
            string role,
            DateTime createdAt)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Role = role,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, "Pass@word1");
            Assert.True(result.Succeeded);
            await userManager.AddToRoleAsync(user, role);

            return user;
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsPaginatedUsersOrderedByCreatedAtDesc()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var oldest = await CreateUserAsync(context, userManager, "oldest@test.com", "Old", "User", "User", DateTime.UtcNow.AddDays(-3));
            var middle = await CreateUserAsync(context, userManager, "middle@test.com", "Middle", "User", "User", DateTime.UtcNow.AddDays(-2));
            var newest = await CreateUserAsync(context, userManager, "newest@test.com", "Newest", "User", "User", DateTime.UtcNow.AddDays(-1));

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.GetUsersAsync(page: 1, pageSize: 2);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(newest.Id, result.Items.First().Id);
            Assert.Equal(middle.Id, result.Items.Skip(1).First().Id);
        }

        [Fact]
        public async Task GetUsersAsync_SearchIsCaseInsensitive()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var target = await CreateUserAsync(context, userManager, "maria.rossi@test.com", "Maria", "Rossi", "User", DateTime.UtcNow);
            await CreateUserAsync(context, userManager, "john.smith@test.com", "John", "Smith", "User", DateTime.UtcNow.AddHours(-1));

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.GetUsersAsync(search: "MARIA");

            Assert.Single(result.Items);
            Assert.Equal(target.Id, result.Items.Single().Id);
        }

        [Fact]
        public async Task CreateUserAsync_WithValidData_CreatesUserAndAssignsRole()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.CreateUserAsync(new CreateUserDto
            {
                Email = "create@test.com",
                Password = "Pass@word1",
                FirstName = "Create",
                LastName = "User",
                Role = "User"
            });

            Assert.True(result.Result.Succeeded);
            Assert.NotNull(result.User);
            Assert.Equal("create@test.com", result.User!.Email);
            Assert.Equal("Create", result.User.FirstName);
            Assert.Equal("User", result.User.LastName);
            Assert.Equal("User", result.User.Role);

            var created = await context.Users.FirstOrDefaultAsync(user => user.Email == "create@test.com");
            Assert.NotNull(created);
            var roles = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().GetRolesAsync(created!);
            Assert.Contains("User", roles);
        }

        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ReturnsDuplicateEmailError()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var first = await service.CreateUserAsync(new CreateUserDto
            {
                Email = "dup@test.com",
                Password = "Pass@word1",
                FirstName = "First",
                LastName = "User",
                Role = "User"
            });

            var second = await service.CreateUserAsync(new CreateUserDto
            {
                Email = "dup@test.com",
                Password = "Pass@word1",
                FirstName = "Second",
                LastName = "User",
                Role = "User"
            });

            Assert.True(first.Result.Succeeded);
            Assert.False(second.Result.Succeeded);
            Assert.Contains(second.Result.Errors, error => error.Code == "DuplicateEmail");
        }

        [Fact]
        public async Task CreateUserAsync_WithInvalidRole_ReturnsValidationError()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.CreateUserAsync(new CreateUserDto
            {
                Email = "role@test.com",
                Password = "Pass@word1",
                FirstName = "Role",
                LastName = "Test",
                Role = "InvalidRole"
            });

            Assert.False(result.Result.Succeeded);
            Assert.Contains(result.Result.Errors, error => error.Code == "InvalidRole");
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidData_UpdatesUser()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var existing = await CreateUserAsync(context, userManager, "update@test.com", "Old", "Name", "User", DateTime.UtcNow.AddDays(-1));
            var service = scope.ServiceProvider.GetRequiredService<IUserService>();

            var result = await service.UpdateUserAsync(existing.Id, new UpdateUserDto
            {
                Email = "updated@test.com",
                FirstName = "New",
                LastName = "Name"
            });

            Assert.True(result.Result.Succeeded);
            Assert.NotNull(result.User);
            Assert.Equal("updated@test.com", result.User!.Email);
            Assert.Equal("New", result.User.FirstName);

            var updated = await userManager.FindByIdAsync(existing.Id);
            Assert.NotNull(updated);
            Assert.Equal("updated@test.com", updated!.Email);
            Assert.Equal("New", updated.FirstName);
        }

        [Fact]
        public async Task UpdateUserAsync_WithMissingUser_ReturnsNotFound()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.UpdateUserAsync("missing-id", new UpdateUserDto
            {
                Email = "missing@test.com",
                FirstName = "Missing",
                LastName = "User"
            });

            Assert.False(result.Result.Succeeded);
            Assert.Contains(result.Result.Errors, error => error.Code == "NotFound");
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_DeletesUser()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var existing = await CreateUserAsync(context, userManager, "delete@test.com", "Delete", "Me", "User", DateTime.UtcNow);
            var service = scope.ServiceProvider.GetRequiredService<IUserService>();

            var result = await service.DeleteUserAsync(existing.Id);

            Assert.True(result.Succeeded);
            Assert.Null(await userManager.FindByIdAsync(existing.Id));
        }

        [Fact]
        public async Task DeleteUserAsync_WithMissingId_ReturnsNotFound()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            var result = await service.DeleteUserAsync("missing-id");

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Code == "NotFound");
        }
    }
}