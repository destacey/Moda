using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement.Commands;

public sealed record SyncExternalWorkStatusesCommand(IList<IExternalWorkStatus> WorkStatuses) : ICommand;

public sealed class SyncExternalWorkStatusesCommandValidator : CustomValidator<SyncExternalWorkStatusesCommand>
{
    public SyncExternalWorkStatusesCommandValidator()
    {
        RuleFor(c => c.WorkStatuses)
            .NotEmpty();

        RuleForEach(c => c.WorkStatuses)
            .NotNull()
            .SetValidator(new IExternalWorkStatusValidator());
    }
}