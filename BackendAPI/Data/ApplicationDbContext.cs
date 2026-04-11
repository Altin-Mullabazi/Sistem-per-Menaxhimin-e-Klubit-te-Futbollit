using Microsoft.EntityFrameworkCore;
using FootballClubAPI.Models;

namespace FootballClubAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Stadium> Stadiums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Player>()
                .Property(p => p.FirstName)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(p => p.LastName)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(p => p.Position)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.TokenHash)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Club>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Stadium>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Stadium>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Season>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Season>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Season>()
                .Property(s => s.Competition)
                .HasMaxLength(100);

            modelBuilder.Entity<Season>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Match>()
                .Property(m => m.Status)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Match>()
                .Property(m => m.CompetitionType)
                .HasMaxLength(100);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeClub)
                .WithMany(c => c.HomeMatches)
                .HasForeignKey(m => m.HomeClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayClub)
                .WithMany(c => c.AwayMatches)
                .HasForeignKey(m => m.AwayClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Stadium)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .HasKey(me => me.Id);

            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Match)
                .WithMany(m => m.MatchEvents)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Player)
                .WithMany()
                .HasForeignKey(me => me.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerStats>()
                .HasKey(ps => ps.Id);

            modelBuilder.Entity<PlayerStats>()
                .Property(ps => ps.Rating)
                .HasColumnType("decimal(4,2)");

            modelBuilder.Entity<PlayerStats>()
                .HasOne(ps => ps.Player)
                .WithMany()
                .HasForeignKey(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerStats>()
                .HasOne(ps => ps.Match)
                .WithMany(m => m.PlayerStats)
                .HasForeignKey(ps => ps.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
