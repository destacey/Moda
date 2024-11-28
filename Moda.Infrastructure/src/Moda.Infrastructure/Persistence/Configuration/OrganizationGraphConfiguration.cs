using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums.Organization;
using Moda.Organization.Application.Teams.Models;

namespace Moda.Infrastructure.Persistence.Configuration;

public class TeamNodeConfig : IEntityTypeConfiguration<TeamNode>
{
    public void Configure(EntityTypeBuilder<TeamNode> builder)
    {
        builder.ToTable("TeamNodes", SchemaNames.Organization);

        builder.HasKey(o => o.Id);
        builder.HasAlternateKey(o => o.Key);

        // Properties
        builder.Property(o => o.Key);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Code).IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(10);
        builder.Property(t => t.Type).IsRequired()
            .HasConversion<EnumConverter<TeamType>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(o => o.ActiveDate).IsRequired();
        builder.Property(o => o.InactiveDate);
        builder.Property(o => o.IsActive);


        // Indexes
        builder.HasIndex(o => new { o.Id })
            .IncludeProperties(o => new { o.Key, o.Name, o.Code, o.IsActive });

        builder.HasIndex(o => new { o.Key })
            .IncludeProperties(o => new { o.Id, o.Name, o.Code, o.IsActive });

        builder.HasIndex(o => o.Name)
            .IsUnique();

        builder.HasIndex(o => o.Code)
            .IsUnique()
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.IsActive });

        builder.HasIndex(o => new { o.IsActive });

        builder.HasIndex(x => new { x.ActiveDate, x.InactiveDate })
            .HasDatabaseName("IX_TeamNodes_ActiveDates");
    }
}

public class TeamMembershipEdgeConfig : IEntityTypeConfiguration<TeamMembershipEdge>
{
    public void Configure(EntityTypeBuilder<TeamMembershipEdge> builder)
    {
        builder.ToTable("TeamMembershipEdges", SchemaNames.Organization);

        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.StartDate).IsRequired();
        builder.Property(m => m.EndDate);

        // Relationships
        builder.HasOne(e => e.FromNode)
            .WithMany(n => n.ParentMemberships)
            .HasForeignKey("FromNodeId")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.ToNode)
            .WithMany(n => n.ChildMemberships)
            .HasForeignKey("ToNodeId")
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes for temporal queries
        builder.HasIndex(e => new { e.StartDate, e.EndDate })
            .HasDatabaseName("IX_TeamMembershipEdges_Active");

        builder.HasIndex(e => new { e.EndDate, e.StartDate })
            .HasDatabaseName("IX_TeamMembershipEdges_DateRange");

        // Indexes for graph traversal
        builder.HasIndex("FromNodeId", nameof(TeamMembershipEdge.StartDate), nameof(TeamMembershipEdge.EndDate))
            .HasDatabaseName("IX_TeamMembershipEdges_FromNode");

        builder.HasIndex("ToNodeId", nameof(TeamMembershipEdge.StartDate), nameof(TeamMembershipEdge.EndDate))
            .HasDatabaseName("IX_TeamMembershipEdges_ToNode");
    }
}
