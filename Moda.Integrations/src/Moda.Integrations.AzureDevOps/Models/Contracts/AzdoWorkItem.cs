using Moda.Common.Application.Interfaces.Work;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public record AzdoWorkItem : IExternalWorkItem
{
    public int? Id { get; set; }
    public int? Rev { get; set; }

    // TODO: flatten this out to the fields we want to use
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
}
