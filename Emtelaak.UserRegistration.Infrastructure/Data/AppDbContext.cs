// Emtelaak.UserRegistration.Infrastructure/Data/AppDbContext.cs
using System;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emtelaak.UserRegistration.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<KycVerification> KycVerifications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Accreditation> Accreditations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure entities and relationships
            ConfigureUserEntity(builder);
            ConfigureKycVerificationEntity(builder);
            ConfigureDocumentEntity(builder);
            ConfigureAccreditationEntity(builder);
            ConfigureRoleEntity(builder);
            ConfigureUserRoleEntity(builder);
            ConfigurePermissionEntity(builder);
            ConfigureRolePermissionEntity(builder);
            ConfigureUserPreferenceEntity(builder);
            ConfigureActivityLogEntity(builder);
            ConfigureUserSessionEntity(builder);
        }

        private void ConfigureUserEntity(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CountryOfResidence).HasMaxLength(100);
                entity.Property(e => e.ReferralCode).HasMaxLength(20);
                entity.Property(e => e.TermsAcceptedVersion).HasMaxLength(20);
            });
        }

        private void ConfigureKycVerificationEntity(ModelBuilder builder)
        {
            builder.Entity<KycVerification>(entity =>
            {
                entity.ToTable("KycVerifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VerificationId).HasMaxLength(100);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);

                // One-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithOne(u => u.KycVerification)
                    .HasForeignKey<KycVerification>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureDocumentEntity(ModelBuilder builder)
        {
            builder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);

                // Many-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Documents)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureAccreditationEntity(ModelBuilder builder)
        {
            builder.Entity<Accreditation>(entity =>
            {
                entity.ToTable("Accreditations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IncomeLevel).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetWorth).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InvestmentLimitAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReviewNotes).HasMaxLength(1000);

                // One-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Accreditation)
                    .HasForeignKey<Accreditation>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureRoleEntity(ModelBuilder builder)
        {
            builder.Entity<Role>(entity =>
            {
                entity.ToTable("AppRoles"); // Using a different name to avoid collision with ASP.NET Identity Roles
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }

        private void ConfigureUserRoleEntity(ModelBuilder builder)
        {
            builder.Entity<UserRole>(entity =>
            {
                entity.ToTable("AppUserRoles"); // Using a different name to avoid collision with ASP.NET Identity UserRoles
                entity.HasKey(e => e.Id);

                // Many-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Many-to-one relationship with Role
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigurePermissionEntity(ModelBuilder builder)
        {
            builder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
            });
        }

        private void ConfigureRolePermissionEntity(ModelBuilder builder)
        {
            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(e => e.Id);

                // Many-to-one relationship with Role
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Many-to-one relationship with Permission
                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureUserPreferenceEntity(ModelBuilder builder)
        {
            builder.Entity<UserPreference>(entity =>
            {
                entity.ToTable("UserPreferences");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Language).HasMaxLength(10);

                // One-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithOne(u => u.UserPreference)
                    .HasForeignKey<UserPreference>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureActivityLogEntity(ModelBuilder builder)
        {
            builder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("ActivityLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Details).HasColumnType("nvarchar(max)");
                entity.Property(e => e.FailureReason).HasMaxLength(500);

                // Many-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.ActivityLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureUserSessionEntity(ModelBuilder builder)
        {
            builder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.DeviceInfo).HasMaxLength(500);
                entity.Property(e => e.RevokedReason).HasMaxLength(500);

                // Many-to-one relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}