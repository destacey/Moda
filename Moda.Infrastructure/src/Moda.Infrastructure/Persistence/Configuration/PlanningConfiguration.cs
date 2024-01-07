using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Organization;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class PlanningTeamConfig : IEntityTypeConfiguration<PlanningTeam>
{
    public void Configure(EntityTypeBuilder<PlanningTeam> builder)
    {
        builder.ToTable("PlanningTeams", SchemaNames.Planning);

        builder.HasKey(t => t.Id);
        builder.HasAlternateKey(t => t.Key);

        builder.HasIndex(t => new { t.Id, t.IsDeleted })
            .IncludeProperties(t => new { t.Key, t.Name, t.Code, t.Type, t.IsActive });
        builder.HasIndex(t => new { t.Key, t.IsDeleted })
            .IncludeProperties(t => new { t.Id, t.Name, t.Code, t.Type, t.IsActive });
        builder.HasIndex(t => t.Code)
            .IsUnique()
            .IncludeProperties(t => new { t.Id, t.Key, t.Name, t.Type, t.IsActive });
        builder.HasIndex(t => new { t.IsActive, t.IsDeleted })
            .IncludeProperties(t => new { t.Id, t.Key, t.Name, t.Code, t.Type });
        builder.HasIndex(t => t.IsDeleted)
            .IncludeProperties(t => new { t.Id, t.Key, t.Name, t.Code, t.Type, t.IsActive });

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

        // Audit
        builder.Property(t => t.Deleted);
        builder.Property(t => t.DeletedBy);
        builder.Property(t => t.IsDeleted);

        // Relationships
        builder.HasMany<Risk>()
            .WithOne(t => t.Team)
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore
    }
}

public class PlanningIntervalConfig : IEntityTypeConfiguration<PlanningInterval>
{
    public void Configure(EntityTypeBuilder<PlanningInterval> builder)
    {
        builder.ToTable("PlanningIntervals", SchemaNames.Planning);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.Key);

        builder.HasIndex(p => p.Id)
            .IncludeProperties(p => new { p.Name, p.Description });
        builder.HasIndex(p => p.Name)
            .IsUnique();
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.Key).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1024);

        // Value Objects
        builder.OwnsOne(p => p.DateRange, options =>
        {
            options.HasIndex(i => new { i.Start, i.End });

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
            .IncludeProperties(i => new { i.Key, i.PlanningIntervalId, i.Name, i.Type });
        builder.HasIndex(i => new { i.Key, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.PlanningIntervalId, i.Name, i.Type });
        builder.HasIndex(i => new { i.PlanningIntervalId, i.IsDeleted })
            .IncludeProperties(i => new { i.Id, i.Key, i.Name, i.Type });
        builder.HasIndex(i => i.IsDeleted)
            .IncludeProperties(i => new { i.Id, i.Key, i.PlanningIntervalId, i.Name, i.Type });

        builder.Property(i => i.Key).ValueGeneratedOnAdd();

        builder.Property(i => i.Name).HasMaxLength(128).IsRequired();
        builder.Property(i => i.Type).IsRequired()
            .HasConversion<EnumConverter<IterationType>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);

        // Value Objects
        builder.OwnsOne(p => p.DateRange, options =>
        {
            options.HasIndex(i => new { i.Start, i.End });

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
            .IncludeProperties(o => new { o.Key, o.PlanningIntervalId, o.ObjectiveId, o.Type, o.IsStretch });
        builder.HasIndex(o => new { o.Key, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.PlanningIntervalId, o.ObjectiveId, o.Type, o.IsStretch });
        builder.HasIndex(o => new { o.PlanningIntervalId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.ObjectiveId, o.Type, o.IsStretch });
        builder.HasIndex(o => new { o.ObjectiveId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.PlanningIntervalId, o.Type, o.IsStretch });
        builder.HasIndex(o => o.IsDeleted)
            .IncludeProperties(o => new { o.Id, o.Key, o.PlanningIntervalId, o.ObjectiveId, o.Type, o.IsStretch });

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

public class RiskConfig : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks", SchemaNames.Planning);

        builder.HasKey(r => r.Id);
        builder.HasAlternateKey(r => r.Key);

        builder.HasIndex(r => r.Id);
        builder.HasIndex(r => r.IsDeleted);

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
