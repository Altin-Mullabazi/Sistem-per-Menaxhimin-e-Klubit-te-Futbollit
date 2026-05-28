using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.Controllers;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace BackendAPI.Tests.Users
{
    public class UsersControllerTests
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
            foreach (var roleName in new[] { "Admin", "Manager", "Fan" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string firstName,
            string lastName,
            string role)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, "Pass@word1");
            Assert.True(result.Succeeded);
            await userManager.AddToRoleAsync(user, role);

            return user;
        }

        private static UsersController CreateController(IUserService service, string userId, params string[] roles)
        {
            var controller = new UsersController(service, NullLogger<UsersController>.Instance);
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            return controller;
        }

        [Fact]
        public void GetUsers_HasAdminOnlyAuthorizeAttribute()
        {
            var method = typeof(UsersController).GetMethod(nameof(UsersController.GetUsers))!;
            var authorize = method.GetCustomAttributes<AuthorizeAttribute>(true).SingleOrDefault();

            Assert.NotNull(authorize);
            Assert.Equal("Admin", authorize!.Roles);
        }

        [Fact]
        public void CreateUser_HasAdminOnlyAuthorizeAttribute()
        {
            var method = typeof(UsersController).GetMethod(nameof(UsersController.CreateUser))!;
            var authorize = method.GetCustomAttributes<AuthorizeAttribute>(true).SingleOrDefault();

            Assert.NotNull(authorize);
            Assert.Equal("Admin", authorize!.Roles);
        }

        [Fact]
        public void DeleteUser_HasAdminOnlyAuthorizeAttribute()
        {
            var method = typeof(UsersController).GetMethod(nameof(UsersController.DeleteUser))!;
            var authorize = method.GetCustomAttributes<AuthorizeAttribute>(true).SingleOrDefault();

            Assert.NotNull(authorize);
            Assert.Equal("Admin", authorize!.Roles);
        }

        [Fact]
        public async Task GetUserById_AnotherUser_ReturnsForbid()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var owner = await CreateUserAsync(userManager, "owner@test.com", "Owner", "User", "Fan");
            await CreateUserAsync(userManager, "other@test.com", "Other", "User", "Fan");

            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IUserService>(), owner.Id, "Fan");
            var result = await controller.GetUserById(Guid.NewGuid().ToString());

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUser_AnotherUser_ReturnsForbid()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var owner = await CreateUserAsync(userManager, "owner@test.com", "Owner", "User", "Fan");
            var other = await CreateUserAsync(userManager, "other@test.com", "Other", "User", "Fan");

            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IUserService>(), owner.Id, "Fan");
            var result = await controller.UpdateUser(other.Id, new UpdateUserDto
            {
                Email = "updated@test.com",
                FirstName = "Updated",
                LastName = "User"
            });

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteUser_SelfDelete_ReturnsBadRequest()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var admin = await CreateUserAsync(userManager, "admin@test.com", "Admin", "User", "Admin");
            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IUserService>(), admin.Id, "Admin");

            var result = await controller.DeleteUser(admin.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateUser_WithInvalidModelState_ReturnsBadRequest()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await SeedRolesAsync(roleManager);

            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IUserService>(), Guid.NewGuid().ToString(), "Admin");
            controller.ModelState.AddModelError("Email", "Email is required");

            var result = await controller.CreateUser(new CreateUserDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}