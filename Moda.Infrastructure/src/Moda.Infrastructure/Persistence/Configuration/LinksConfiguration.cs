using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Links.Models;

namespace Moda.Infrastructure.Persistence.Configuration;
public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable(name: "Links", SchemaNames.Links);

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.Id)
            .IncludeProperties(o => new { o.ObjectId, o.Name, o.Url });
        builder.HasIndex(o => o.ObjectId)
            .IncludeProperties(o => new { o.Id, o.Name, o.Url});

        builder.Property(o => o.ObjectId).IsRequired();
        builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
        builder.Property(o => o.Url).IsRequired();
    }
}
