using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Common.Models;
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

        builder.Navigation(w => w.BacklogLevels)
            .AutoInclude();
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

public class WorkItemConfig : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("WorkItems", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);

        builder.HasIndex(w => w.Id)
            .IncludeProperties(w => new { w.Key, w.Title, w.WorkspaceId, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId });

        builder.HasIndex(w => w.Key)
            .IncludeProperties(w => new { w.Id, w.Title, w.WorkspaceId, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId });

        builder.HasIndex(w => w.ExternalId)
            .IncludeProperties(w => new { w.Id, w.Key , w.Title, w.WorkspaceId, w.AssignedToId, w.TypeId, w.StatusId });

        // Properties
        builder.Property(w => w.Key).IsRequired()
            .HasConversion(
                w => w.Value,
                w => new WorkItemKey(w))
            .HasColumnType("varchar")
            .HasMaxLength(64);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(128);
        builder.Property(w => w.ExternalId);
        builder.Property(w => w.Priority);
        builder.Property(w => w.StackRank);

        builder.Property(w => w.Created);
        builder.Property(w => w.LastModified);


        // Relationships
        builder.HasOne(w => w.Type)
            .WithMany()
            .HasForeignKey(w => w.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Status)
            .WithMany()
            .HasForeignKey(w => w.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.AssignedTo)
            .WithMany()
            .HasForeignKey(w => w.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.LastModifiedBy)
            .WithMany()
            .HasForeignKey(w => w.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkProcessConfig : IEntityTypeConfiguration<WorkProcess>
{
    public void Configure(EntityTypeBuilder<WorkProcess> builder)
    {
        builder.ToTable("WorkProcesses", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);

        builder.HasIndex(i => new { i.Id, i.IsDeleted })
            .IncludeProperties(i => new { i.Key, i.Name, i.ExternalId, i.Ownership, i.IsActive });

        builder.HasIndex(i => new { i.Key, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.Name, i.ExternalId, i.Ownership, i.IsActive });

        builder.Property(p => p.Key).ValueGeneratedOnAdd();

        // Properties
        builder.Property(w => w.Name).HasMaxLength(128).IsRequired();
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
        builder.Property(w => w.IsDeleted); ;

        // Relationships
        builder.HasMany(w => w.Schemes)
            .WithOne(w => w.WorkProcess)
            .HasForeignKey(w => w.WorkProcessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkProcessSchemeConfig : IEntityTypeConfiguration<WorkProcessScheme>
{
    public void Configure(EntityTypeBuilder<WorkProcessScheme> builder)
    {
        builder.ToTable("WorkProcessSchemes", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);

        // Properties
        builder.Property(w => w.WorkProcessId);
        builder.Property(w => w.WorkTypeId);
        builder.Property(w => w.WorkflowId);
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
        builder.HasOne(w => w.WorkType)
            .WithMany()
            .HasForeignKey(w => w.WorkTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Workflow)
            .WithMany()
            .HasForeignKey(w => w.WorkProcessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkspaceConfig : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);

        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .IncludeProperties(w => new { w.Key, w.Name, w.Ownership, w.IsActive });
        builder.HasIndex(w => new { w.Key, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name, w.Ownership, w.IsActive });
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Key, w.Name, w.Ownership });

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
        builder.HasOne(w => w.WorkProcess)
            .WithMany(w => w.Workspaces)
            .HasForeignKey(w => w.WorkProcessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.WorkItems)
            .WithOne(w => w.Workspace)
            .HasForeignKey(w => w.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkStatusConfig : IEntityTypeConfiguration<WorkStatus>
{
    public void Configure(EntityTypeBuilder<WorkStatus> builder)
    {
        builder.ToTable("WorkStatuses", SchemaNames.Work);

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

public class WorkflowConfig : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name });

        // Properties
        builder.Property(w => w.Name).IsRequired().HasMaxLength(64);
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
        builder.HasMany(w => w.Schemes)
            .WithOne()
            .HasForeignKey(w => w.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Schemes)
            .AutoInclude();
    }
}

public class WorkflowSchemeConfig : IEntityTypeConfiguration<WorkflowScheme>
{
    public void Configure(EntityTypeBuilder<WorkflowScheme> builder)
    {
        builder.ToTable("WorkflowSchemes", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id });

        // Properties
        builder.Property(w => w.WorkflowId);
        builder.Property(w => w.WorkStatusId);
        builder.Property(w => w.WorkStatusCategory).IsRequired()
            .HasConversion<EnumConverter<WorkStatusCategory>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
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
        builder.HasOne(w => w.WorkStatus)
            .WithMany()
            .HasForeignKey(ws => ws.WorkStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(w => w.WorkStatus)
            .AutoInclude();
    }
}