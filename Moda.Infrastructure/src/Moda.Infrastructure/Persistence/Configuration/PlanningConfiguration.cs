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
