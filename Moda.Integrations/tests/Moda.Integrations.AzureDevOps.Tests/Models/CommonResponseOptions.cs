using System.Text.Json;
using Moda.Integrations.AzureDevOps.Models.Converters;

namespace Moda.Integrations.AzureDevOps.Tests.Models;
public class CommonResponseOptions
{
    protected readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new ReportingWorkItemLinkResponseConverter() }
    };
}
