using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums.AppIntegrations;

namespace Moda.Infrastructure.Persistence.Configuration;

public class ConnectionConfig : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("Connections", SchemaNames.AppIntegration);

        builder.HasKey(c => c.Id);
        builder.HasDiscriminator(c => c.Connector)
            .HasValue<AzureDevOpsBoardsConnection>(Connector.AzureDevOps)
            .HasValue<AzureOpenAIConnection>(Connector.AzureOpenAI);

        builder.HasIndex(c => new { c.Id, c.IsDeleted })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.Connector, c.IsActive, c.IsDeleted })
            .IncludeProperties(c => new { c.Id, c.Name })
            .HasFilter("[IsDeleted] = 0"); ;
        builder.HasIndex(c => new { c.IsActive, c.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Description).HasMaxLength(1024);
        builder.Property(c => c.SystemId)
            .HasColumnType("varchar")
            .HasMaxLength(64);
        builder.Property(w => w.Connector).IsRequired()
            .HasConversion<EnumConverter<Connector>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(c => c.IsActive);
        builder.Property(c => c.IsValidConfiguration);
        builder.Property(c => c.IsSyncEnabled);

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
            ownedBuilder.OwnsMany(conf => conf.Workspaces, wb =>
            {
                wb.OwnsOne(w => w.IntegrationState);
            });
            ownedBuilder.OwnsMany(conf => conf.WorkProcesses, wb =>
            {
                wb.OwnsOne(w => w.IntegrationState);
            });
        });

        builder.OwnsOne(c => c.TeamConfiguration, ownedBuilder =>
        {
            ownedBuilder.ToJson();
            ownedBuilder.OwnsMany(conf => conf.WorkspaceTeams);
        });
    }
}

public class AzureOpenAIConnectionConfig : IEntityTypeConfiguration<AzureOpenAIConnection>
{
    public void Configure(EntityTypeBuilder<AzureOpenAIConnection> builder)
    {
        builder.OwnsOne(c => c.Configuration, ownedBuilder =>
        {
            ownedBuilder.ToJson();
        });
    }
}