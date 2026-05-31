using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FootballClubAPI.Controllers;
using FootballClubAPI.Data;
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
using Xunit;

namespace BackendAPI.Tests.Roles
{
    public class RolesControllerTests
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

        private static RolesController CreateController(IRoleService service, params Claim[] claims)
        {
            var controller = new RolesController(service, NullLogger<RolesController>.Instance);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, claims.Length == 0 ? string.Empty : "TestAuth"))
                }
            };

            return controller;
        }

        [Fact]
        public void RolesController_HasAdminAuthorizeAttribute()
        {
            var authorize = typeof(RolesController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            Assert.NotNull(authorize);
            Assert.Equal("Admin", authorize!.Roles);
        }

        [Fact]
        public async Task GetRoles_WithUnauthenticatedUser_ReturnsUnauthorized()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IRoleService>());
            var result = await controller.GetRoles();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithNonAdminUser_ReturnsForbid()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User"));

            var result = await controller.GetRoles();

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetRoles_ReturnsSeededRoles()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"));

            var result = await controller.GetRoles() as OkObjectResult;

            Assert.NotNull(result);
            var payload = result!.Value!;
            var dataProperty = payload.GetType().GetProperty("data");
            var roles = ((IEnumerable<RoleDto>)dataProperty!.GetValue(payload)!).ToList();

            Assert.Equal(4, roles.Count);
            Assert.Contains(roles, role => role.Name == "Admin");
            Assert.Contains(roles, role => role.Name == "Manager");
            Assert.Contains(roles, role => role.Name == "Coach");
            Assert.Contains(roles, role => role.Name == "User");
        }

        [Fact]
        public async Task CreateRole_WithValidPayload_CreatesRole()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"));

            var result = await controller.CreateRole(new CreateRoleDto { Name = "Physio" }) as CreatedAtActionResult;

            Assert.NotNull(result);
            var payload = result!.Value!;
            var dataProperty = payload.GetType().GetProperty("data");
            var role = (RoleDto)dataProperty!.GetValue(payload)!;
            Assert.Equal("Physio", role.Name);
        }

        [Fact]
        public async Task UpdateRole_WithBuiltInRole_ReturnsBadRequest()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();
            var builtIn = await context.Roles.FirstAsync(role => role.Name == "User");

            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"));

            var result = await controller.UpdateRole(builtIn.Id, new UpdateRoleDto { Name = "StandardUser" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteRole_WithAssignedUsers_ReturnsBadRequest()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            var role = await roleManager.CreateAsync(new IdentityRole { Name = "AcademyStaff", NormalizedName = "ACADEMYSTAFF" });
            Assert.True(role.Succeeded);

            var user = new ApplicationUser
            {
                UserName = "staff@test.com",
                Email = "staff@test.com",
                FirstName = "Staff",
                LastName = "User",
                FullName = "Staff User",
                Role = "AcademyStaff",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, "Pass@word1");
            Assert.True(createResult.Succeeded);
            await userManager.AddToRoleAsync(user, "AcademyStaff");

            var roleEntity = await roleManager.FindByNameAsync("AcademyStaff");
            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"));

            var result = await controller.DeleteRole(roleEntity!.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}