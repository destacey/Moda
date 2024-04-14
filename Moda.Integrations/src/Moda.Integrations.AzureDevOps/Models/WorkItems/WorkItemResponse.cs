using Moda.Common.Application.Interfaces.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;
internal class WorkItemResponse
{
    public int Id { get; set; }
    public int? Rev { get; set; }
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
}

internal static class WorkItemResponseExtensions
{
    public static AzdoWorkItem ToAzdoWorkItem(this WorkItemResponse workItem)
    {
        return new AzdoWorkItem()
        {
            Id = workItem.Id,
            Rev = workItem.Rev,
            Fields = workItem.Fields
        };
    }

    public static List<IExternalWorkItem> ToIExternalWorkItems(this List<WorkItemResponse> workItems)
    {
        var result = new List<IExternalWorkItem>(workItems.Count);
        foreach (var workItem in workItems)
        {
            result.Add(workItem.ToAzdoWorkItem());
        }
        return result;
    }
}
