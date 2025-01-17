using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class StrategicThemeConfig : IEntityTypeConfiguration<StrategicTheme>
{
    public void Configure(EntityTypeBuilder<StrategicTheme> builder)
    {
        builder.ToTable("StrategicThemes", SchemaNames.StrategicManagement);

        builder.HasKey(s => s.Id);
        builder.HasAlternateKey(s => s.Key);

        builder.HasIndex(s => s.State);

        builder.Property(s => s.Key).ValueGeneratedOnAdd();
        builder.Property(s => s.Name).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1024).IsRequired();

        builder.Property(s => s.State).IsRequired()
            .HasConversion<EnumConverter<StrategicThemeState>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");
    }
}

public class StrategyConfig : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.ToTable("Strategies", SchemaNames.StrategicManagement);

        builder.HasKey(s => s.Id);
        builder.HasAlternateKey(s => s.Key);

        builder.HasIndex(s => s.Status);

        builder.Property(s => s.Key).ValueGeneratedOnAdd();
        builder.Property(s => s.Name).HasMaxLength(1024).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(3072);

        builder.Property(s => s.Status).IsRequired()
            .HasConversion<EnumConverter<StrategyStatus>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(s => s.Start);
        builder.Property(s => s.End);
    }
}

public class VisionConfig : IEntityTypeConfiguration<Vision>
{
    public void Configure(EntityTypeBuilder<Vision> builder)
    {
        builder.ToTable("Visions", SchemaNames.StrategicManagement);

        builder.HasKey(s => s.Id);
        builder.HasAlternateKey(s => s.Key);

        builder.HasIndex(s => s.State);

        builder.Property(s => s.Key).ValueGeneratedOnAdd();
        builder.Property(s => s.Description).HasMaxLength(3072).IsRequired();

        builder.Property(s => s.State).IsRequired()
            .HasConversion<EnumConverter<VisionState>>()
            .HasMaxLength(32)
            .HasColumnType("varchar");

        builder.Property(s => s.Start);
        builder.Property(s => s.End);
    }
}
