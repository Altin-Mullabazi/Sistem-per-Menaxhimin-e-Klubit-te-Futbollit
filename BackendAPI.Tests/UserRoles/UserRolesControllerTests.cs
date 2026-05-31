using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FootballClubAPI.Controllers;
using FootballClubAPI.Data;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackendAPI.Tests.UserRoles
{
    public class UserRolesControllerTests
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

        private static UserRolesController CreateController(IUserRoleService service, params Claim[] claims)
        {
            var controller = new UserRolesController(service, NullLogger<UserRolesController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, claims.Length == 0 ? string.Empty : "TestAuth"))
                    }
                }
            };

            return controller;
        }

        [Fact]
        public void UserRolesController_HasAdminAuthorizeAttribute()
        {
            var authorize = typeof(UserRolesController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            Assert.NotNull(authorize);
            Assert.Equal("Admin", authorize!.Roles);
        }

        [Fact]
        public async Task GetUserRoles_WithUnauthenticatedUser_ReturnsUnauthorized()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(scope.ServiceProvider.GetRequiredService<IUserRoleService>());
            var result = await controller.GetUserRoles(Guid.NewGuid().ToString());

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetUserRoles_WithNonAdminUser_ReturnsForbid()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            var controller = CreateController(
                scope.ServiceProvider.GetRequiredService<IUserRoleService>(),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, RoleConstants.User));

            var result = await controller.GetUserRoles(Guid.NewGuid().ToString());

            Assert.IsType<ForbidResult>(result);
        }
    }
}