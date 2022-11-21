using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.AppIntegration.Domain.Enums;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ConnectorConfig : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.ToTable("Connectors", SchemaNames.AppIntegration);

        builder.HasKey(c => c.Id);
        builder.HasDiscriminator(c => c.Type)
            .HasValue<AzureDevOpsBoardsConnector>(ConnectorType.AzureDevOpsBoards);

        builder.HasIndex(c => c.Id);
        builder.HasIndex(e => new { e.Type, e.IsActive })
            .IncludeProperties(e => new { e.Id, e.Name });
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.IsDeleted);
        
        builder.Property(c => c.Name).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Description).HasMaxLength(1024);
        builder.Property(c => c.Type)
            .HasConversion(
                c => c.ToString(),
                c => (ConnectorType)Enum.Parse(typeof(ConnectorType), c))
            .HasMaxLength(128);
        builder.Property(c => c.ConfigurationString);
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

public class AzureDevOpsBoardsConnectorConfig : IEntityTypeConfiguration<AzureDevOpsBoardsConnector>
{
    public void Configure(EntityTypeBuilder<AzureDevOpsBoardsConnector> builder)
    {
        // Ignore
        builder.Ignore(c => c.Configuration);
    }
}