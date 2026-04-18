using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Requests.WorkManagement.Interfaces;
using Wayd.Common.Application.Validators;
using Wayd.Common.Domain.Models;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record CreateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess, IEnumerable<ICreateWorkProcessScheme> WorkProcessSchemes) : ICommand<IntegrationState<Guid>>;

public sealed class CreateExternalWorkProcessCommandValidator : CustomValidator<CreateExternalWorkProcessCommand>
{
    public CreateExternalWorkProcessCommandValidator()
    {
        RuleFor(c => c.ExternalWorkProcess)
            .NotNull()
            .SetValidator(new IExternalWorkProcessConfigurationValidator());

        // TODO: Add validation for WorkProcessSchemes
    }
}
