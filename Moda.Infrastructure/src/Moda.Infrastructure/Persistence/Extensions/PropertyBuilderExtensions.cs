using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moda.Infrastructure.Persistence.Converters;

namespace Moda.Infrastructure.Persistence.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(
        this PropertyBuilder<TProperty> builder,
        JsonSerializerOptions? options = null)
    {
        return builder.HasConversion(new JsonValueConverter<TProperty>(options));
    }
}
