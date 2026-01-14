using Microsoft.EntityFrameworkCore;
using SteamLibrary.Data.Entities;
using SteamLibrary.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SteamLibrary.Data
{


    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Access> Accesses => Set<Access>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<UserGame> UserGame => Set<UserGame>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------
            // User
            // ----------------------------------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.AccessId).IsRequired();
            });

            // ----------------------------------------------------
            // Access
            // ----------------------------------------------------
            modelBuilder.Entity<Access>(entity =>
            {
                entity.HasIndex(a => a.Name).IsUnique();

                entity.Property(a => a.Name)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ----------------------------------------------------
            // Publisher
            // ----------------------------------------------------
            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Location).HasMaxLength(200);
                entity.Property(p => p.Email).HasMaxLength(100);
                entity.Property(p => p.Phone).HasMaxLength(20);

                // SQLite-safe default
                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ----------------------------------------------------
            // Game
            // ----------------------------------------------------
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasIndex(g => g.Title);

                entity.Property(g => g.Title)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(g => g.Description).HasMaxLength(500);
                entity.Property(g => g.Genre).HasMaxLength(50);

                entity.Property(g => g.Price).HasPrecision(18, 2);

                entity.Property(g => g.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ----------------------------------------------------
            // UserGame (join table)
            // ----------------------------------------------------
            modelBuilder.Entity<UserGame>(entity =>
            {
                entity.HasKey(ug => new { ug.UserId, ug.GameId });

                entity.HasOne(ug => ug.User)
                      .WithMany(u => u.Games)
                      .HasForeignKey(ug => ug.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Game)
                      .WithMany(g => g.Users)
                      .HasForeignKey(ug => ug.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ug => ug.AddedDate)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ----------------------------------------------------
            // Relationships
            // ----------------------------------------------------
            modelBuilder.Entity<User>()
                .HasOne(u => u.Access)
                .WithMany(a => a.Users)
                .HasForeignKey(u => u.AccessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Publisher)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.AddedByUser)
                .WithMany(u => u.AddedGames)
                .HasForeignKey(g => g.AddedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Publisher>()
                .HasOne(p => p.CreatedByUser)
                .WithMany(u => u.CreatedPublishers)
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
