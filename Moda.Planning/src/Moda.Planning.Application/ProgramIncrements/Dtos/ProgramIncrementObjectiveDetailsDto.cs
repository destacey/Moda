using Moda.Common.Application.Dtos;
using Moda.Goals.Application.Objectives.Dtos;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementObjectiveDetailsDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; set; }

    /// <summary>
    /// The name of the objective.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the objective.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public required SimpleNavigationDto Status { get; set; }

    public required NavigationDto ProgramIncrement { get; set; }

    public required PlanningTeamNavigationDto Team { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>Gets or sets the start date.</summary>
    /// <value>The start date.</value>
    public LocalDate? StartDate { get; private set; }

    /// <summary>Gets or sets the target date.</summary>
    /// <value>The target date.</value>
    public LocalDate? TargetDate { get; set; }

    /// <summary>Gets the closed date.</summary>
    /// <value>The closed date.</value>
    public Instant? ClosedDate { get; private set; }

    /// <summary>Gets a value indicating whether this instance is stretch.</summary>
    /// <value><c>true</c> if this instance is stretch; otherwise, <c>false</c>.</value>
    public bool IsStretch { get; set; }

    public static ProgramIncrementObjectiveDetailsDto Create(ProgramIncrementObjective piObjective, ObjectiveDetailsDto objective, NavigationDto piNavigationDto)
    {
        return new ProgramIncrementObjectiveDetailsDto()
        {
            Id = piObjective.Id,
            LocalId = piObjective.LocalId,
            Name = objective.Name,
            Description = objective.Description,
            ProgramIncrement = piNavigationDto,
            Status = objective.Status,
            Team = PlanningTeamNavigationDto.FromPlanningTeam(piObjective.Team),
            Type = SimpleNavigationDto.FromEnum(piObjective.Type),
            StartDate = objective.StartDate,
            TargetDate = objective.TargetDate,
            ClosedDate = objective.ClosedDate,
            IsStretch = piObjective.IsStretch
        };
    }
}
