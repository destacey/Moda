using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wayd.Infrastructure.Persistence.Converters;

namespace Wayd.Infrastructure.Persistence.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(
        this PropertyBuilder<TProperty> builder,
        JsonSerializerOptions? options = null)
    {
        return builder.HasConversion(new JsonValueConverter<TProperty>(options));
    }
}
