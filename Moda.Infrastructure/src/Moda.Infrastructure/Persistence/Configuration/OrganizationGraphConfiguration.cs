using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(o => o.ActiveTimestamp).IsRequired();
        builder.Property(o => o.InactiveTimestamp);
        builder.Property(o => o.IsActive);
        builder.Property(o => o.IsDeleted);


        // Indexes
        builder.HasIndex(o => new { o.Id, o.IsDeleted })
            .IncludeProperties(o => new { o.Key, o.Name, o.Code, o.IsActive })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(o => new { o.Key, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Name, o.Code, o.IsActive })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(o => o.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(o => o.Code)
            .IsUnique()
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.IsActive, o.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(o => new { o.IsActive, o.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.ActiveTimestamp, x.InactiveTimestamp, x.IsDeleted })
            .HasDatabaseName("IX_TeamNodes_ActiveTimestamps")
            .HasFilter("[IsDeleted] = 0");
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
        builder.Property(m => m.IsDeleted);

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
        builder.HasIndex(e => new { e.StartDate, e.EndDate, e.IsDeleted })
            .HasDatabaseName("IX_TeamMembershipEdges_Active")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.EndDate, e.StartDate, e.IsDeleted })
            .HasDatabaseName("IX_TeamMembershipEdges_DateRange")
            .HasFilter("[IsDeleted] = 0");

        // Indexes for graph traversal
        builder.HasIndex("FromNodeId", nameof(TeamMembershipEdge.StartDate), nameof(TeamMembershipEdge.EndDate), nameof(TeamMembershipEdge.IsDeleted))
            .HasDatabaseName("IX_TeamMembershipEdges_FromNode")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex("ToNodeId", nameof(TeamMembershipEdge.StartDate), nameof(TeamMembershipEdge.EndDate), nameof(TeamMembershipEdge.IsDeleted))
            .HasDatabaseName("IX_TeamMembershipEdges_ToNode")
            .HasFilter("[IsDeleted] = 0");
    }
}
