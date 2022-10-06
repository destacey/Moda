using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Moda.Infrastructure.Persistence.Configuration;

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", SchemaNames.Organization);

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.IsActive);

        builder.Property(e => e.EmployeeId).HasMaxLength(50);
        builder.Property(e => e.HireDate);

        builder.Property(e => e.JobTitle).HasMaxLength(256);
        builder.Property(e => e.Department).HasMaxLength(256);
        builder.Property(e => e.IsActive);

        //builder.Property(e => e.DirectReports).HasField("_directReports");

        // Value Objects
        builder.OwnsOne(e => e.Name)
            .Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(100).IsRequired();

        builder.OwnsOne(e => e.Name)
            .Property(e => e.MiddleName).HasColumnName("MiddleName").HasMaxLength(100);

        builder.OwnsOne(e => e.Name)
            .Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(100).IsRequired();

        builder.OwnsOne(e => e.Name)
            .Property(e => e.Suffix).HasColumnName("Suffix").HasMaxLength(50);

        builder.OwnsOne(e => e.Name)
            .Property(e => e.Title).HasColumnName("Title").HasMaxLength(50);

        builder.OwnsOne(e => e.Email)
            .Property(e => e.Value).HasColumnName("Email").HasMaxLength(256);

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