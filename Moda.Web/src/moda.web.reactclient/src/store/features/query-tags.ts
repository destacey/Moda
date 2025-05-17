export enum QueryTags {
  // ADMIN
  BackgroundJob = 'Admin.BackgroundJob',
  BackgroundJobType = 'Admin.BackgroundJobType',

  // APP INTEGRATION
  Connection = 'AppIntegration.Connection',
  ConnectionDetail = 'AppIntegration.ConnectionDetail',
  AzdoConnectionTeam = 'AppIntegration.AzdoConnectionTeam',
  AzdoConnectionsConfiguration = 'AppIntegration.AzdoConnectionsConfiguration',
  AzdoConnectionsConfigurationTest = 'AppIntegration.AzdoConnectionsConfigurationTest',

  // COMMON
  Links = 'Common.Links',
  HealthCheck = 'Common.HealthCheck',
  HealthChecksHealthReport = 'Common.HealthChecks.HealthReport',
  HealthChecksStatusOptions = 'Common.HealthChecks.StatusOptions',

  // ORGANIZATIONS
  EmployeeOption = 'Organizations.EmployeeOption',
  Team = 'Organizations.Team',
  TeamBacklog = 'Organizations.TeamBacklog',
  TeamDependency = 'Organizations.TeamDependency',
  FunctionalOrganizationChart = 'Organizations.FunctionalOrganizationChart',

  // PLANNING
  PlanningInterval = 'Planning.PlanningInterval',
  PlanningIntervalCalendar = 'Planning.PlanningInterval.Calendar',
  PlanningIntervalIteration = 'Planning.PlanningInterval.Iteration',
  PlanningIntervalIterationTypeOptions = 'Planning.PlanningInterval.IterationTypeOptions',
  PlanningIntervalTeam = 'Planning.PlanningInterval.Team',
  PlanningIntervalPredictability = 'Planning.PlanningInterval.Predictability',
  PlanningIntervalTeamPredictability = 'Planning.PlanningInterval.TeamPredictability',
  PlanningIntervalObjective = 'Planning.PlanningInterval.Objective',
  PlanningIntervalObjectiveHealthReport = 'Planning.PlanningInterval.Objective.HealthReport',
  PlanningIntervalObjectiveStatus = 'Planning.PlanningInterval.ObjectiveStatus',
  PlanningIntervalObjectiveStatusOptions = 'Planning.PlanningInterval.ObjectiveStatusOptions',
  PlanningIntervalObjectiveWorkItemsSummary = 'Planning.PlanningInterval.Objective.WorkItemsSummary',
  PlanningIntervalObjectiveWorkItemMetric = 'Planning.PlanningInterval.Objective.WorkItemMetric',
  PlanningIntervalRisk = 'Planning.PlanningInterval.Risk',
  Risk = 'Planning.Risk',
  MyRisk = 'Planning.MyRisk',
  RiskStatusOptions = 'Planning.Risk.StatusOptions',
  RiskCategoryOptions = 'Planning.Risk.CategoryOptions',
  RiskGradeOptions = 'Planning.Risk.GradeOptions',
  Roadmap = 'Planning.Roadmap',
  RoadmapItem = 'Planning.RoadmapItem',
  RoadmapActivity = 'Planning.RoadmapActivity',
  RoadmapVisibility = 'Planning.RoadmapVisibility',

  // PPM
  ExpenditureCategory = 'Ppm.ExpenditureCategory',
  Portfolio = 'Ppm.Portfolio',
  PortfolioProjects = 'Ppm.Portfolio.Project',
  PortfolioStrategicInitiatives = 'Ppm.Portfolio.StrategicInitiatives',
  Project = 'Ppm.Project',
  ProjectWorkItems = 'Ppm.Project.WorkItems',
  StrategicInitiative = 'Ppm.StrategicInitiative',
  StrategicInitiativeKpi = 'Ppm.StrategicInitiativeKpi',
  StrategicInitiativeKpiCheckpoint = 'Ppm.StrategicInitiativeKpiCheckpoint',
  StrategicInitiativeKpiMeasurement = 'Ppm.StrategicInitiativeKpiMeasurement',
  StrategicInitiativeKpiUnit = 'Ppm.StrategicInitiativeKpiUnit',
  StrategicInitiativeKpiTargetDirection = 'Ppm.StrategicInitiativeKpiTargetDirection',
  StrategicInitiativeProject = 'Ppm.StrategicInitiativeProject',

  // STRATEGIC MANAGEMENT
  StrategicTheme = 'StrategicManagement.StrategicTheme',
  StrategicThemState = 'StrategicManagement.StrategicThemeState',

  // USER MANAGEMENT
  User = 'UserManagement.User',
  UserRole = 'UserManagement.UserRole',
  Role = 'UserManagement.Role',
  Permission = 'UserManagement.Permission',

  // WORK MANAGEMENT
  WorkItem = 'Work.WorkItem',
  WorkItemChildren = 'Work.WorkItem.Children',
  WorkItemDependency = 'Work.WorkItem.Dependency',
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
