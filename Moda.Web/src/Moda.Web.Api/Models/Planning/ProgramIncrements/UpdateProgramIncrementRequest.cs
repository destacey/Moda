using Moda.Planning.Application.ProgramIncrements.Commands;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public sealed record UpdateProgramIncrementRequest
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public required string Name { get; set; }

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    /// <summary>Gets or sets the objectives locked.</summary>
    /// <value><c>true</c> if [objectives locked]; otherwise, <c>false</c>.</value>
    public bool ObjectivesLocked { get; set; }

    public UpdateProgramIncrementCommand ToUpdateProgramIncrementCommand()
    {
        return new UpdateProgramIncrementCommand(Id, Name, Description, new LocalDateRange(Start, End), ObjectivesLocked);
    }
}

public sealed class UpdateProgramIncrementRequestValidator : CustomValidator<UpdateProgramIncrementRequest>
{
    public UpdateProgramIncrementRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(t => t.Description)
            .MaximumLength(1024);

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
