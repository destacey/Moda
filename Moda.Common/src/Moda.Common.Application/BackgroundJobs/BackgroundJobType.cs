using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.BackgroundJobs;
public enum BackgroundJobType
{
    [Display(Name = "Employee Sync", Description = "Synchronize employees from external directory service.", Order = 1)]
    EmployeeSync = 0,

    [Display(Name = "Azure DevOps Boards Full Sync", Description = "Synchronize active connectors for Azure DevOps Boards and return all work items.", Order = 2)]
    AzdoBoardsFullSync = 1,

    [Display(Name = "Azure DevOps Boards Differential Sync", Description = "Synchronize active connectors for Azure DevOps Boards and return work items that have changed since the last sync.", Order = 3)]
    AzdoBoardsDiffSync = 2,
}
