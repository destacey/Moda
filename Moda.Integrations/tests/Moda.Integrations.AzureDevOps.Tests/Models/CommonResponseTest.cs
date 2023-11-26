using System.Text.Json;

namespace Moda.Integrations.AzureDevOps.Tests.Models;
public class CommonResponseTest
{
    protected readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
}
