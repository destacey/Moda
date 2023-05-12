using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Organization.Domain.Enums;

namespace Moda.Infrastructure.Persistence.Configuration;

public class BaseTeamConfig : IEntityTypeConfiguration<BaseTeam>
{
    public void Configure(EntityTypeBuilder<BaseTeam> builder)
    {
        builder.ToTable("Teams", SchemaNames.Organization);

        builder.HasDiscriminator(c => c.Type)
            .HasValue<Team>(TeamType.Team)
            .HasValue<TeamOfTeams>(TeamType.TeamOfTeams);

        builder.HasKey(o => o.Id);
        builder.HasAlternateKey(o => o.LocalId);

        builder.HasIndex(o => o.Id)
            .IncludeProperties(o => new { o.LocalId, o.Name, o.Code, o.IsActive, o.IsDeleted });
        builder.HasIndex(o => o.LocalId)
            .IncludeProperties(o => new { o.Id, o.Name, o.Code, o.IsActive, o.IsDeleted });
        builder.HasIndex(o => o.Name)
            .IsUnique();
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .IncludeProperties(o => new { o.Id, o.LocalId, o.Name, o.IsActive, o.IsDeleted });
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.IsDeleted)
            .IncludeProperties(o => new { o.Id, o.LocalId, o.Name, o.Code, o.Type, o.IsActive });

        builder.Property(o => o.LocalId).ValueGeneratedOnAdd();

        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Code).IsRequired()
            .HasConversion(
                o => o.Value,
                o => new TeamCode(o))
            .HasMaxLength(10);
        builder.Property(o => o.Description).HasMaxLength(1024);
        builder.Property(o => o.Type).IsRequired()
            .HasConversion<EnumConverter<TeamType>>()
            .HasMaxLength(64);
        builder.Property(o => o.IsActive);

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

public class TeamMembershipConfig : IEntityTypeConfiguration<TeamMembership>
{
    public void Configure(EntityTypeBuilder<TeamMembership> builder)
    {
        builder.ToTable("TeamMemberships", SchemaNames.Organization);

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.Id)
            .IncludeProperties(m => new { m.SourceId, m.TargetId, m.IsDeleted });

        // Value Objects
        builder.OwnsOne(m => m.DateRange, options =>
        {
            options.HasIndex(i => new { i.Start, i.End });

            options.Property(d => d.Start).HasColumnName("Start").IsRequired();
            options.Property(d => d.End).HasColumnName("End");
        });

        // Relationships
        builder.HasOne(o => o.Source)
            .WithMany(m => m.ParentMemberships)
            .HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(o => o.Target).WithMany(m => m.ChildMemberships).HasForeignKey(m => m.TargetId).OnDelete(DeleteBehavior.NoAction);

        // Audit
        builder.Property(o => o.Created);
        builder.Property(o => o.CreatedBy);
        builder.Property(o => o.LastModified);
        builder.Property(o => o.LastModifiedBy);
        builder.Property(o => o.Deleted);
        builder.Property(o => o.DeletedBy);
        builder.Property(o => o.IsDeleted);
    }
}