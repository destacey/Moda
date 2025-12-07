using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Requests.WorkManagement.Interfaces;
using Moda.Common.Application.Validators;
using Moda.Common.Domain.Models;

namespace Moda.Common.Application.Requests.WorkManagement.Commands;
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
