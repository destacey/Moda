using System.Collections.ObjectModel;

namespace Moda.Common.Domain.Authorization;

public static class ApplicationAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
}

public static class ApplicationResource
{
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);

    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);

    public const string Employees = nameof(Employees);

    public const string Workspaces = nameof(Workspaces);
    public const string WorkItems = nameof(WorkItems);
}

public static class ApplicationPermissions
{
    private static readonly ApplicationPermission[] _common = new ApplicationPermission[]
    {
        new("View Dashboard", ApplicationAction.View, ApplicationResource.Dashboard),
        new("View Hangfire", ApplicationAction.View, ApplicationResource.Hangfire)
    };

    private static readonly ApplicationPermission[] _identity = new ApplicationPermission[]
    {
        new("View Users", ApplicationAction.View, ApplicationResource.Users),
        new("Search Users", ApplicationAction.Search, ApplicationResource.Users),
        new("Create Users", ApplicationAction.Create, ApplicationResource.Users),
        new("Update Users", ApplicationAction.Update, ApplicationResource.Users),
        new("Delete Users", ApplicationAction.Delete, ApplicationResource.Users),
        new("Export Users", ApplicationAction.Export, ApplicationResource.Users),
        new("View UserRoles", ApplicationAction.View, ApplicationResource.UserRoles),
        new("Update UserRoles", ApplicationAction.Update, ApplicationResource.UserRoles),
        new("View Roles", ApplicationAction.View, ApplicationResource.Roles),
        new("Create Roles", ApplicationAction.Create, ApplicationResource.Roles),
        new("Update Roles", ApplicationAction.Update, ApplicationResource.Roles),
        new("Delete Roles", ApplicationAction.Delete, ApplicationResource.Roles),
        new("View RoleClaims", ApplicationAction.View, ApplicationResource.RoleClaims),
        new("Update RoleClaims", ApplicationAction.Update, ApplicationResource.RoleClaims)
    };

    private static readonly ApplicationPermission[] _organization = new ApplicationPermission[]
    {
        new("View Employees", ApplicationAction.View, ApplicationResource.Employees),
        new("Create Employees", ApplicationAction.Create, ApplicationResource.Employees),
        new("Update Employees", ApplicationAction.Update, ApplicationResource.Employees),
        new("Delete Employees", ApplicationAction.Delete, ApplicationResource.Employees),
    };

    private static readonly ApplicationPermission[] _work = new ApplicationPermission[]
    {
        new("View Workspaces", ApplicationAction.View, ApplicationResource.Workspaces, IsBasic: true),
        new("Search Workspaces", ApplicationAction.Search, ApplicationResource.Workspaces, IsBasic: true),
        new("Create Workspaces", ApplicationAction.Create, ApplicationResource.Workspaces),
        new("Update Workspaces", ApplicationAction.Update, ApplicationResource.Workspaces),
        new("Delete Workspaces", ApplicationAction.Delete, ApplicationResource.Workspaces),
        new("View WorkItems", ApplicationAction.View, ApplicationResource.WorkItems, IsBasic: true),
        new("Search WorkItems", ApplicationAction.Search, ApplicationResource.WorkItems, IsBasic: true),
        new("Create WorkItems", ApplicationAction.Create, ApplicationResource.WorkItems),
        new("Update WorkItems", ApplicationAction.Update, ApplicationResource.WorkItems),
        new("Delete WorkItems", ApplicationAction.Delete, ApplicationResource.WorkItems),
        new("Generate WorkItems", ApplicationAction.Generate, ApplicationResource.WorkItems)
    };

    private static readonly ApplicationPermission[] _all = _common
        .Union(_identity)
        .Union(_organization)
        .Union(_work)
        .ToArray();

    public static IReadOnlyList<ApplicationPermission> All { get; } = new ReadOnlyCollection<ApplicationPermission>(_all);
    public static IReadOnlyList<ApplicationPermission> Root { get; } = new ReadOnlyCollection<ApplicationPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<ApplicationPermission> Admin { get; } = new ReadOnlyCollection<ApplicationPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<ApplicationPermission> Basic { get; } = new ReadOnlyCollection<ApplicationPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record ApplicationPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}