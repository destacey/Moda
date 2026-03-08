using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Infrastructure.Persistence.Configuration;
public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable(name: "FeatureFlags", SchemaNames.FeatureManagement);

        builder.HasKey(f => f.Id);

        builder.HasIndex(f => f.Name)
            .IsUnique();

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(f => f.DisplayName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(f => f.Description)
            .HasMaxLength(1024);

        builder.Property(f => f.IsEnabled)
            .IsRequired();

        builder.Property(f => f.IsArchived)
            .IsRequired();

        builder.Property(f => f.IsSystem)
            .IsRequired();

        builder.Property(f => f.FiltersJson)
            .HasColumnType("nvarchar(max)");
    }
}
