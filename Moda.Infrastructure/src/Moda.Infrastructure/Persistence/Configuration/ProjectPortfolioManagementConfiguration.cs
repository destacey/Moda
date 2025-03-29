using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ExpenditureCategoryConfig : IEntityTypeConfiguration<ExpenditureCategory>
{
    public void Configure(EntityTypeBuilder<ExpenditureCategory> builder)
    {
        builder.ToTable("ExpenditureCategories", SchemaNames.ProjectPortfolioManagement);
        builder.HasKey(e => e.Id);


        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024).IsRequired();

        builder.Property(e => e.State).IsRequired()
            .HasConversion<EnumConverter<ExpenditureCategoryState>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(e => e.IsCapitalizable).IsRequired();
        builder.Property(e => e.RequiresDepreciation).IsRequired();
        builder.Property(e => e.AccountingCode).HasMaxLength(64);
    }
}

public class PpmStrategicThemeConfig : IEntityTypeConfiguration<StrategicTheme>
{
    public void Configure(EntityTypeBuilder<StrategicTheme> builder)
    {
        builder.ToTable("StrategicThemes", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(s => s.Id);
        builder.HasAlternateKey(s => s.Key);

        builder.HasIndex(s => s.State);

        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Key).ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1024).IsRequired();

        builder.Property(s => s.State).IsRequired()
            .HasConversion<EnumConverter<StrategicThemeState>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");
    }
}

public class ProjectPortfolioConfiguration : IEntityTypeConfiguration<ProjectPortfolio>
{
    public void Configure(EntityTypeBuilder<ProjectPortfolio> builder)
    {
        builder.ToTable("Portfolios", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.Key);

        builder.HasIndex(p => p.Status);

        builder.Property(p => p.Key).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1024).IsRequired();
        builder.Property(p => p.Status).IsRequired()
            .HasConversion<EnumConverter<ProjectPortfolioStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Value Objects
        builder.OwnsOne(r => r.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start");
            options.Property(d => d.End).HasColumnName("End");
        });

        // Relationships
        builder.HasMany(p => p.Projects)
            .WithOne(prj => prj.Portfolio)
            .HasForeignKey(p => p.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Programs)
            .WithOne(prg => prg.Portfolio)
            .HasForeignKey(p => p.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StrategicInitiatives)
            .WithOne(i => i.Portfolio)
            .HasForeignKey(i => i.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Roles)
            .WithOne()
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.ToTable("Programs", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.Key);

        builder.HasIndex(p => p.Status);

        builder.Property(p => p.Key).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2048).IsRequired();
        builder.Property(p => p.Status).IsRequired()
            .HasConversion<EnumConverter<ProgramStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Value Objects
        builder.OwnsOne(r => r.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start");
            options.Property(d => d.End).HasColumnName("End");
        });

        // Relationships
        builder.HasMany(p => p.Projects)
            .WithOne(prj => prj.Program)
            .HasForeignKey(p => p.ProgramId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Roles)
            .WithOne()
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StrategicThemeTags)
            .WithOne(t => t.Object)
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.Key);

        builder.HasIndex(p => p.Status);

        builder.Property(p => p.Key).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2048).IsRequired();
        builder.Property(p => p.Status).IsRequired()
            .HasConversion<EnumConverter<ProjectStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Value Objects
        builder.OwnsOne(r => r.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start");
            options.Property(d => d.End).HasColumnName("End");
        });

        // Relationships
        builder.HasOne(p => p.ExpenditureCategory)
            .WithMany()
            .HasForeignKey(p => p.ExpenditureCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Roles)
            .WithOne()
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StrategicThemeTags)
            .WithOne(t => t.Object)
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StrategicInitiativeProjects)
            .WithOne(i => i.Project)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategicInitiativeConfiguration : IEntityTypeConfiguration<StrategicInitiative>
{
    public void Configure(EntityTypeBuilder<StrategicInitiative> builder)
    {
        builder.ToTable("StrategicInitiatives", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(i => i.Id);
        builder.HasAlternateKey(i => i.Key);

        builder.HasIndex(i => i.Status);

        builder.Property(i => i.Key).ValueGeneratedOnAdd();
        builder.Property(i => i.Name).HasMaxLength(128).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(2048).IsRequired();
        builder.Property(i => i.Status).IsRequired()
            .HasConversion<EnumConverter<StrategicInitiativeStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Value Objects
        builder.OwnsOne(r => r.DateRange, options =>
        {
            options.Property(d => d.Start).HasColumnName("Start");
            options.Property(d => d.End).HasColumnName("End");
        });

        // Relationships
        builder.HasMany(i => i.Roles)
            .WithOne()
            .HasForeignKey(r => r.ObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Kpis)
            .WithOne()
            .HasForeignKey(i => i.StrategicInitiativeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategicInitiativeKpiConfiguration : IEntityTypeConfiguration<StrategicInitiativeKpi>
{
    public void Configure(EntityTypeBuilder<StrategicInitiativeKpi> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.ToTable("StrategicInitiativeKpis", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(k => k.Id);
        builder.HasAlternateKey(k => k.Key);

        builder.HasIndex(k => k.StrategicInitiativeId);

        builder.Property(k => k.Key).ValueGeneratedOnAdd();
        builder.Property(k => k.Name).HasMaxLength(64).IsRequired();
        builder.Property(k => k.Description).HasMaxLength(512).IsRequired();
        builder.Property(k => k.TargetValue).IsRequired();
        builder.Property(k => k.ActualValue);

        builder.Property(k => k.Unit).IsRequired()
            .HasConversion<EnumConverter<KpiUnit>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(k => k.TargetDirection).IsRequired()
            .HasConversion<EnumConverter<KpiTargetDirection>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Relationships
        builder.HasMany(k => k.Checkpoints)
            .WithOne()
            .HasForeignKey(k => k.KpiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(k => k.Measurements)
            .WithOne()
            .HasForeignKey(k => k.KpiId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StrategicInitiativeKpiCheckpointConfiguration : IEntityTypeConfiguration<StrategicInitiativeKpiCheckpoint>
{
    public void Configure(EntityTypeBuilder<StrategicInitiativeKpiCheckpoint> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.ToTable("StrategicInitiativeKpiCheckpoints", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(k => k.Id);

        builder.HasIndex(k => k.KpiId);

        builder.Property(k => k.TargetValue).IsRequired();
        builder.Property(k => k.CheckpointDate).IsRequired();
    }
}

public class StrategicInitiativeKpiMeasurementConfiguration : IEntityTypeConfiguration<StrategicInitiativeKpiMeasurement>
{
    public void Configure(EntityTypeBuilder<StrategicInitiativeKpiMeasurement> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.ToTable("StrategicInitiativeKpiMeasurements", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.KpiId);

        builder.Property(m => m.ActualValue).IsRequired();
        builder.Property(m => m.MeasurementDate).IsRequired();
        builder.Property(m => m.Note).HasMaxLength(1024);

        // Relationships
        builder.HasOne(p => p.MeasuredBy)
            .WithMany()
            .HasForeignKey(p => p.MeasuredById)
            .OnDelete(DeleteBehavior.NoAction);
    }
}


public class StrategicInitiativeProjectConfiguration : IEntityTypeConfiguration<StrategicInitiativeProject>
{
    public void Configure(EntityTypeBuilder<StrategicInitiativeProject> builder)
    {
        builder.ToTable("StrategicInitiativeProjects", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(i => new { i.StrategicInitiativeId, i.ProjectId });

        builder.HasIndex(i => i.StrategicInitiativeId);
        builder.HasIndex(i => i.ProjectId);

        // Relationships
        builder.HasOne(i => i.StrategicInitiative)
            .WithMany(i => i.StrategicInitiativeProjects)
            .HasForeignKey(i => i.StrategicInitiativeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(i => i.Project)
            .WithMany(i => i.StrategicInitiativeProjects)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#region Role Assignments

public class ProjectPortfolioRoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment<ProjectPortfolioRole>>
{
    public void Configure(EntityTypeBuilder<RoleAssignment<ProjectPortfolioRole>> builder)
    {
        builder.ToTable("PortfolioRoleAssignments", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(r => new { r.ObjectId, r.EmployeeId, r.Role });

        builder.HasIndex(r => r.ObjectId);
        builder.HasIndex(r => r.EmployeeId);

        builder.Property(p => p.Role).IsRequired()
            .HasConversion<EnumConverter<ProjectPortfolioRole>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Relationships
        builder.HasOne(rm => rm.Employee)
            .WithMany()
            .HasForeignKey(rm => rm.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProgramRoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment<ProgramRole>>
{
    public void Configure(EntityTypeBuilder<RoleAssignment<ProgramRole>> builder)
    {
        builder.ToTable("ProgramRoleAssignments", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(r => new { r.ObjectId, r.EmployeeId, r.Role });

        builder.HasIndex(r => r.ObjectId);
        builder.HasIndex(r => r.EmployeeId);

        builder.Property(p => p.Role).IsRequired()
            .HasConversion<EnumConverter<ProgramRole>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Relationships
        builder.HasOne(rm => rm.Employee)
            .WithMany()
            .HasForeignKey(rm => rm.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProjectRoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment<ProjectRole>>
{
    public void Configure(EntityTypeBuilder<RoleAssignment<ProjectRole>> builder)
    {
        builder.ToTable("ProjectRoleAssignments", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(r => new { r.ObjectId, r.EmployeeId, r.Role });

        builder.HasIndex(r => r.ObjectId);
        builder.HasIndex(r => r.EmployeeId);

        builder.Property(p => p.Role).IsRequired()
            .HasConversion<EnumConverter<ProjectRole>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Relationships
        builder.HasOne(rm => rm.Employee)
            .WithMany()
            .HasForeignKey(rm => rm.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class StrategicInitiativeRoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment<StrategicInitiativeRole>>
{
    public void Configure(EntityTypeBuilder<RoleAssignment<StrategicInitiativeRole>> builder)
    {
        builder.ToTable("StrategicInitiativeRoleAssignments", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(r => new { r.ObjectId, r.EmployeeId, r.Role });

        builder.HasIndex(r => r.ObjectId);
        builder.HasIndex(r => r.EmployeeId);

        builder.Property(p => p.Role).IsRequired()
            .HasConversion<EnumConverter<StrategicInitiativeRole>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        // Relationships
        builder.HasOne(rm => rm.Employee)
            .WithMany()
            .HasForeignKey(rm => rm.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

#endregion Role Assignments

#region Strategic Theme Tags

public class ProjectStrategicThemesConfiguration : IEntityTypeConfiguration<StrategicThemeTag<Project>>
{
    public void Configure(EntityTypeBuilder<StrategicThemeTag<Project>> builder)
    {
        builder.ToTable("ProjectStrategicThemes", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(t => new { t.ObjectId, t.StrategicThemeId });

        builder.HasIndex(t => t.ObjectId);
        builder.HasIndex(t => t.StrategicThemeId);

        // Relationships
        builder.HasOne(t => t.StrategicTheme)
            .WithMany()
            .HasForeignKey(t => t.StrategicThemeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProgramStrategicThemesConfiguration : IEntityTypeConfiguration<StrategicThemeTag<Program>>
{
    public void Configure(EntityTypeBuilder<StrategicThemeTag<Program>> builder)
    {
        builder.ToTable("ProgramStrategicThemes", SchemaNames.ProjectPortfolioManagement);

        builder.HasKey(t => new { t.ObjectId, t.StrategicThemeId });

        builder.HasIndex(t => t.ObjectId);
        builder.HasIndex(t => t.StrategicThemeId);

        // Relationships
        builder.HasOne(t => t.StrategicTheme)
            .WithMany()
            .HasForeignKey(t => t.StrategicThemeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

#endregion Strategic Theme Tags
