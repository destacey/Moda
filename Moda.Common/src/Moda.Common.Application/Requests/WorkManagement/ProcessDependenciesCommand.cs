namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record ProcessDependenciesCommand(string SystemId) : ICommand, ILongRunningRequest;

public sealed class ProcessDependenciesCommandValidator : CustomValidator<ProcessDependenciesCommand>
{
    public ProcessDependenciesCommandValidator()
    {
        RuleFor(c => c.SystemId)
            .NotEmpty()
            .MaximumLength(64);
    }
}
