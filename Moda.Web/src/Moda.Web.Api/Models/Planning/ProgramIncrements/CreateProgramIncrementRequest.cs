using Moda.Planning.Application.ProgramIncrements.Commands;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public sealed record CreateProgramIncrementRequest
{
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

    public CreateProgramIncrementCommand ToCreateProgramIncrementCommand()
    {
        return new CreateProgramIncrementCommand(Name, Description, new LocalDateRange(Start, End));
    }
}

public sealed class CreateProgramIncrementRequestValidator : CustomValidator<CreateProgramIncrementRequest>
{
    public CreateProgramIncrementRequestValidator()
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
