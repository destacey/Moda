using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

/// <summary>
/// Represents the role an employee can have for a project task.
/// </summary>
public enum TaskAssignmentRole
{
    /// <summary>
    /// Responsible for completing the task.
    /// </summary>
    [Display(Name = "Assignee", Description = "Responsible for completing the task.", Order = 1)]
    Assignee = 1,

    /// <summary>
    /// Reviews the completed task.
    /// </summary>
    [Display(Name = "Reviewer", Description = "Reviews the completed task.", Order = 2)]
    Reviewer = 2
}
