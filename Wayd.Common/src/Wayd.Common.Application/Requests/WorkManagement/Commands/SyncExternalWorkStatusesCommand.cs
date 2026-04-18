using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

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