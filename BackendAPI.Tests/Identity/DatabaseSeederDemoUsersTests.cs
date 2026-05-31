using System;
using System.Linq;
using System.Threading.Tasks;
using FootballClubAPI.Data;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendAPI.Tests.Identity
{
    public class DatabaseSeederDemoUsersTests
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

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task SeedDataAsync_CreatesDemoUsersWithExpectedRoles()
        {
            using var provider = BuildProvider(Guid.NewGuid().ToString());
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedDataAsync(context, userManager, roleManager);

            var demoUsers = new (string Email, string Role)[]
            {
                ("admin@email.com", RoleConstants.Admin),
                ("manager@email.com", RoleConstants.Manager),
                ("coach@email.com", RoleConstants.Coach),
                ("user@email.com", RoleConstants.User)
            };

            foreach (var (email, role) in demoUsers)
            {
                var user = await userManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                Assert.True(await userManager.IsInRoleAsync(user!, role));
            }
        }
    }
}