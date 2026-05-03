using FootballClubAPI.Models;

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
                    FirstName = "System",
                    LastName = "Admin",
                    Role = "Admin",
                    EmailVerified = true,
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
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }
    }
}
