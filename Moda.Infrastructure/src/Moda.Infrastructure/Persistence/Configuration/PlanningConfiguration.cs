using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        builder.Property(p => p.Name).HasMaxLength(128);
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
