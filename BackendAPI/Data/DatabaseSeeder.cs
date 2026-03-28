using FootballClubAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace FootballClubAPI.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(ApplicationDbContext context)
        {
            // Check if data already exists
            if (context.Users.Any() && context.Players.Any())
                return;

            // Seed one demo user
            if (!context.Users.Any())
            {
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    Email = "admin@footballclub.com",
                    PasswordHash = HashPassword("Admin@123"),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.Users.Add(user);
                context.SaveChanges();
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
                        Age = 38,
                        Position = "Forward",
                        ClubName = "Al Nassr",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Lionel",
                        LastName = "Messi",
                        Age = 36,
                        Position = "Forward",
                        ClubName = "Inter Miami",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Kylian",
                        LastName = "Mbappé",
                        Age = 25,
                        Position = "Forward",
                        ClubName = "Real Madrid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Vinicius",
                        LastName = "Junior",
                        Age = 24,
                        Position = "Left Winger",
                        ClubName = "Real Madrid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Player
                    {
                        FirstName = "Jude",
                        LastName = "Bellingham",
                        Age = 21,
                        Position = "Midfielder",
                        ClubName = "Real Madrid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.Players.AddRange(players);
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
