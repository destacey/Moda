using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement;

public sealed record UpdateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess, IEnumerable<IExternalWorkType> ExternalWorkTypes) : ICommand;

public sealed class UpdateExternalWorkProcessCommandValidator : CustomValidator<UpdateExternalWorkProcessCommand>
{
    public UpdateExternalWorkProcessCommandValidator()
    {
        RuleFor(c => c.ExternalWorkProcess)
            .NotNull()
            .SetValidator(new IExternalWorkProcessConfigurationValidator());
    }
}
