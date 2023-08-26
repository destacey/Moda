using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;
using Moda.Goals.Application.Objectives.Dtos;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementObjectiveListDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the objective.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public required SimpleNavigationDto Status { get; set; }

    public required NavigationDto ProgramIncrement { get; set; }

    public required PlanningTeamNavigationDto Team { get; set; }

    public double Progress { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>Gets or sets the start date.</summary>
    /// <value>The start date.</value>
    public LocalDate? StartDate { get; set; }

    /// <summary>Gets or sets the target date.</summary>
    /// <value>The target date.</value>
    public LocalDate? TargetDate { get; set; }

    /// <summary>Gets a value indicating whether this instance is stretch.</summary>
    /// <value><c>true</c> if this instance is stretch; otherwise, <c>false</c>.</value>
    public bool IsStretch { get; set; }

    public static ProgramIncrementObjectiveListDto Create(ProgramIncrementObjective piObjective, ObjectiveListDto objective, NavigationDto piNavigationDto)
    {
        return new ProgramIncrementObjectiveListDto()
        {
            Id = piObjective.Id,
            Key = piObjective.Key,
            Name = objective.Name,
            ProgramIncrement = piNavigationDto,
            Status = objective.Status,
            Progress = objective.Progress,
            Team = PlanningTeamNavigationDto.FromPlanningTeam(piObjective.Team),
            Type = SimpleNavigationDto.FromEnum(piObjective.Type),
            StartDate = objective.StartDate,
            TargetDate = objective.TargetDate,
            IsStretch = piObjective.IsStretch
        };
    }
}
