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
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<SponsorClub> SponsorClubs { get; set; }
        public DbSet<Stadium> Stadiums { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Trophy> Trophies { get; set; }
        public DbSet<Season> Seasons { get; set; }
      
        public DbSet<ClubTrophy> ClubTrophies { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Club> Clubs { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Player Configuration
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

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Club)
                .WithMany(c => c.Players)
                .HasForeignKey(p => p.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // User Configuration
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

            // Club Configuration
            modelBuilder.Entity<Club>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Club>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .Property(c => c.City)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Stadium Configuration
            modelBuilder.Entity<Stadium>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Stadium>()
                .Property(s => s.Name)
                .IsRequired();

            modelBuilder.Entity<Stadium>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sponsor Configuration
            modelBuilder.Entity<Sponsor>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Sponsor>()
                .Property(s => s.Name)
                .IsRequired();

            modelBuilder.Entity<Sponsor>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trophy Configuration
            modelBuilder.Entity<Trophy>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Trophy>()
                .Property(t => t.Name)
                .IsRequired();

            modelBuilder.Entity<Trophy>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Season Configuration
            modelBuilder.Entity<Season>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Season>()
                .Property(s => s.Name)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(s => s.StartDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(s => s.EndDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SponsorClub Configuration (Junction Table)
            modelBuilder.Entity<SponsorClub>()
                .HasKey(sc => new { sc.SponsorId, sc.ClubId });

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sc => sc.Sponsor)
                .WithMany(s => s.SponsorClubs)
                .HasForeignKey(sc => sc.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sc => sc.Club)
                .WithMany(c => c.SponsorClubs)
                .HasForeignKey(sc => sc.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClubTrophy Configuration (Junction Table)
            modelBuilder.Entity<ClubTrophy>()
                .HasKey(ct => new { ct.TrophyId, ct.ClubId });

            modelBuilder.Entity<ClubTrophy>()
                .HasOne(ct => ct.Trophy)
                .WithMany(t => t.ClubTrophies)
                .HasForeignKey(ct => ct.TrophyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubTrophy>()
                .HasOne(ct => ct.Club)
                .WithMany(c => c.ClubTrophies)
                .HasForeignKey(ct => ct.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefreshToken Configuration
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.TokenHash)
                .IsRequired();

            // Club configuration
            modelBuilder.Entity<Club>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Club>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Sponsor configuration
            modelBuilder.Entity<Sponsor>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Sponsor>()
                .Property(s => s.Name)
                .IsRequired();

            modelBuilder.Entity<Sponsor>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // SponsorClub configuration (junction table)
            modelBuilder.Entity<SponsorClub>()
                .HasKey(sc => sc.Id);

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sc => sc.Sponsor)
                .WithMany(s => s.SponsorClubs)
                .HasForeignKey(sc => sc.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sc => sc.Club)
                .WithMany(c => c.SponsorClubs)
                .HasForeignKey(sc => sc.ClubId)
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
