using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Enums.Work;
using Moda.Common.Models;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class WorkTypeHierarchyConfig : IEntityTypeConfiguration<WorkTypeHierarchy>
{
    public void Configure(EntityTypeBuilder<WorkTypeHierarchy> builder)
    {
        builder.ToTable("WorkTypeHierarchies", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);

        // Relationships
        builder.HasMany(builder => builder.Levels)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Levels)
            .AutoInclude();
    }
}

public class WorkTypeLevelConfig : IEntityTypeConfiguration<WorkTypeLevel>
{
    public void Configure(EntityTypeBuilder<WorkTypeLevel> builder)
    {
        builder.ToTable("WorkTypeLevels", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.Id);

        builder.Property(w => w.Name).IsRequired().HasMaxLength(128);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.Tier)
            .HasConversion(
                w => w.ToString(),
                w => (WorkTypeTier)Enum.Parse(typeof(WorkTypeTier), w))
            .HasMaxLength(32);
        builder.Property(w => w.Order);
    }
}

public class WorkItemReferenceConfig : IEntityTypeConfiguration<WorkItemReference>
{
    public void Configure(EntityTypeBuilder<WorkItemReference> builder)
    {
        builder.ToTable("WorkItemReferences", SchemaNames.Work);

        builder.HasKey(w => new { w.WorkItemId, w.ObjectId });

        builder.HasIndex(w => w.WorkItemId)
            .IncludeProperties(w => new { w.ObjectId, w.Context });

        builder.HasIndex(w => new { w.ObjectId, w.Context })
            .IncludeProperties(w => new { w.WorkItemId });

        // Properties
        builder.Property(h => h.Context).IsRequired()
            .HasConversion<EnumConverter<SystemContext>>()
            .HasColumnType("varchar")
            .HasMaxLength(64);

        // Relationships
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
            .IncludeProperties(w => new { w.Key, w.Title, w.WorkspaceId, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId, w.StatusCategory, w.ActivatedTimestamp, w.DoneTimestamp });

        builder.HasIndex(w => w.Key)
            .IncludeProperties(w => new { w.Id, w.Title, w.WorkspaceId, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId, w.StatusCategory, w.ActivatedTimestamp, w.DoneTimestamp });

        builder.HasIndex(w => w.WorkspaceId)
            .IncludeProperties(w => new { w.Id, w.Key, w.Title, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId, w.StatusCategory, w.ActivatedTimestamp, w.DoneTimestamp });

        builder.HasIndex(w => new { w.Key, w.Title })
            .IncludeProperties(w => new { w.Id, w.WorkspaceId, w.ExternalId, w.AssignedToId, w.TypeId, w.StatusId, w.StatusCategory, w.ActivatedTimestamp, w.DoneTimestamp });

        builder.HasIndex(w => w.ExternalId)
            .IncludeProperties(w => new { w.Id, w.Key , w.Title, w.WorkspaceId, w.AssignedToId, w.TypeId, w.StatusId, w.StatusCategory, w.ActivatedTimestamp, w.DoneTimestamp });

        builder.HasIndex(w => w.StatusCategory)
            .IncludeProperties(w => new { w.Id, w.Key, w.Title, w.WorkspaceId, w.AssignedToId, w.TypeId, w.StatusId, w.ActivatedTimestamp, w.DoneTimestamp });

        // Properties
        builder.Property(w => w.Key).IsRequired()
            .HasConversion(
                w => w.Value,
                w => new WorkItemKey(w))
            .HasColumnType("varchar")
            .HasMaxLength(64);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(256);
        builder.Property(w => w.StatusCategory).IsRequired()
            .HasConversion<EnumConverter<WorkStatusCategory>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(w => w.ExternalId);
        builder.Property(w => w.Priority);
        builder.Property(w => w.StackRank);

        builder.Property(w => w.Created);
        builder.Property(w => w.LastModified);
        builder.Property(w => w.ActivatedTimestamp);
        builder.Property(w => w.DoneTimestamp);


        // Relationships
        builder.HasOne(w => w.Type)
            .WithMany()
            .HasForeignKey(w => w.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Status)
            .WithMany()
            .HasForeignKey(w => w.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(w => w.ParentId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(w => w.Team)
            .WithMany()
            .HasForeignKey(w => w.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(w => w.AssignedTo)
            .WithMany()
            .HasForeignKey(w => w.AssignedToId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.LastModifiedBy)
            .WithMany()
            .HasForeignKey(w => w.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.OutboundLinks)
            .WithOne(ol => ol.Source)
            .HasForeignKey(ol => ol.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.InboundLinks)
            .WithOne(ol => ol.Target)
            .HasForeignKey(ol => ol.TargetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.ReferenceLinks)
            .WithOne()
            .HasForeignKey(w => w.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ExtendedProps)
            .WithOne()
            .HasForeignKey<WorkItemExtended>(w => w.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkItemExtendedConfig : IEntityTypeConfiguration<WorkItemExtended>
{
    public void Configure(EntityTypeBuilder<WorkItemExtended> builder)
    {
        builder.ToTable("WorkItemsExtended", SchemaNames.Work);

        builder.HasIndex(w => new { w.Id, w.ExternalTeamIdentifier });

        // Properties
        builder.Property(w => w.ExternalTeamIdentifier).HasMaxLength(128);
    }
}

public class WorkItemLinkConfig : IEntityTypeConfiguration<WorkItemLink>
{
    public void Configure(EntityTypeBuilder<WorkItemLink> builder)
    {
        builder.ToTable("WorkItemLinks", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => new { w.SourceId, w.LinkType })
            .IncludeProperties(w => new { w.Id, w.TargetId });

        builder.HasIndex(w => new { w.TargetId, w.LinkType })
            .IncludeProperties(w => new { w.Id, w.SourceId });

        // Properties
        builder.Property(w => w.LinkType).IsRequired()
            .HasConversion<EnumConverter<WorkItemLinkType>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);

        // Relationships
        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.EndedBy)
            .WithMany()
            .HasForeignKey(w => w.EndedById)
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
            .IncludeProperties(i => new { i.Key, i.Name, i.ExternalId, i.Ownership, i.IsActive })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(i => new { i.Key, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.Name, i.ExternalId, i.Ownership, i.IsActive })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(w => w.Key).ValueGeneratedOnAdd();

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

        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => new { w.WorkProcessId, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.WorkTypeId, w.WorkflowId, w.IsActive })
            .HasFilter("[IsDeleted] = 0");

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
            .HasForeignKey(w => w.WorkflowId)
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
            .IncludeProperties(w => new { w.Key, w.Name, w.Ownership, w.IsActive })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => new { w.Key, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name, w.Ownership, w.IsActive })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Key, w.Name, w.Ownership })
            .HasFilter("[IsDeleted] = 0");

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
        builder.Property(w => w.ExternalViewWorkItemUrlTemplate).HasMaxLength(256);
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

        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Name })
            .HasFilter("[IsDeleted] = 0");

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
public class WorkTeamConfig : IEntityTypeConfiguration<WorkTeam>
{
    public void Configure(EntityTypeBuilder<WorkTeam> builder)
    {
        builder.ToTable("WorkTeams", SchemaNames.Work);

        builder.HasKey(t => t.Id);
        builder.HasAlternateKey(t => t.Key);

        builder.HasIndex(t => t.Id)
            .IncludeProperties(t => new { t.Key, t.Name, t.Code, t.Type, t.IsActive });
        builder.HasIndex(t => t.Key)
            .IncludeProperties(t => new { t.Id, t.Name, t.Code, t.Type, t.IsActive });
        builder.HasIndex(t => t.Code)
            .IsUnique()
            .IncludeProperties(t => new { t.Id, t.Key, t.Name, t.Type, t.IsActive });
        builder.HasIndex(t => t.IsActive)
            .IncludeProperties(t => new { t.Id, t.Key, t.Name, t.Code, t.Type });

        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Key).ValueGeneratedNever();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(128);
        builder.Property(t => t.Code).IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(10);
        builder.Property(t => t.Type).IsRequired()
            .HasConversion<EnumConverter<TeamType>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(t => t.IsActive);

        // Relationships


        // Ignore
    }
}

public class WorkTypeConfig : IEntityTypeConfiguration<WorkType>
{
    public void Configure(EntityTypeBuilder<WorkType> builder)
    {
        builder.ToTable("WorkTypes", SchemaNames.Work);

        builder.HasKey(w => w.Id);

        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .IncludeProperties(w => new { w.Name, w.LevelId, w.IsActive })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => w.Name).IsUnique();
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.LevelId, w.Name })
            .HasFilter("[IsDeleted] = 0");

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

        // Relationships
        builder.HasOne(w => w.Level)
            .WithMany()
            .HasForeignKey(w => w.LevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkflowConfig : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows", SchemaNames.Work);

        builder.HasKey(w => w.Id);
        builder.HasAlternateKey(w => w.Key);


        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => new { w.Key, w.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => new { w.IsActive, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.Key, w.Name })
            .HasFilter("[IsDeleted] = 0");

        // Properties
        builder.Property(p => p.Key).ValueGeneratedOnAdd();
        builder.Property(w => w.Name).IsRequired().HasMaxLength(128);
        builder.Property(w => w.Description).HasMaxLength(1024);
        builder.Property(w => w.Ownership).IsRequired()
            .HasConversion<EnumConverter<Ownership>>()
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
        builder.HasMany(w => w.Schemes)
            .WithOne(s => s.Workflow)
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

        builder.HasIndex(w => new { w.Id, w.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(w => new { w.WorkflowId, w.IsDeleted })
            .IncludeProperties(w => new { w.Id, w.WorkStatusId, w.WorkStatusCategory })
            .HasFilter("[IsDeleted] = 0");

        // Properties
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