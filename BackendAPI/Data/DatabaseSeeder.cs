using FootballClubAPI.Models;
using FootballClubAPI.Helpers;
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
            var legacyFanRoleName = "Fan";
            var demoUsers = new[]
            {
                new { Email = "admin@email.com", Role = RoleConstants.Admin, FirstName = "System", LastName = "Admin" },
                new { Email = "manager@email.com", Role = RoleConstants.Manager, FirstName = "System", LastName = "Manager" },
                new { Email = "coach@email.com", Role = RoleConstants.Coach, FirstName = "System", LastName = "Coach" },
                new { Email = "user@email.com", Role = RoleConstants.User, FirstName = "System", LastName = "User" }
            };

            var roles = RoleConstants.BuiltInRoles;
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    });
                }
            }

            var legacyFanRole = await roleManager.FindByNameAsync(legacyFanRoleName);
            if (legacyFanRole != null)
            {
                var userRole = await roleManager.FindByNameAsync(RoleConstants.User);
                if (userRole == null)
                {
                    legacyFanRole.Name = RoleConstants.User;
                    legacyFanRole.NormalizedName = RoleConstants.User.ToUpperInvariant();
                    await roleManager.UpdateAsync(legacyFanRole);
                }
                else
                {
                    var usersInLegacyRole = await userManager.GetUsersInRoleAsync(legacyFanRoleName);
                    foreach (var legacyUser in usersInLegacyRole)
                    {
                        if (!await userManager.IsInRoleAsync(legacyUser, RoleConstants.User))
                        {
                            await userManager.AddToRoleAsync(legacyUser, RoleConstants.User);
                        }

                        await userManager.RemoveFromRoleAsync(legacyUser, legacyFanRoleName);
                    }

                    await roleManager.DeleteAsync(legacyFanRole);
                }
            }

            foreach (var demoUser in demoUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(demoUser.Email);

                if (existingUser == null)
                {
                    existingUser = new ApplicationUser
                    {
                        UserName = demoUser.Email,
                        Email = demoUser.Email,
                        FirstName = demoUser.FirstName,
                        LastName = demoUser.LastName,
                        FullName = $"{demoUser.FirstName} {demoUser.LastName}",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Role = demoUser.Role
                    };

                    var createResult = await userManager.CreateAsync(existingUser, "Pass@word123");
                    if (!createResult.Succeeded)
                    {
                        continue;
                    }
                }

                if (!await userManager.IsInRoleAsync(existingUser, demoUser.Role))
                {
                    await userManager.AddToRoleAsync(existingUser, demoUser.Role);
                }

                existingUser.Role = demoUser.Role;
                existingUser.UpdatedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(existingUser);

                var legacyUser = await context.LegacyUsers.FirstOrDefaultAsync(user =>
                    user.Id == existingUser.Id ||
                    user.Email == existingUser.Email ||
                    user.Username == existingUser.UserName);

                if (legacyUser == null)
                {
                    context.LegacyUsers.Add(new User
                    {
                        Id = existingUser.Id,
                        Username = existingUser.UserName ?? existingUser.Email ?? demoUser.Email,
                        Email = existingUser.Email ?? demoUser.Email,
                        PasswordHash = existingUser.PasswordHash ?? string.Empty,
                        FirstName = existingUser.FirstName,
                        LastName = existingUser.LastName,
                        Role = demoUser.Role,
                        EmailVerified = true,
                        CreatedAt = existingUser.CreatedAt,
                        UpdatedAt = existingUser.UpdatedAt,
                        IsActive = true
                    });
                }
                else
                {
                    legacyUser.Username = existingUser.UserName ?? legacyUser.Username;
                    legacyUser.Email = existingUser.Email ?? legacyUser.Email;
                    legacyUser.PasswordHash = existingUser.PasswordHash ?? legacyUser.PasswordHash;
                    legacyUser.FirstName = existingUser.FirstName;
                    legacyUser.LastName = existingUser.LastName;
                    legacyUser.Role = demoUser.Role;
                    legacyUser.EmailVerified = true;
                    legacyUser.UpdatedAt = DateTime.UtcNow;
                    legacyUser.IsActive = true;
                }
            }

            await context.SaveChangesAsync();

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
