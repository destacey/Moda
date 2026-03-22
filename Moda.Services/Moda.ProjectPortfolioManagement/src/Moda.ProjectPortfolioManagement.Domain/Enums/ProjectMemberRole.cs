using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Domain.Enums;

/// <summary>
/// Represents the combined set of roles a user can have on a project,
/// including both project-level roles and task-level participation.
/// Used for filtering projects by the current user's involvement.
/// Values 1-4 map directly to <see cref="ProjectRole"/>.
/// </summary>
public enum ProjectMemberRole
{
    [Display(Name = "Project Sponsor", Description = "Provides overall guidance, support, and funding for the project.", Order = 1)]
    Sponsor = 1,

    [Display(Name = "Project Owner", Description = "Accountable for the project's outcomes and ensures alignment with business goals.", Order = 2)]
    Owner = 2,

    [Display(Name = "Project Manager", Description = "Responsible for planning, execution, and successful delivery of the project.", Order = 3)]
    Manager = 3,

    [Display(Name = "Project Member", Description = "A team member contributing to the project's work and deliverables.", Order = 4)]
    Member = 4,

    [Display(Name = "Task Assignee", Description = "Has tasks assigned on the project but is not a core team member.", Order = 5)]
    Assignee = 5,
}
