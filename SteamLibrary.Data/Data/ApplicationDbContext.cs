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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.AccessId)
                      .IsRequired();
            });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Access)
                .WithMany(a => a.Users)
                .HasForeignKey(u => u.AccessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Access>()
                .HasIndex(a => a.Name)
                .IsUnique();
        }
    }
}
