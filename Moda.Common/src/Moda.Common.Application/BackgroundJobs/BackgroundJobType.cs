using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.BackgroundJobs;
public enum BackgroundJobType
{
    // Integration Jobs

    [Display(Name = "Employee Sync", Description = "Synchronize employees from external directory service.", Order = 1, GroupName = "Integration Jobs")]
    EmployeeSync = 0,

    [Display(Name = "Azure DevOps Boards Full Sync", Description = "Synchronize active connectors for Azure DevOps Boards and return all work items.", Order = 2, GroupName = "Integration Jobs")]
    AzdoBoardsFullSync = 1,

    [Display(Name = "Azure DevOps Boards Differential Sync", Description = "Synchronize active connectors for Azure DevOps Boards and return work items that have changed since the last sync.", Order = 3, GroupName = "Integration Jobs")]
    AzdoBoardsDiffSync = 2,


    // Data Replication Jobs

    [Display(Name = "Team Graph Sync", Description = "Synchronize the latest team data into the Graph tables.", Order = 1000, GroupName = "Data Replication Jobs")]
    TeamGraphSync = 1000,

    [Display(Name = "Strategic Themes Sync", Description = "Synchronize the latest strategic themes data.", Order = 1001, GroupName = "Data Replication Jobs")]
    StrategicThemesSync = 1001
}
