using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Moda.Infrastructure.Persistence.Configuration;

public class AuditTrailConfig : IEntityTypeConfiguration<Trail>
{
    public void Configure(EntityTypeBuilder<Trail> builder)
    {
        builder.ToTable("AuditTrails", SchemaNames.Auditing);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PrimaryKey);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(32).HasColumnType("varchar");
        builder.Property(x => x.SchemaName).HasMaxLength(64).HasColumnType("varchar");
        builder.Property(x => x.TableName).HasMaxLength(128).HasColumnType("varchar");
        builder.Property(x => x.DateTime).IsRequired();
        builder.Property(x => x.OldValues);
        builder.Property(x => x.NewValues);
        builder.Property(x => x.AffectedColumns);
        builder.Property(x => x.PrimaryKey).HasMaxLength(450);  // needs a max because of index
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
    }
}