using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Goals.Domain.Enums;
using Moda.Goals.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;
public class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
{
    public void Configure(EntityTypeBuilder<Objective> builder)
    {
        builder.ToTable(name: "Objectives", SchemaNames.Goals);

        builder.HasKey(o => o.Id);
        builder.HasAlternateKey(o => o.Key);

        builder.HasIndex(o => new { o.Id, o.IsDeleted })
            .IncludeProperties(o => new { o.Key, o.Name, o.Type, o.Status, o.OwnerId, o.PlanId});
        builder.HasIndex(o => new { o.Key, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Name, o.Type, o.Status, o.OwnerId, o.PlanId });
        builder.HasIndex(o => new { o.OwnerId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.Type, o.Status, o.PlanId });
        builder.HasIndex(o => new { o.PlanId, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.Type, o.Status, o.OwnerId });
        builder.HasIndex(o => o.IsDeleted)
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.Type, o.Status, o.OwnerId, o.PlanId });

        builder.Property(o => o.Key).ValueGeneratedOnAdd();

        builder.Property(o => o.Name).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Description).HasMaxLength(1024);
        builder.Property(o => o.Type).IsRequired()
            .HasConversion<EnumConverter<ObjectiveType>>()
            .HasMaxLength(64);
        builder.Property(o => o.Status).IsRequired()
            .HasConversion<EnumConverter<ObjectiveStatus>>()
            .HasMaxLength(64);
        builder.Property(o => o.Progress).IsRequired();
        builder.Property(o => o.OwnerId);
        builder.Property(o => o.PlanId);
        builder.Property(o => o.StartDate);
        builder.Property(o => o.TargetDate);
        builder.Property(o => o.ClosedDate);

        // Audit
        builder.Property(o => o.Created);
        builder.Property(o => o.CreatedBy);
        builder.Property(o => o.LastModified);
        builder.Property(o => o.LastModifiedBy);
        builder.Property(o => o.Deleted);
        builder.Property(o => o.DeletedBy);
        builder.Property(o => o.IsDeleted);

        // Relationships

        // Ignore
    }
}
