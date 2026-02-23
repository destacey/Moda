using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Analytics.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class AnalyticsViewConfiguration : IEntityTypeConfiguration<AnalyticsView>
{
    public void Configure(EntityTypeBuilder<AnalyticsView> builder)
    {
        builder.ToTable(name: "AnalyticsViews", SchemaNames.Analytics);

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.Name)
            .IncludeProperties(o => new { o.Dataset, o.Visibility, o.OwnerId, o.IsActive });

        builder.HasIndex(o => o.OwnerId)
            .IncludeProperties(o => new { o.Name, o.Dataset, o.Visibility, o.IsActive });

        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Description).HasMaxLength(2048);
        builder.Property(o => o.DefinitionJson).IsRequired();
        builder.Property(o => o.Dataset).IsRequired();
        builder.Property(o => o.Visibility).IsRequired();
        builder.Property(o => o.OwnerId).IsRequired();
        builder.Property(o => o.IsActive).IsRequired();
    }
}
