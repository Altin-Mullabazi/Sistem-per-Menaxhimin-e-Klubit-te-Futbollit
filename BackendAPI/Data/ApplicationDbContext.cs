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

// ✅ CRITICAL: Only 1 Active contract per player
// Filter ensures unique constraint only applies to Status = 1 (Active)
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

// ✅ CRITICAL: Only 1 attendance record per player per session
// Prevents duplicate attendance entries
modelBuilder.Entity<TrainingAttendance>()
    .HasIndex(ta => new { ta.TrainingSessionId, ta.PlayerId })
    .IsUnique();
        }
    }
}
