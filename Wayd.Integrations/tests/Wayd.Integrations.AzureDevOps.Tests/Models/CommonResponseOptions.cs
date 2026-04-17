using System.Text.Json;
using Wayd.Integrations.AzureDevOps.Models.Converters;

namespace Wayd.Integrations.AzureDevOps.Tests.Models;

public class CommonResponseOptions
{
    protected readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new ReportingWorkItemLinkResponseConverter() }
    };
}
