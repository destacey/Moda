using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Application.BackgroundJobs;
public enum BackgroundJobType
{
    [Display(Name = "Employee Sync", Description = "Synchronize employees from external directory service.", Order = 1)]
    EmployeeImport = 0
}
