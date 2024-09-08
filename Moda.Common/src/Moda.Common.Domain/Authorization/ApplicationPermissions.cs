using System.Collections.ObjectModel;

namespace Moda.Common.Domain.Authorization;

public static class ApplicationAction
{
    // Common
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Manage = nameof(Manage);
    public const string Import = nameof(Import);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string Run = nameof(Run);

    // Specific
    public const string ManageTeamMemberships = nameof(ManageTeamMemberships);
}

public static class ApplicationResource
{
    public const string Hangfire = nameof(Hangfire);
    public const string BackgroundJobs = nameof(BackgroundJobs);

    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Permissions = nameof(Permissions);

    public const string Connections = nameof(Connections);
    public const string Connectors = nameof(Connectors);

    public const string Employees = nameof(Employees);
    public const string Teams = nameof(Teams);

    public const string PlanningIntervals = nameof(PlanningIntervals);
    public const string PlanningIntervalObjectives = nameof(PlanningIntervalObjectives);
    public const string Risks = nameof(Risks);
    public const string Roadmaps = nameof(Roadmaps);

    public const string WorkTypeTiers = nameof(WorkTypeTiers);
    public const string WorkTypeLevels = nameof(WorkTypeLevels);
    public const string Workspaces = nameof(Workspaces);
    public const string WorkItems = nameof(WorkItems);
    public const string WorkProcesses = nameof(WorkProcesses);
    public const string WorkStatusCategories = nameof(WorkStatusCategories);
    public const string WorkStatuses = nameof(WorkStatuses);
    public const string WorkTypes = nameof(WorkTypes);

    public const string Links = nameof(Links);

    public const string HealthChecks = nameof(HealthChecks);
}

public static class ApplicationPermissions
{
    private static readonly ApplicationPermission[] _common = [];

    private static readonly ApplicationPermission[] _backgroundJobs =
    [
        new("View Hangfire", ApplicationAction.View, ApplicationResource.Hangfire),
        new("View Background Jobs", ApplicationAction.View, ApplicationResource.BackgroundJobs),
        new("Create Background Jobs", ApplicationAction.Create, ApplicationResource.BackgroundJobs),
        new("Run Background Jobs", ApplicationAction.Run, ApplicationResource.BackgroundJobs)
    ];

    private static readonly ApplicationPermission[] _identity =
    [
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
        new("Update RoleClaims", ApplicationAction.Update, ApplicationResource.RoleClaims),

        new("View Permissions", ApplicationAction.View, ApplicationResource.Permissions)
    ];

    private static readonly ApplicationPermission[] _appIntegration =
    [
        new("View Connections", ApplicationAction.View, ApplicationResource.Connections),
        new("Create Connections", ApplicationAction.Create, ApplicationResource.Connections),
        new("Update Connections", ApplicationAction.Update, ApplicationResource.Connections),
        new("Delete Connections", ApplicationAction.Delete, ApplicationResource.Connections),

        new("View Connectors", ApplicationAction.View, ApplicationResource.Connectors),
    ];

    private static readonly ApplicationPermission[] _healthChecks =
    [
        new("View Health Checks", ApplicationAction.View, ApplicationResource.HealthChecks, IsBasic: true),
        new("Create Health Checks", ApplicationAction.Create, ApplicationResource.HealthChecks, IsBasic: true),
        new("Update Health Checks", ApplicationAction.Update, ApplicationResource.HealthChecks, IsBasic: true),
    ];

    private static readonly ApplicationPermission[] _links =
    [
        new("View Links", ApplicationAction.View, ApplicationResource.Links, IsBasic: true),
        new("Create Links", ApplicationAction.Create, ApplicationResource.Links, IsBasic: true),
        new("Update Links", ApplicationAction.Update, ApplicationResource.Links, IsBasic: true),
        new("Delete Links", ApplicationAction.Delete, ApplicationResource.Links, IsBasic: true),
    ];

    private static readonly ApplicationPermission[] _organization =
    [
        new("View Employees", ApplicationAction.View, ApplicationResource.Employees, IsBasic: true),
        new("Create Employees", ApplicationAction.Create, ApplicationResource.Employees),
        new("Update Employees", ApplicationAction.Update, ApplicationResource.Employees),
        new("Delete Employees", ApplicationAction.Delete, ApplicationResource.Employees),

        new("View Teams and Teams of Teams", ApplicationAction.View, ApplicationResource.Teams, IsBasic: true),
        new("Create Teams", ApplicationAction.Create, ApplicationResource.Teams),
        new("Update Teams", ApplicationAction.Update, ApplicationResource.Teams),
        new("Manage Team Memberships.  This includes adding, updating, and removing team memberships.", ApplicationAction.ManageTeamMemberships, ApplicationResource.Teams),
        new("Delete Teams", ApplicationAction.Delete, ApplicationResource.Teams),
    ];

    private static readonly ApplicationPermission[] _planning =
    [
        new("View Planning Intervals", ApplicationAction.View, ApplicationResource.PlanningIntervals, IsBasic: true),
        new("Create Planning Intervals", ApplicationAction.Create, ApplicationResource.PlanningIntervals),
        new("Update Planning Intervals", ApplicationAction.Update, ApplicationResource.PlanningIntervals),
        new("Delete Planning Intervals", ApplicationAction.Delete, ApplicationResource.PlanningIntervals),

        new("View Planning Interval Objectives", ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives, IsBasic: true),
        new("Create, update, and delete Planning Interval Objectives", ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives, IsBasic: true),
        new("Import Planning Interval Objectives", ApplicationAction.Import, ApplicationResource.PlanningIntervalObjectives),

        new("View Risks", ApplicationAction.View, ApplicationResource.Risks, IsBasic: true),
        new("Create Risks", ApplicationAction.Create, ApplicationResource.Risks, IsBasic: true),
        new("Update Risks", ApplicationAction.Update, ApplicationResource.Risks, IsBasic: true),
        new("Delete Risks", ApplicationAction.Delete, ApplicationResource.Risks, IsBasic: true),
        new("Import Risks", ApplicationAction.Import, ApplicationResource.Risks),

        new("View Roadmaps", ApplicationAction.View, ApplicationResource.Roadmaps, IsBasic: true),
        new("Create Roadmaps", ApplicationAction.Create, ApplicationResource.Roadmaps, IsBasic: true),
        new("Update Roadmaps", ApplicationAction.Update, ApplicationResource.Roadmaps, IsBasic: true),
        new("Delete Roadmaps", ApplicationAction.Delete, ApplicationResource.Roadmaps, IsBasic: true),
    ];

    private static readonly ApplicationPermission[] _work =
    [
        new("View work type tiers", ApplicationAction.View, ApplicationResource.WorkTypeTiers, IsBasic: true),

        new("View work type levels", ApplicationAction.View, ApplicationResource.WorkTypeLevels, IsBasic: true),
        new("Create work type levels", ApplicationAction.Create, ApplicationResource.WorkTypeLevels),
        new("Update work type levels", ApplicationAction.Update, ApplicationResource.WorkTypeLevels),
        new("Delete work type levels", ApplicationAction.Delete, ApplicationResource.WorkTypeLevels),

        new("View Workspaces", ApplicationAction.View, ApplicationResource.Workspaces, IsBasic: true),
        new("Create Workspaces", ApplicationAction.Create, ApplicationResource.Workspaces),
        new("Update Workspaces", ApplicationAction.Update, ApplicationResource.Workspaces),
        new("Delete Workspaces", ApplicationAction.Delete, ApplicationResource.Workspaces),

        new("View WorkItems", ApplicationAction.View, ApplicationResource.WorkItems, IsBasic: true),
        new("Create WorkItems", ApplicationAction.Create, ApplicationResource.WorkItems),
        new("Update WorkItems", ApplicationAction.Update, ApplicationResource.WorkItems),
        new("Delete WorkItems", ApplicationAction.Delete, ApplicationResource.WorkItems),

        new("View WorkTypes", ApplicationAction.View, ApplicationResource.WorkProcesses, IsBasic: true),
        new("Create WorkTypes", ApplicationAction.Create, ApplicationResource.WorkProcesses),
        new("Update WorkTypes", ApplicationAction.Update, ApplicationResource.WorkProcesses),
        new("Delete WorkTypes", ApplicationAction.Delete, ApplicationResource.WorkProcesses),

        new("View WorkStatusCategories", ApplicationAction.View, ApplicationResource.WorkStatusCategories, IsBasic: true),

        new("View WorkStatuses", ApplicationAction.View, ApplicationResource.WorkStatuses, IsBasic: true),
        new("Create WorkStatuses", ApplicationAction.Create, ApplicationResource.WorkStatuses),
        new("Update WorkStatuses", ApplicationAction.Update, ApplicationResource.WorkStatuses),
        new("Delete WorkStatuses", ApplicationAction.Delete, ApplicationResource.WorkStatuses),

        new("View WorkTypes", ApplicationAction.View, ApplicationResource.WorkTypes, IsBasic: true),
        new("Create WorkTypes", ApplicationAction.Create, ApplicationResource.WorkTypes),
        new("Update WorkTypes", ApplicationAction.Update, ApplicationResource.WorkTypes),
        new("Delete WorkTypes", ApplicationAction.Delete, ApplicationResource.WorkTypes),
    ];

    private static readonly ApplicationPermission[] _all = _common
        .Union(_backgroundJobs)
        .Union(_identity)
        .Union(_appIntegration)
        .Union(_healthChecks)
        .Union(_links)
        .Union(_organization)
        .Union(_planning)
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