using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement.Commands;

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
