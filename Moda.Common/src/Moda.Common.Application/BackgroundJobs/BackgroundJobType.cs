using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.BackgroundJobs;
public enum BackgroundJobType
{
    [Display(Name = "Employee Sync", Description = "Synchronize employees from external directory service.", Order = 1)]
    EmployeeSync = 0,

    [Display(Name = "Azure DevOps Boards Sync", Description = "Synchronize active connectors for Azure DevOps Boards.", Order = 2)]
    AzdoBoardsSync = 1,
}
