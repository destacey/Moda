export enum QueryTags {
  // ADMIN
  BackgroundJob = 'Admin.BackgroundJob',
  BackgroundJobType = 'Admin.BackgroundJobType',

  // APP INTEGRATION
  Connections = 'AppIntegration.Connections',
  ConnectionDetails = 'AppIntegration.ConnectionDetails',
  AzdoConnectionTeams = 'AppIntegration.AzdoConnectionTeams',
  AzdoConnectionsConfigurations = 'AppIntegration.AzdoConnectionsConfigurations',
  AzdoConnectionsConfigurationTest = 'AppIntegration.AzdoConnectionsConfigurationTest',

  // COMMON
  Links = 'Common.Links',
  HealthChecks = 'Common.HealthChecks',
  HealthChecksHealthReport = 'Common.HealthChecks.HealthReport',
  HealthChecksStatusOptions = 'Common.HealthChecks.StatusOptions',

  // ORGANIZATIONS
  EmployeeOptions = 'Organizations.EmployeeOptions',
  Team = 'Organizations.Team',
  TeamBacklog = 'Organizations.TeamBacklog',
  TeamDependencies = 'Organizations.TeamDependencies',
  FunctionalOrganizationChart = 'Organizations.FunctionalOrganizationChart',

  // PLANNING
  PlanningInterval = 'Planning.PlanningInterval',
  PlanningIntervalObjective = 'Planning.PlanningIntervalObjective',
  PlanningIntervalObjectiveWorkItemsSummary = 'Planning.PlanningIntervalObjective.WorkItemsSummary',
  PlanningIntervalObjectiveWorkItemMetrics = 'Planning.PlanningIntervalObjective.WorkItemMetrics',
  Roadmap = 'Planning.Roadmap',
  RoadmapItems = 'Planning.RoadmapItems',
  RoadmapActivities = 'Planning.RoadmapActivities',
  RoadmapVisibility = 'Planning.RoadmapVisibility',

  // PPM
  Portfolio = 'Ppm.Portfolio',

  // STRATEGIC MANAGEMENT
  StrategicTheme = 'StrategicManagement.StrategicTheme',
  StrategicThemState = 'StrategicManagement.StrategicThemeState',

  // USER MANAGEMENT
  Users = 'UserManagement.Users',
  UserRoles = 'UserManagement.UserRoles',
  Roles = 'UserManagement.Roles',
  Permissions = 'UserManagement.Permissions',

  // WORK MANAGEMENT
  WorkItem = 'Work.WorkItem',
  WorkItemChildren = 'Work.WorkItem.Children',
  WorkItemDependencies = 'Work.WorkItem.Dependencies',
  WorkItemMetrics = 'Work.WorkItem.Metrics',
  WorkItemSearch = 'Work.WorkItem.Search',
  WorkProcess = 'Work.WorkProcess',
  WorkProcessScheme = 'Work.WorkProcessScheme',
  Workspace = 'Work.Workspace',
  WorkStatus = 'Work.WorkStatus',
  WorkType = 'Work.WorkType',
  WorkTypeLevel = 'Work.WorkTypeLevel',
  WorkTypeLevelOption = 'Work.WorkTypeLevelOption',
  WorkTypeTier = 'Work.WorkTypeTier',
}
