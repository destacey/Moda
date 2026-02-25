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
            .IncludeProperties(o => new { o.Dataset, o.Visibility, o.IsActive });

        builder.HasIndex(o => o.Visibility)
            .IncludeProperties(o => new { o.Id, o.Name, o.Dataset, o.IsActive });

        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Description).HasMaxLength(2048);
        builder.Property(o => o.DefinitionJson).IsRequired();
        builder.Property(o => o.Dataset).IsRequired();
        builder.Property(o => o.Visibility).IsRequired();
        builder.Property(o => o.IsActive).IsRequired();

        builder.HasMany(o => o.AnalyticsViewManagers)
            .WithOne()
            .HasForeignKey(m => m.AnalyticsViewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AnalyticsViewManagerConfiguration : IEntityTypeConfiguration<AnalyticsViewManager>
{
    public void Configure(EntityTypeBuilder<AnalyticsViewManager> builder)
    {
        builder.ToTable("AnalyticsViewManagers", SchemaNames.Analytics);

        builder.HasKey(m => new { m.AnalyticsViewId, m.ManagerId });

        builder.HasIndex(m => m.AnalyticsViewId)
            .IncludeProperties(m => new { m.ManagerId });

        builder.HasIndex(m => new { m.AnalyticsViewId, m.ManagerId });

        builder.Property(m => m.AnalyticsViewId).IsRequired();
        builder.Property(m => m.ManagerId).IsRequired();
    }
}
