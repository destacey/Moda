using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OneOf.Serialization.SystemTextJson;

namespace Moda.Infrastructure.Common.Services;
public sealed class SystemTextJsonService : ISerializerService
{
    public T Deserialize<T>(string text)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        return JsonSerializer.Deserialize<T>(text, options)!;
    }

    public string Serialize<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new TypeConverter(), // Add the custom TypeConverter
                new OneOfJsonConverter(),
                new OneOfBaseJsonConverter(),
            }
        };
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        return JsonSerializer.Serialize(obj, options);
    }

    public string Serialize<T>(T obj, Type type)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        return JsonSerializer.Serialize(obj, type, options);
    }
}

public class TypeConverter : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var typeName = reader.GetString();
        return Type.GetType(typeName!)!;
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.AssemblyQualifiedName);
    }
}
