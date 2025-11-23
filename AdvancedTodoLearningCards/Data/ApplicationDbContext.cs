using AdvancedTodoLearningCards.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<CardSchedule> CardSchedules { get; set; }
        public DbSet<ReviewLog> ReviewLogs { get; set; }
        public DbSet<AlgorithmSettings> AlgorithmSettings { get; set; }
        public DbSet<ReviewNotification> ReviewNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Card Configuration
            builder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Cards)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Schedule)
                    .WithOne(s => s.Card)
                    .HasForeignKey<CardSchedule>(s => s.CardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CardSchedule Configuration
            builder.Entity<CardSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CardId).IsUnique();
                entity.HasIndex(e => e.NextReviewAt);
            });

            // ReviewLog Configuration
            builder.Entity<ReviewLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CardId, e.ReviewedAt });
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.Card)
                    .WithMany(c => c.ReviewLogs)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ReviewLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // AlgorithmSettings Configuration
            builder.Entity<AlgorithmSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AlgorithmSettings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ReviewNotification Configuration
            builder.Entity<ReviewNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.IsAcknowledged });
                entity.HasIndex(e => e.CardId);
                entity.HasIndex(e => e.NotifiedAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Card)
                    .WithMany()
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}