using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Work.Domain.Enums;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class BacklogLevelSchemeConfig : IEntityTypeConfiguration<BacklogLevelScheme>
{
    public void Configure(EntityTypeBuilder<BacklogLevelScheme> builder)
    {
        builder.ToTable("BacklogLevelSchemes", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);

        // Relationships
        builder.HasMany(w => w.BacklogLevels)
            .WithOne()
            .HasForeignKey(w => w.ParentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BacklogLevelConfig : IEntityTypeConfiguration<BacklogLevel>
{
    public void Configure(EntityTypeBuilder<BacklogLevel> builder)
    {
        builder.ToTable("BacklogLevels", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);
        builder.HasIndex(w => w.ParentId);

        builder.Property(w => w.Name).IsRequired().HasMaxLength(256);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.Category)
            .HasConversion(
                w => w.ToString(),
                w => (BacklogCategory)Enum.Parse(typeof(BacklogCategory), w))
            .HasMaxLength(128);
        builder.Property(w => w.Rank);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);
    }
}

public class WorkProcessConfig : IEntityTypeConfiguration<WorkProcess>
{
    public void Configure(EntityTypeBuilder<WorkProcess> builder)
    {
        builder.ToTable("WorkProcesses", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);

        builder.HasIndex(w => w.Id);


        // Properties
        builder.Property(w => w.Name).IsRequired().HasMaxLength(64);
        builder.Property(w => w.Description).HasMaxLength(1024);

        builder.Property(w => w.Ownership).IsRequired()
            .HasConversion<EnumConverter<Ownership>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);
    }
}

public class WorkspaceConfig : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);

        builder.HasIndex(w => w.Id)
            .IncludeProperties(w => new { w.Name, w.Ownership, w.IsActive, w.IsDeleted });
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name, w.Ownership });

        // Properties
        builder.Property(w => w.Key).IsRequired()
            .HasConversion(
                w => w.Value,
                w => new WorkspaceKey(w))
            .HasColumnType("varchar")
            .HasMaxLength(20);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(128);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.Ownership).IsRequired()
            .HasConversion<EnumConverter<Ownership>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(w => w.ExternalId);
        builder.Property(w => w.IsActive);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);

        // Relationships
        builder.HasOne<WorkProcess>()
            .WithMany(w => w.Workspaces)
            .HasForeignKey(w => w.WorkProcessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkStateConfig : IEntityTypeConfiguration<WorkState>
{
    public void Configure(EntityTypeBuilder<WorkState> builder)
    {
        builder.ToTable("WorkStates", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name });

        // Properties
        builder.Property(w => w.Name).IsRequired().HasMaxLength(64);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.IsActive);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);
    }
}

public class WorkTypeConfig : IEntityTypeConfiguration<WorkType>
{
    public void Configure(EntityTypeBuilder<WorkType> builder)
    {
        builder.ToTable("WorkTypes", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name });

        // Properties
        builder.Property(w => w.Name).IsRequired().HasMaxLength(64);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.IsActive);

        // Audit
        builder.Property(w => w.Created);
        builder.Property(w => w.CreatedBy);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.LastModifiedBy);
        builder.Property(w => w.Deleted);
        builder.Property(w => w.DeletedBy);
        builder.Property(w => w.IsDeleted);
    }
}