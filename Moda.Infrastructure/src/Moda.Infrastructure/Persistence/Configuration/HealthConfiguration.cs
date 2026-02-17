using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums;
using Moda.Health.Models;
using Moda.Infrastructure.Persistence.Converters;

namespace Moda.Infrastructure.Persistence.Configuration;
public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable(name: "HealthChecks", SchemaNames.Health);

        builder.HasKey(h => h.Id);

        builder.HasIndex(h => h.Id)
            .IncludeProperties(o => new { o.ObjectId });
        builder.HasIndex(h => h.ObjectId)
            .IncludeProperties(o => new { o.Id });

        builder.Property(h => h.ObjectId).IsRequired();

        builder.Property(h => h.Context).IsRequired()
            .HasConversion<EnumConverter<SystemContext>>()
            .HasColumnType("varchar")
            .HasMaxLength(64);

        builder.Property(h => h.Status).IsRequired()
            .HasConversion<EnumConverter<HealthStatus>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);

        builder.Property(h => h.ReportedOn).IsRequired();
        builder.Property(h => h.Expiration).IsRequired();
        builder.Property(h => h.Note).HasMaxLength(1024);

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
    }
}
