using Moda.Common.Domain.Authorization;

namespace Moda.Web.BlazorClient.Models;

public record PermissionViewModel : ApplicationPermission
{
    public bool Enabled { get; set; }
    
    public PermissionViewModel(string description, string action, string resource, bool isBasic = false, bool isRoot = false)
        : base(description, action, resource, isBasic, isRoot)
    {
    }
}
