using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record SyncExternalWorkTypesCommand(IList<IExternalWorkType> WorkTypes, int DefaultWorkTypeLevelId) : ICommand;

public sealed class SyncExternalWorkTypesCommandValidator : CustomValidator<SyncExternalWorkTypesCommand>
{
    public SyncExternalWorkTypesCommandValidator()
    {
        RuleFor(c => c.WorkTypes)
            .NotEmpty();

        RuleForEach(c => c.WorkTypes)
            .NotNull()
            .SetValidator(new IExternalWorkTypeValidator());

        RuleFor(c => c.DefaultWorkTypeLevelId)
            .GreaterThan(0);
    }
}
