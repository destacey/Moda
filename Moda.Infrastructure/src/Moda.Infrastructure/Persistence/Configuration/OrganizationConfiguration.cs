using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Organization.Domain.Enums;

namespace Moda.Infrastructure.Persistence.Configuration;

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", SchemaNames.Organization);

        builder.HasKey(e => e.Id);
        builder.HasAlternateKey(e => e.LocalId);

        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.EmployeeNumber)
            .IsUnique()
            .IncludeProperties(e => new { e.Id });
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsDeleted);

        builder.Property(e => e.LocalId).ValueGeneratedOnAdd();

        builder.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(256);
        builder.Property(e => e.HireDate);

        builder.Property(e => e.JobTitle).HasMaxLength(256);
        builder.Property(e => e.Department).HasMaxLength(256);
        builder.Property(e => e.OfficeLocation).HasMaxLength(256);
        builder.Property(e => e.IsActive);

        // Value Objects
        //// PersonName
        builder.OwnsOne(e => e.Name)
            .Property(e => e.FirstName).IsRequired().HasColumnName("FirstName").HasMaxLength(100).IsRequired();

        builder.OwnsOne(e => e.Name)
            .Property(e => e.MiddleName).HasColumnName("MiddleName").HasMaxLength(100);

        builder.OwnsOne(e => e.Name)
            .Property(e => e.LastName).IsRequired().HasColumnName("LastName").HasMaxLength(100).IsRequired();

        builder.OwnsOne(e => e.Name)
            .Property(e => e.Suffix).HasColumnName("Suffix").HasMaxLength(50);

        builder.OwnsOne(e => e.Name)
            .Property(e => e.Title).HasColumnName("Title").HasMaxLength(50);

        //// EmailAddress
        builder.OwnsOne(e => e.Email)
            .Property(e => e.Value).IsRequired().HasColumnName("Email").HasMaxLength(256);


        // Audit
        builder.Property(e => e.Created);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.LastModified);
        builder.Property(e => e.LastModifiedBy);
        builder.Property(e => e.Deleted);
        builder.Property(e => e.DeletedBy);
        builder.Property(e => e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.Manager).WithMany(m => m.DirectReports).HasForeignKey(e => e.ManagerId).OnDelete(DeleteBehavior.NoAction);
    }
}

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

        builder.HasIndex(o => o.Id);
        builder.HasIndex(o => o.LocalId)
            .IsUnique()
            .IncludeProperties(e => new { e.Id, e.Name, e.Code });
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .IncludeProperties(e => new { e.Id, e.LocalId, e.Name });
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.IsDeleted);

        builder.Property(o => o.LocalId).ValueGeneratedOnAdd();

        builder.Property(o => o.Name).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Code).IsRequired()
            .HasConversion(
                o => o.ToString(),
                o => new TeamCode(o!))
            .HasMaxLength(10);
        builder.Property(o => o.Description).HasMaxLength(1024);
        builder.Property(o => o.Type).IsRequired()
            .HasConversion<EnumConverter<TeamType>>()
            //.HasConversion(
            //    w => w.ToString(),
            //    w => (TeamType)Enum.Parse(typeof(TeamType), w))
            .HasMaxLength(64);
        builder.Property(o => o.IsActive);


        //builder.OwnsOne(o => o.Code)
        //    .Property(o => o.Value).IsRequired().HasColumnName("Code").HasMaxLength(10);

        // Audit
        builder.Property(o => o.Created);
        builder.Property(o => o.CreatedBy);
        builder.Property(o => o.LastModified);
        builder.Property(o => o.LastModifiedBy);
        builder.Property(o => o.Deleted);
        builder.Property(o => o.DeletedBy);
        builder.Property(o => o.IsDeleted);

        // Relationships
    }
}

public class PersonConfig : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People", SchemaNames.Organization);

        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Key)
            .IsUnique()
            .IncludeProperties(p => new { p.Id });

        builder.Property(p => p.Key).HasMaxLength(256).IsRequired();
    }
}