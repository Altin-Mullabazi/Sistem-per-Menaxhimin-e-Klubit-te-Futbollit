using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "Manager", "Fan" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@footballclub.com";
            var adminUser = await userManager.Users.FirstOrDefaultAsync(user => user.Email == adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Players
            if (!context.Players.Any())
            {
                var players = new List<Player>
                {
                    new Player
                    {
                        FirstName = "Cristiano",
                        LastName = "Ronaldo",
                        DateOfBirth = new DateTime(1985, 2, 5),
                        Nationality = "Portuguese",
                        JerseyNumber = 7,
                        Position = "Forward",
                        Height = 1.87m,
                        Weight = 84m,
                        Status = PlayerStatus.Active,
                        MarketValue = 10000000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Lionel",
                        LastName = "Messi",
                        DateOfBirth = new DateTime(1987, 6, 24),
                        Nationality = "Argentine",
                        JerseyNumber = 10,
                        Position = "Forward",
                        Height = 1.70m,
                        Weight = 72m,
                        Status = PlayerStatus.Active,
                        MarketValue = 15000000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Kylian",
                        LastName = "Mbappé",
                        DateOfBirth = new DateTime(1998, 12, 20),
                        Nationality = "French",
                        JerseyNumber = 9,
                        Position = "Forward",
                        Height = 1.78m,
                        Weight = 73m,
                        Status = PlayerStatus.Active,
                        MarketValue = 180000000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Vinicius",
                        LastName = "Junior",
                        DateOfBirth = new DateTime(2000, 7, 12),
                        Nationality = "Brazilian",
                        JerseyNumber = 11,
                        Position = "Left Winger",
                        Height = 1.80m,
                        Weight = 73m,
                        Status = PlayerStatus.Active,
                        MarketValue = 120000000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Jude",
                        LastName = "Bellingham",
                        DateOfBirth = new DateTime(2003, 6, 17),
                        Nationality = "English",
                        JerseyNumber = 5,
                        Position = "Midfielder",
                        Height = 1.86m,
                        Weight = 75m,
                        Status = PlayerStatus.Active,
                        MarketValue = 100000000m,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.Players.AddRange(players);
                await context.SaveChangesAsync();
            }
        }
    }
}
