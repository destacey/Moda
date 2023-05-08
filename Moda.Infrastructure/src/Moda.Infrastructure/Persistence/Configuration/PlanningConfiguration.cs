using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Organization.Domain.Enums;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ProgramIncrementConfig : IEntityTypeConfiguration<ProgramIncrement>
{
    public void Configure(EntityTypeBuilder<ProgramIncrement> builder)
    {
        builder.ToTable("ProgramIncrements", SchemaNames.Planning);

        builder.HasKey(p => p.Id);
        builder.HasAlternateKey(p => p.LocalId);

        builder.HasIndex(p => p.Id)
            .IncludeProperties(p => new { p.Name, p.Description });
        builder.HasIndex(p => p.Name)
            .IsUnique();
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.LocalId).ValueGeneratedOnAdd();

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

public class ProgramIncrementTeamConfig : IEntityTypeConfiguration<ProgramIncrementTeam>
{
    public void Configure(EntityTypeBuilder<ProgramIncrementTeam> builder)
    {
        builder.ToTable("ProgramIncrementTeams", SchemaNames.Planning);

        builder.HasKey(p => new { p.ProgramIncrementId, p.TeamId });

        builder.HasIndex(p => p.ProgramIncrementId)
            .IncludeProperties(p => p.TeamId);

        builder.Property(p => p.ProgramIncrementId).IsRequired();
        builder.Property(p => p.TeamId).IsRequired();

        builder.HasOne<ProgramIncrement>()
            .WithMany(p => p.ProgramIncrementTeams)
            .HasForeignKey(p => p.ProgramIncrementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RiskConfig : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks", SchemaNames.Planning);

        builder.HasKey(r => r.Id);
        builder.HasAlternateKey(r => r.LocalId);

        builder.HasIndex(r => r.Id);
        builder.HasIndex(r => r.IsDeleted);

        builder.Property(r => r.LocalId).ValueGeneratedOnAdd();

        builder.Property(r => r.Summary).HasMaxLength(256).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(1024);
        builder.Property(r => r.TeamId);
        builder.Property(r => r.ReportedOn).IsRequired();
        builder.Property(r => r.ReportedBy).IsRequired();

        builder.Property(r => r.Status).IsRequired()
            .HasConversion<EnumConverter<RiskStatus>>()
            .HasMaxLength(64);

        builder.Property(r => r.Category).IsRequired()
            .HasConversion<EnumConverter<RiskCategory>>()
            .HasMaxLength(64);

        builder.Property(r => r.Impact).IsRequired()
            .HasConversion<EnumConverter<RiskGrade>>()
            .HasMaxLength(64);

        builder.Property(r => r.Likelihood).IsRequired()
            .HasConversion<EnumConverter<RiskGrade>>()
            .HasMaxLength(64);

        builder.Property(r => r.AssigneeId);
        builder.Property(r => r.FollowUpDate);
        builder.Property(r => r.Response);
        builder.Property(r => r.ClosedDate);

        // Audit
        builder.Property(r => r.Created);
        builder.Property(r => r.CreatedBy);
        builder.Property(r => r.LastModified);
        builder.Property(r => r.LastModifiedBy);
        builder.Property(r => r.Deleted);
        builder.Property(r => r.DeletedBy);
        builder.Property(r => r.IsDeleted);
    }
}
