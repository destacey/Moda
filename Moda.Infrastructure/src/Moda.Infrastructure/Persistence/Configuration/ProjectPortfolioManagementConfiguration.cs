using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

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

        builder.Property(s => s.Key).ValueGeneratedOnAdd();
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

        builder.HasMany(p => p.StrategicThemes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProgramStrategicThemes",  // Table name
                j => j.HasOne<StrategicTheme>()
                      .WithMany()
                      .HasForeignKey("StrategicThemeId")
                      .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Program>()
                      .WithMany()
                      .HasForeignKey("ProgramId")
                      .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProgramId", "StrategicThemeId"); // Composite Primary Key
                }
            );
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

        builder.HasMany(p => p.StrategicThemes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProjectStrategicThemes",  // Table name
                j => j.HasOne<StrategicTheme>()
                      .WithMany()
                      .HasForeignKey("StrategicThemeId")
                      .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey("ProjectId")
                      .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProjectId", "StrategicThemeId"); // Composite Primary Key
                }
            );
    }
}
