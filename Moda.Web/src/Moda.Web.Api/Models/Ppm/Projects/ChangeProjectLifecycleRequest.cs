using Moda.ProjectPortfolioManagement.Application.Projects.Commands;

namespace Moda.Web.Api.Models.Ppm.Projects;

public sealed record ChangeProjectLifecycleRequest
{
    public Guid LifecycleId { get; set; }
    public Dictionary<Guid, Guid> PhaseMapping { get; set; } = new();

    public ChangeProjectLifecycleCommand ToCommand(Guid projectId)
        => new(projectId, LifecycleId, PhaseMapping);
}

public sealed class ChangeProjectLifecycleRequestValidator : AbstractValidator<ChangeProjectLifecycleRequest>
{
    public ChangeProjectLifecycleRequestValidator()
    {
        RuleFor(x => x.LifecycleId)
            .NotEmpty();

        RuleFor(x => x.PhaseMapping)
            .NotNull()
            .NotEmpty();
    }
}
