using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public const string AdminRoleId = "3d8a2fec-a50f-4d6d-bb1c-b2caf3de9a91";
        public const string ManagerRoleId = "716b8f4c-443c-4858-9d67-b049f6b0a16f";
        public const string FanRoleId = "f7a2609f-1ad6-46a8-a73d-8fbc7ed8f8c8";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<User> LegacyUsers { get; set; }
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

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = AdminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = AdminRoleId
                },
                new IdentityRole
                {
                    Id = ManagerRoleId,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = ManagerRoleId
                },
                new IdentityRole
                {
                    Id = FanRoleId,
                    Name = "Fan",
                    NormalizedName = "FAN",
                    ConcurrencyStamp = FanRoleId
                });

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.IsActive)
                .HasDefaultValue(true);

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

            modelBuilder.Entity<Player>()
                .HasOne(player => player.CreatedByUser)
                .WithMany(user => user.Players)
                .HasForeignKey(player => player.CreatedById)
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

            modelBuilder.Entity<RefreshToken>()
                .Property(refreshToken => refreshToken.TokenHash)
                .HasMaxLength(88);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => new { refreshToken.UserId, refreshToken.IsRevoked, refreshToken.ExpiresAt });

            modelBuilder.Entity<RefreshToken>()
                .HasOne<ApplicationUser>()
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

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
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

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
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Season>()
                .Property(season => season.StartDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(season => season.EndDate)
                .IsRequired();

            modelBuilder.Entity<Season>()
                .Property(season => season.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Season>()
                .Property(season => season.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Season>()
                .Property(season => season.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Season>()
                .HasIndex(season => season.Name)
                .IsUnique();

            modelBuilder.Entity<Season>()
                .HasIndex(season => season.StartDate);

            modelBuilder.Entity<Season>()
                .HasIndex(season => season.EndDate);

            modelBuilder.Entity<Season>()
                .HasMany(season => season.Matches)
                .WithOne(match => match.Season)
                .HasForeignKey(match => match.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Season>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Seasons_StartBeforeEnd", "[StartDate] < [EndDate]"));

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

            modelBuilder.Entity<Match>()
                .HasOne(match => match.CreatedByUser)
                .WithMany(user => user.Matches)
                .HasForeignKey(match => match.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<Transfer>()
                .HasOne(transfer => transfer.Player)
                .WithMany(player => player.Transfers)
                .HasForeignKey(transfer => transfer.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(transfer => transfer.FromClub)
                .WithMany(club => club.OutgoingTransfers)
                .HasForeignKey(transfer => transfer.FromClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(transfer => transfer.ToClub)
                .WithMany(club => club.IncomingTransfers)
                .HasForeignKey(transfer => transfer.ToClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(contract => contract.Player)
                .WithMany(player => player.Contracts)
                .HasForeignKey(contract => contract.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(contract => contract.Club)
                .WithMany(club => club.Contracts)
                .HasForeignKey(contract => contract.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(contract => contract.CreatedByUser)
                .WithMany(user => user.Contracts)
                .HasForeignKey(contract => contract.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Contract>()
                .HasIndex(contract => contract.PlayerId)
                .IsUnique()
                .HasFilter("Status = 1");

            modelBuilder.Entity<Injury>()
                .HasOne(injury => injury.Player)
                .WithMany(player => player.Injuries)
                .HasForeignKey(injury => injury.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainingSession>()
                .HasOne(trainingSession => trainingSession.Club)
                .WithMany(club => club.TrainingSessions)
                .HasForeignKey(trainingSession => trainingSession.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainingAttendance>()
                .HasOne(trainingAttendance => trainingAttendance.TrainingSession)
                .WithMany(trainingSession => trainingSession.Attendances)
                .HasForeignKey(trainingAttendance => trainingAttendance.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainingAttendance>()
                .HasOne(trainingAttendance => trainingAttendance.Player)
                .WithMany(player => player.TrainingAttendances)
                .HasForeignKey(trainingAttendance => trainingAttendance.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainingAttendance>()
                .HasIndex(trainingAttendance => new { trainingAttendance.TrainingSessionId, trainingAttendance.PlayerId })
                .IsUnique();
        }
    }
}
