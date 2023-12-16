using System.Text.Json;

namespace Moda.Integrations.AzureDevOps.Tests.Models;
public class CommonResponseOptions
{
    protected readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
    };
}
