using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Moda.Infrastructure.Persistence.Converters;

public class JsonValueConverter<T> : ValueConverter<T, string>
{
    public JsonValueConverter(JsonSerializerOptions? options = null)
        : base(
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<T>(v, options)!)
    {
    }
}