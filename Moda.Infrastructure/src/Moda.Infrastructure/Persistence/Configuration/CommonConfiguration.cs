using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Common.Domain.Models;
using Moda.Common.Models;

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

        builder.Property(e => e.EmployeeNumber).HasMaxLength(256).IsRequired();
        builder.Property(e => e.HireDate);

        builder.Property(e => e.Email)
            .HasConversion(
                e => e.Value,
                e => new EmailAddress(e))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.JobTitle).HasMaxLength(256);
        builder.Property(e => e.Department).HasMaxLength(256);
        builder.Property(e => e.OfficeLocation).HasMaxLength(256);
        builder.Property(e => e.IsActive);

        // Value Objects
        builder.OwnsOne(e => e.Name, options =>
        {
            options.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(100).IsRequired();
            options.Property(e => e.MiddleName).HasColumnName("MiddleName").HasMaxLength(100);
            options.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(100).IsRequired();
            options.Property(e => e.Suffix).HasColumnName("Suffix").HasMaxLength(50);
            options.Property(e => e.Title).HasColumnName("Title").HasMaxLength(50);
        });


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