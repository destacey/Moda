using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Planning.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ProgramIncrementConfig : IEntityTypeConfiguration<ProgramIncrement>
{
    public void Configure(EntityTypeBuilder<ProgramIncrement> builder)
    {
        builder.ToTable("ProgramIncrements", SchemaNames.Planning);

        builder.HasKey(e => e.Id);
        builder.HasAlternateKey(e => e.LocalId);

        builder.HasIndex(e => e.Id)
            .IncludeProperties(p => new { p.Name, p.Description });
        builder.HasIndex(e => e.IsDeleted);

        builder.Property(e => e.LocalId).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(1024);

        // Value Objects
        builder.OwnsOne(m => m.DateRange, options =>
        {
            options.HasIndex(i => new { i.Start, i.End });

            options.Property(d => d.Start).HasColumnName("Start").IsRequired();
            options.Property(d => d.End).HasColumnName("End").IsRequired();
        });

        // Audit
        builder.Property(o => o.Created);
        builder.Property(o => o.CreatedBy);
        builder.Property(o => o.LastModified);
        builder.Property(o => o.LastModifiedBy);
        builder.Property(o => o.Deleted);
        builder.Property(o => o.DeletedBy);
        builder.Property(o => o.IsDeleted);
    }
}
