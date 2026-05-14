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
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Injury> Injuries { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<TrainingAttendance> TrainingAttendances { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<SponsorClub> SponsorClubs { get; set; }
        public DbSet<Stadium> Stadiums { get; set; }
        public DbSet<Trophy> Trophies { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<ClubTrophy> ClubTrophies { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasKey(player => player.Id);

            modelBuilder.Entity<Player>()
                .Property(player => player.FirstName)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(player => player.LastName)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .Property(player => player.Position)
                .IsRequired();

            modelBuilder.Entity<Player>()
                .HasOne(player => player.Club)
                .WithMany(club => club.Players)
                .HasForeignKey(player => player.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Player>()
                .HasOne(player => player.User)
                .WithMany()
                .HasForeignKey(player => player.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasKey(user => user.Id);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(user => user.Role)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(user => user.EmailVerificationToken)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .HasMany(user => user.RefreshTokens)
                .WithOne()
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .Property(refreshToken => refreshToken.TokenHash)
                .HasMaxLength(88);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => new { refreshToken.UserId, refreshToken.IsRevoked, refreshToken.ExpiresAt });

            modelBuilder.Entity<Club>()
                .HasKey(club => club.Id);

            modelBuilder.Entity<Club>()
                .Property(club => club.Name)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .Property(club => club.City)
                .IsRequired();

            modelBuilder.Entity<Club>()
                .HasOne(club => club.User)
                .WithMany()
<<<<<<< HEAD
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
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Season>()
                .Property(s => s.StartDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(s => s.EndDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(s => s.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Season>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Season>()
                .Property(s => s.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Season>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Season>()
                .HasIndex(s => s.StartDate);

            modelBuilder.Entity<Season>()
                .HasIndex(s => s.EndDate);

            modelBuilder.Entity<Season>()
                .HasMany(s => s.Matches)
                .WithOne(m => m.Season)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Season>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Seasons_StartBeforeEnd", "[StartDate] < [EndDate]"));

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
=======
                .HasForeignKey(club => club.UserId)
                .OnDelete(DeleteBehavior.NoAction);
>>>>>>> 26e4a8041e6c280df256fe16226347f8870939dc

            modelBuilder.Entity<Club>()
                .HasMany(club => club.Players)
                .WithOne(player => player.Club)
                .HasForeignKey(player => player.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Club>()
                .HasMany(club => club.Stadiums)
                .WithOne(stadium => stadium.Club)
                .HasForeignKey(stadium => stadium.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Club>()
                .HasMany(club => club.SponsorClubs)
                .WithOne(sponsorClub => sponsorClub.Club)
                .HasForeignKey(sponsorClub => sponsorClub.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Club>()
                .HasMany(club => club.ClubTrophies)
                .WithOne(clubTrophy => clubTrophy.Club)
                .HasForeignKey(clubTrophy => clubTrophy.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stadium>()
                .HasKey(stadium => stadium.Id);

            modelBuilder.Entity<Stadium>()
                .Property(stadium => stadium.Name)
                .IsRequired();

            modelBuilder.Entity<Stadium>()
                .HasOne(stadium => stadium.User)
                .WithMany()
                .HasForeignKey(stadium => stadium.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sponsor>()
                .HasKey(sponsor => sponsor.Id);

            modelBuilder.Entity<Sponsor>()
                .Property(sponsor => sponsor.Name)
                .IsRequired();

            modelBuilder.Entity<Sponsor>()
                .HasOne(sponsor => sponsor.User)
                .WithMany()
                .HasForeignKey(sponsor => sponsor.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SponsorClub>()
                .HasKey(sponsorClub => sponsorClub.Id);

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sponsorClub => sponsorClub.Sponsor)
                .WithMany(sponsor => sponsor.SponsorClubs)
                .HasForeignKey(sponsorClub => sponsorClub.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SponsorClub>()
                .HasOne(sponsorClub => sponsorClub.Club)
                .WithMany(club => club.SponsorClubs)
                .HasForeignKey(sponsorClub => sponsorClub.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trophy>()
                .HasKey(trophy => trophy.Id);

<<<<<<< HEAD
=======
            modelBuilder.Entity<Trophy>()
                .Property(trophy => trophy.Name)
                .IsRequired();

            modelBuilder.Entity<Trophy>()
                .HasOne(trophy => trophy.User)
                .WithMany()
                .HasForeignKey(trophy => trophy.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Season>()
                .HasKey(season => season.Id);

            modelBuilder.Entity<Season>()
                .Property(season => season.Name)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(season => season.StartDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(season => season.EndDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .HasOne(season => season.User)
                .WithMany()
                .HasForeignKey(season => season.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Season>()
                .HasMany(season => season.Matches)
                .WithOne(match => match.Season)
                .HasForeignKey(match => match.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubTrophy>()
                .HasKey(clubTrophy => new { clubTrophy.TrophyId, clubTrophy.ClubId });

            modelBuilder.Entity<ClubTrophy>()
                .HasOne(clubTrophy => clubTrophy.Trophy)
                .WithMany(trophy => trophy.ClubTrophies)
                .HasForeignKey(clubTrophy => clubTrophy.TrophyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubTrophy>()
                .HasOne(clubTrophy => clubTrophy.Club)
                .WithMany(club => club.ClubTrophies)
                .HasForeignKey(clubTrophy => clubTrophy.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

>>>>>>> 26e4a8041e6c280df256fe16226347f8870939dc
            modelBuilder.Entity<Match>()
                .HasKey(match => match.Id);

            modelBuilder.Entity<Match>()
                .HasOne(match => match.HomeClub)
                .WithMany(club => club.HomeMatches)
                .HasForeignKey(match => match.HomeClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(match => match.AwayClub)
                .WithMany(club => club.AwayMatches)
                .HasForeignKey(match => match.AwayClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(match => match.Stadium)
                .WithMany(stadium => stadium.Matches)
                .HasForeignKey(match => match.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(match => match.Season)
                .WithMany(season => season.Matches)
                .HasForeignKey(match => match.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .HasKey(matchEvent => matchEvent.Id);

            modelBuilder.Entity<MatchEvent>()
                .HasOne(matchEvent => matchEvent.Match)
                .WithMany(match => match.MatchEvents)
                .HasForeignKey(matchEvent => matchEvent.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .HasOne(matchEvent => matchEvent.Player)
                .WithMany()
                .HasForeignKey(matchEvent => matchEvent.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerStats>()
                .HasKey(playerStats => playerStats.Id);

            modelBuilder.Entity<PlayerStats>()
                .HasOne(playerStats => playerStats.Player)
                .WithMany()
                .HasForeignKey(playerStats => playerStats.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerStats>()
                .HasOne(playerStats => playerStats.Match)
                .WithMany(match => match.PlayerStats)
                .HasForeignKey(playerStats => playerStats.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== TRANSFER RELATIONSHIPS =====
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.Player)
                .WithMany(p => p.Transfers)
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.FromClub)
                .WithMany(c => c.OutgoingTransfers)
                .HasForeignKey(t => t.FromClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.ToClub)
                .WithMany(c => c.IncomingTransfers)
                .HasForeignKey(t => t.ToClubId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CONTRACT RELATIONSHIPS =====
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Player)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Club)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.PlayerId)
                .IsUnique()
                .HasFilter("Status = 1");

            // ===== INJURY RELATIONSHIPS =====
            modelBuilder.Entity<Injury>()
                .HasOne(i => i.Player)
                .WithMany(p => p.Injuries)
                .HasForeignKey(i => i.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== TRAINING SESSION RELATIONSHIPS =====
            modelBuilder.Entity<TrainingSession>()
                .HasOne(ts => ts.Club)
                .WithMany(c => c.TrainingSessions)
                .HasForeignKey(ts => ts.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== TRAINING ATTENDANCE RELATIONSHIPS =====
            modelBuilder.Entity<TrainingAttendance>()
                .HasOne(ta => ta.TrainingSession)
                .WithMany(ts => ts.Attendances)
                .HasForeignKey(ta => ta.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainingAttendance>()
                .HasOne(ta => ta.Player)
                .WithMany(p => p.TrainingAttendances)
                .HasForeignKey(ta => ta.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainingAttendance>()
                .HasIndex(ta => new { ta.TrainingSessionId, ta.PlayerId })
                .IsUnique();
        }
    }
}
