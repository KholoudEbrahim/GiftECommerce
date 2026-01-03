using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace IdentityService.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Gender)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("User");

                entity.Property(e => e.EmailVerified)
                    .IsRequired()
                   .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();

                entity.Property(e => e.IsRevoked)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetRequest configuration
            modelBuilder.Entity<PasswordResetRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.ResetCode).IsUnique();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ResetCode)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.IsUsed)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });
        }
    }
}

