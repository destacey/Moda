using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Identity;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", SchemaNames.Identity);

        builder.HasIndex(u => u.Id)
            .IncludeProperties(e => new { e.UserName, e.EmployeeId, e.Email, e.FirstName, e.LastName }); ;

        builder.Property(u => u.ObjectId).HasMaxLength(256);

        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        // Relationships
        builder.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder) =>
        builder
            .ToTable("Roles", SchemaNames.Identity);
}

public class ApplicationRoleClaimConfig : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder) =>
        builder
            .ToTable("RoleClaims", SchemaNames.Identity);
}

public class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder) =>
        builder
            .ToTable("UserRoles", SchemaNames.Identity);
}

public class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder) =>
        builder
            .ToTable("UserClaims", SchemaNames.Identity);
}

public class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder) =>
        builder
            .ToTable("UserLogins", SchemaNames.Identity);
}

public class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder) =>
        builder
            .ToTable("UserTokens", SchemaNames.Identity);
}

public class PersonalAccessTokenConfig : IEntityTypeConfiguration<PersonalAccessToken>
{
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.ToTable("PersonalAccessTokens", SchemaNames.Identity);

        builder.HasKey(p => p.Id);

        // Indexes
        builder.HasIndex(p => p.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_PersonalAccessTokens_TokenHash");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_PersonalAccessTokens_UserId");

        builder.HasIndex(p => p.ExpiresAt)
            .HasDatabaseName("IX_PersonalAccessTokens_ExpiresAt");

        builder.HasIndex(p => new { p.UserId, p.RevokedAt })
            .HasDatabaseName("IX_PersonalAccessTokens_UserId_RevokedAt");

        // Composite index for efficient token lookup
        builder.HasIndex(p => new { p.TokenIdentifier, p.RevokedAt, p.ExpiresAt })
            .HasDatabaseName("IX_PersonalAccessTokens_TokenIdentifier_RevokedAt_ExpiresAt");

        // Properties
        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.TokenIdentifier)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(p => p.TokenHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(p => p.EmployeeId);

        builder.Property(p => p.Scopes)
            .HasMaxLength(4000); // Allows for JSON array of permission names

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.Property(p => p.LastUsedAt);

        builder.Property(p => p.RevokedAt);

        builder.Property(p => p.RevokedBy);

        // Relationships
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}