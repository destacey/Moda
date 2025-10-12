﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Organization;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Infrastructure.Persistence.Configuration;

public class PlanningTeamConfig : IEntityTypeConfiguration<PlanningTeam>
{
    public void Configure(EntityTypeBuilder<PlanningTeam> builder)
    {
        builder.ToTable("PlanningTeams", SchemaNames.Planning);

        builder.HasKey(t => t.Id);
        builder.HasAlternateKey(t => t.Key);

        builder.HasIndex(t => t.Id )
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
        builder.HasMany<Risk>()
            .WithOne(t => t.Team)
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore
    }
}

#region Planning Intervals

public class PlanningIntervalConfig : IEntityTypeConfiguration<PlanningInterval>
{
    public void Configure(EntityTypeBuilder<PlanningInterval> builder)
    {
        builder.ToTable("PlanningIntervals", SchemaNames.Planning);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.Key);

        builder.HasIndex(p => new { p.Id, p.IsDeleted })
            .IncludeProperties(p => new { p.Name, p.Description })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Key).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2048);

        // Value Objects
        builder.ComplexProperty(p => p.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start").IsRequired();
            options.Property(d => d.End).HasColumnName("End").IsRequired();
        });

        // Audit
        builder.Property(p => p.Created);
        builder.Property(p => p.CreatedBy);
        builder.Property(p => p.LastModified);
        builder.Property(p => p.LastModifiedBy);
        builder.Property(p => p.Deleted);
        builder.Property(p => p.DeletedBy);
        builder.Property(p => p.IsDeleted);
    }
}

public class PlanningIntervalIterationConfig : IEntityTypeConfiguration<PlanningIntervalIteration>
{
    public void Configure(EntityTypeBuilder<PlanningIntervalIteration> builder)
    {
        builder.ToTable("PlanningIntervalIterations", SchemaNames.Planning);

        builder.HasKey(i => i.Id);
        builder.HasAlternateKey(i => i.Key);

        builder.HasIndex(i => new { i.Id, i.IsDeleted })
            .IncludeProperties(i => new { i.Key, i.PlanningIntervalId, i.Name, i.Category })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(i => new { i.Key, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.PlanningIntervalId, i.Name, i.Category })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(i => new { i.PlanningIntervalId, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.Key, i.Name, i.Category })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(i => i.Key).ValueGeneratedOnAdd();

        builder.Property(i => i.Name).HasMaxLength(128).IsRequired();
        builder.Property(i => i.Category).IsRequired()
            .HasConversion<EnumConverter<IterationCategory>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);

        // Value Objects
        builder.ComplexProperty(p => p.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start").IsRequired();
            options.Property(d => d.End).HasColumnName("End").IsRequired();
        });

        // Audit
        builder.Property(i => i.Created);
        builder.Property(i => i.CreatedBy);
        builder.Property(i => i.LastModified);
        builder.Property(i => i.LastModifiedBy);
        builder.Property(i => i.Deleted);
        builder.Property(i => i.DeletedBy);
        builder.Property(i => i.IsDeleted);

        // Relationships
        builder.HasOne<PlanningInterval>()
            .WithMany(p => p.Iterations)
            .HasForeignKey(i => i.PlanningIntervalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlanningIntervalObjectiveConfig : IEntityTypeConfiguration<PlanningIntervalObjective>
{
    public void Configure(EntityTypeBuilder<PlanningIntervalObjective> builder)
    {
        builder.ToTable("PlanningIntervalObjectives", SchemaNames.Planning);

        builder.HasKey(o => o.Id);
        builder.HasAlternateKey(o => o.Key);

        builder.HasIndex(o => new { o.Id, o.IsDeleted })
            .IncludeProperties(o => new { o.Key, o.PlanningIntervalId, o.ObjectiveId, o.Type, o.IsStretch })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.Key, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.PlanningIntervalId, o.ObjectiveId, o.Type, o.IsStretch })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.PlanningIntervalId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.ObjectiveId, o.Type, o.IsStretch })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.ObjectiveId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.PlanningIntervalId, o.Type, o.IsStretch })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(o => o.Key).ValueGeneratedOnAdd();

        builder.Property(o => o.ObjectiveId).IsRequired();
        builder.Property(o => o.Type).IsRequired()
            .HasConversion<EnumConverter<PlanningIntervalObjectiveType>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");
        builder.Property(o => o.Status).IsRequired()
            .HasConversion<EnumConverter<ObjectiveStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");
        builder.Property(o => o.IsStretch).IsRequired();

        // Audit
        builder.Property(o => o.Created);
        builder.Property(o => o.CreatedBy);
        builder.Property(o => o.LastModified);
        builder.Property(o => o.LastModifiedBy);
        builder.Property(o => o.Deleted);
        builder.Property(o => o.DeletedBy);
        builder.Property(o => o.IsDeleted);

        // Relationships
        builder.HasOne<PlanningInterval>()
            .WithMany(p => p.Objectives)
            .HasForeignKey(o => o.PlanningIntervalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Team)
            .WithMany()
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.HealthCheck)
            .WithOne()
            .HasForeignKey<SimpleHealthCheck>(h => h.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore
    }
}

public class PlanningIntervalTeamConfig : IEntityTypeConfiguration<PlanningIntervalTeam>
{
    public void Configure(EntityTypeBuilder<PlanningIntervalTeam> builder)
    {
        builder.ToTable("PlanningIntervalTeams", SchemaNames.Planning);

        builder.HasKey(p => new { p.PlanningIntervalId, p.TeamId });

        builder.HasIndex(p => p.PlanningIntervalId)
            .IncludeProperties(p => p.TeamId);

        builder.Property(p => p.PlanningIntervalId).IsRequired();
        builder.Property(p => p.TeamId).IsRequired();

        builder.HasOne<PlanningInterval>()
            .WithMany(p => p.Teams)
            .HasForeignKey(p => p.PlanningIntervalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Team)
            .WithMany(p => p.PlanningIntervalTeams)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#endregion Planning Intervals

public class RiskConfig : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks", SchemaNames.Planning);

        builder.HasKey(r => r.Id);
        builder.HasAlternateKey(r => r.Key);

        builder.HasIndex(r => new { r.Id, r.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.Key, r.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new {r.AssigneeId, r.IsDeleted})
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TeamId, r.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(r => r.Key).ValueGeneratedOnAdd();

        builder.Property(r => r.Summary).HasMaxLength(256).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(1024);
        builder.Property(r => r.ReportedOn).IsRequired();

        builder.Property(r => r.Status).IsRequired()
            .HasConversion<EnumConverter<RiskStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(r => r.Category).IsRequired()
            .HasConversion<EnumConverter<RiskCategory>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(r => r.Impact).IsRequired()
            .HasConversion<EnumConverter<RiskGrade>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(r => r.Likelihood).IsRequired()
            .HasConversion<EnumConverter<RiskGrade>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(r => r.FollowUpDate);
        builder.Property(r => r.Response).HasMaxLength(1024);
        builder.Property(r => r.ClosedDate);

        // Audit
        builder.Property(r => r.Created);
        builder.Property(r => r.CreatedBy);
        builder.Property(r => r.LastModified);
        builder.Property(r => r.LastModifiedBy);
        builder.Property(r => r.Deleted);
        builder.Property(r => r.DeletedBy);
        builder.Property(r => r.IsDeleted);

        // Relationships
        builder.HasOne(p => p.ReportedBy)
            .WithMany()
            .HasForeignKey(p => p.ReportedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Assignee)
            .WithMany()
            .HasForeignKey(p => p.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

#region Roadmaps

public class RoadmapConfig : IEntityTypeConfiguration<Roadmap>
{
    public void Configure(EntityTypeBuilder<Roadmap> builder)
    {
        builder.ToTable("Roadmaps", SchemaNames.Planning);

        builder.HasKey(r => r.Id);
        builder.HasAlternateKey(r => r.Key);

        builder.HasIndex(r => new { r.Id, r.Visibility })
            .IncludeProperties(r => new { r.Key, r.Name });

        builder.HasIndex(r => new { r.Key, r.Visibility })
            .IncludeProperties(r => new { r.Id, r.Name });

        builder.HasIndex(r => r.Visibility)
            .IncludeProperties(r => new { r.Id, r.Key, r.Name });

        builder.Property(r => r.Key).ValueGeneratedOnAdd();

        builder.Property(r => r.Name).HasMaxLength(128).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(2048);

        builder.Property(r => r.Visibility).IsRequired()
            .HasConversion<EnumConverter<Visibility>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Value Objects
        builder.ComplexProperty(r => r.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start").IsRequired();
            options.Property(d => d.End).HasColumnName("End").IsRequired();
        });

        // Audit
        builder.Property(r => r.Created);
        builder.Property(r => r.CreatedBy);
        builder.Property(r => r.LastModified);
        builder.Property(r => r.LastModifiedBy);

        // Relationships
        builder.HasMany(r => r.RoadmapManagers)
            .WithOne()
            .HasForeignKey(rm => rm.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RoadmapManagerConfiguration : IEntityTypeConfiguration<RoadmapManager>
{
    public void Configure(EntityTypeBuilder<RoadmapManager> builder)
    {
        builder.ToTable("RoadmapManagers", SchemaNames.Planning);

        builder.HasKey(rm => new { rm.RoadmapId, rm.ManagerId });

        builder.HasIndex(rm => rm.RoadmapId)
            .IncludeProperties(rm => new { rm.ManagerId });

        builder.HasIndex(rm => new { rm.RoadmapId, rm.ManagerId });

        builder.Property(rm => rm.RoadmapId)
            .IsRequired();

        builder.Property(rm => rm.ManagerId)
            .IsRequired();

        builder.HasOne(rm => rm.Manager)
            .WithMany()
            .HasForeignKey(rm => rm.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BaseRoadmapItemConfiguration : IEntityTypeConfiguration<BaseRoadmapItem>
{
    public void Configure(EntityTypeBuilder<BaseRoadmapItem> builder)
    {
        builder.ToTable("RoadmapItems", SchemaNames.Planning);

        builder.HasKey(ri => ri.Id);

        builder.HasDiscriminator(ri => ri.Type)
            .HasValue<RoadmapActivity>(RoadmapItemType.Activity)
            .HasValue<RoadmapMilestone>(RoadmapItemType.Milestone)
            .HasValue<RoadmapTimebox>(RoadmapItemType.Timebox);

        builder.HasIndex(ri => ri.RoadmapId);

        // Properties
        builder.Property(ri => ri.Name).HasMaxLength(128).IsRequired();
        builder.Property(ri => ri.Description).HasMaxLength(2048);

        builder.Property(ri => ri.Type);

        builder.Property(ri => ri.Color)
            .HasMaxLength(7)
            .HasColumnType("varchar");
    }
}

public class RoadmapActivityConfiguration : IEntityTypeConfiguration<RoadmapActivity>
{
    public void Configure(EntityTypeBuilder<RoadmapActivity> builder)
    {
        // Properties
        builder.Property(a => a.Order).IsRequired();

        // Value Objects
        builder.ComplexProperty(a => a.DateRange, options =>
        {
            options.Property(a => a.Start)
                .HasColumnName("Start")
                .IsRequired();
            options.Property(a => a.End)
                .HasColumnName("End")
                .IsRequired();
        });

        // Relationships
        builder.HasMany(a => a.Children)
            .WithOne(ri => ri.Parent)
            .HasForeignKey(a => a.ParentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class RoadmapTimeboxConfiguration : IEntityTypeConfiguration<RoadmapTimebox>
{
    public void Configure(EntityTypeBuilder<RoadmapTimebox> builder)
    {
        // Value Objects
        builder.ComplexProperty(a => a.DateRange, options =>
        {
            options.Property(a => a.Start)
                .HasColumnName("Start")
                .IsRequired();
            options.Property(a => a.End)
                .HasColumnName("End")
                .IsRequired();
        });
    }
}

public class RoadmapMilestoneConfiguration : IEntityTypeConfiguration<RoadmapMilestone>
{
    public void Configure(EntityTypeBuilder<RoadmapMilestone> builder)
    {
        // Properties
        builder.Property(e => e.Date)
            .HasColumnName("Start")
            .IsRequired();
    }
}

#endregion Roadmaps


public class SimpleHealthCheckConfig : IEntityTypeConfiguration<SimpleHealthCheck>
{
    public void Configure(EntityTypeBuilder<SimpleHealthCheck> builder)
    {
        builder.ToTable("PlanningHealthChecks", SchemaNames.Planning);

        builder.HasKey(h => h.ObjectId);

        builder.HasIndex(r => r.ObjectId);

        builder.Property(h => h.ObjectId).IsRequired();
        builder.Property(h => h.Id).IsRequired();
        builder.Property(h => h.Status).IsRequired()
            .HasConversion<EnumConverter<HealthStatus>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(h => h.ReportedOn).IsRequired();
        builder.Property(h => h.Expiration).IsRequired();
    }
}
