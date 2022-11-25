using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class WorkStateConfig : IEntityTypeConfiguration<WorkState>
{
    public void Configure(EntityTypeBuilder<WorkState> builder)
    {
        builder.ToTable("WorkStates", SchemaNames.Work);
        
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => new { e.IsActive, e.IsDeleted })
            .IncludeProperties(e => new { e.Id, e.Name });

        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(1024);
        builder.Property(e => e.IsActive);

        // Audit
        builder.Property(e => e.Created);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.LastModified);
        builder.Property(e => e.LastModifiedBy);
        builder.Property(e => e.Deleted);
        builder.Property(e => e.DeletedBy);
        builder.Property(e => e.IsDeleted);        
    }
}