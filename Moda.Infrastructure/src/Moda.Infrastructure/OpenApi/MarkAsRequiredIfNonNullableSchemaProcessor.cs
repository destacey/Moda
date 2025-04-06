using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Moda.Infrastructure.OpenApi;
public class MarkAsRequiredIfNonNullableSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.Schema.Properties is null)
        {
            return;
        }

        foreach (PropertyInfo property in context.ContextualType.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string jsonPropertyName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            if (context.Schema.Properties.ContainsKey(jsonPropertyName) && !IsNullable(property))
            {
                if (!context.Schema.RequiredProperties.Contains(jsonPropertyName))
                {
                    context.Schema.RequiredProperties.Add(jsonPropertyName);
                }
            }
        }
    }

    private static bool IsNullable(PropertyInfo property)
    {
        // For value types, only Nullable<T> is nullable
        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        // For reference types, use NullabilityInfoContext
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(property);

        return nullabilityInfo.WriteState == NullabilityState.Nullable;
    }
}
