using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Enums.Organization;

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
        builder.HasAlternateKey(o => o.Key);

        builder.HasIndex(o => new { o.Id, o.IsDeleted })
            .IncludeProperties(o => new { o.Key, o.Name, o.Code, o.IsActive })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.Key, o.IsDeleted })
            .IncludeProperties(o => new { o.Id, o.Name, o.Code, o.IsActive })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.Name)
            .IsUnique();
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .IncludeProperties(o => new { o.Id, o.Key, o.Name, o.IsActive, o.IsDeleted });
        builder.HasIndex(o => new { o.IsActive, o.IsDeleted })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(o => o.Key).ValueGeneratedOnAdd();

        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Code).IsRequired()
            .HasConversion(
                o => o.Value,
                o => new TeamCode(o))
            .HasColumnType("varchar")
            .HasMaxLength(10);
        builder.Property(o => o.Description).HasMaxLength(1024);
        builder.Property(t => t.Type).IsRequired()
            .HasConversion<EnumConverter<TeamType>>()
            .HasColumnType("varchar")
            .HasMaxLength(32);
        builder.Property(o => o.ActiveDate).IsRequired();
        builder.Property(o => o.InactiveDate);
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

        builder.HasIndex(m => new { m.Id, m.IsDeleted })
            .IncludeProperties(m => new { m.SourceId, m.TargetId })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(m => new { m.SourceId, m.IsDeleted })
            .IncludeProperties(m => new { m.Id, m.TargetId })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(m => new { m.TargetId, m.IsDeleted })
            .IncludeProperties(m => new { m.Id, m.SourceId })
            .HasFilter("[IsDeleted] = 0");

        // Value Objects
        builder.ComplexProperty(m => m.DateRange, options =>
        {
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