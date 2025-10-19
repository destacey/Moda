using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.Planning.Iterations;
public sealed record SyncAzureDevOpsIterationsCommand(string SystemId, List<IExternalIteration<AzdoIterationMetadata>> Iterations) : ICommand, ILongRunningRequest;

public sealed class SyncAzureDevOpsIterationsCommandValidator : CustomValidator<SyncAzureDevOpsIterationsCommand>
{
    public SyncAzureDevOpsIterationsCommandValidator()
    {
        RuleFor(c => c.SystemId)
            .NotEmpty();

        RuleForEach(c => c.Iterations)
            .NotNull()
            .SetValidator(new IExternalIterationValidator<AzdoIterationMetadata>());
    }
}
