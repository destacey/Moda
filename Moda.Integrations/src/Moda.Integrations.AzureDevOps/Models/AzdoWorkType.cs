using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models;
public class AzdoWorkType : IExternalWorkType
{
    public AzdoWorkType(WorkItemType type)
    {
        Name = type.Name;
        ReferenceName = type.ReferenceName;
        Description = type.Description;
    }

    public string Name { get; set; }
    public string ReferenceName { get; set; }
    public string Description { get; set; }
}
