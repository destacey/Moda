using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.AppIntegration.Domain.Enums;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ConnectionConfig : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("Connections", SchemaNames.AppIntegration);

        builder.HasKey(c => c.Id);
        builder.HasDiscriminator(c => c.Connector)
            .HasValue<AzureDevOpsBoardsConnection>(Connector.AzureDevOpsBoards);

        builder.HasIndex(c => c.Id);
        builder.HasIndex(e => new { e.Connector, e.IsActive })
            .IncludeProperties(e => new { e.Id, e.Name });
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.IsDeleted);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Description).HasMaxLength(1024);
        builder.Property(c => c.Connector)
            .HasConversion(
                c => c.ToString(),
                c => (Connector)Enum.Parse(typeof(Connector), c))
            .HasMaxLength(128);
        builder.Property(c => c.IsActive);
        builder.Property(c => c.IsValidConfiguration);

        // Audit
        builder.Property(c => c.Created);
        builder.Property(c => c.CreatedBy);
        builder.Property(c => c.LastModified);
        builder.Property(c => c.LastModifiedBy);
        builder.Property(c => c.Deleted);
        builder.Property(c => c.DeletedBy);
        builder.Property(c => c.IsDeleted);

        // Relationships
    }
}

public class AzureDevOpsBoardsConnectionConfig : IEntityTypeConfiguration<AzureDevOpsBoardsConnection>
{
    public void Configure(EntityTypeBuilder<AzureDevOpsBoardsConnection> builder)
    {
        builder.OwnsOne(c => c.Configuration, ownedBuilder =>
        {
            ownedBuilder.ToJson();
            ownedBuilder.OwnsMany(conf => conf.Workspaces);
        });
    }
}