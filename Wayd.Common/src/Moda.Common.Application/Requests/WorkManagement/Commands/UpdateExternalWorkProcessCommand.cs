using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Requests.WorkManagement.Interfaces;
using Wayd.Common.Application.Validators;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

/// <summary>
/// Updates an external work process configuration.
/// </summary>
/// <param name="ExternalWorkProcess"></param>
/// <param name="ExternalWorkTypes">Includes data for work types associated to the work process.</param>
/// <param name="WorkflowMappings">Includes data to map work types to their workflows.</param>
public sealed record UpdateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess, IEnumerable<IExternalWorkType> ExternalWorkTypes, IEnumerable<ICreateWorkProcessScheme> WorkflowMappings) : ICommand, ILongRunningRequest;

public sealed class UpdateExternalWorkProcessCommandValidator : CustomValidator<UpdateExternalWorkProcessCommand>
{
    public UpdateExternalWorkProcessCommandValidator()
    {
        RuleFor(c => c.ExternalWorkProcess)
            .NotNull()
            .SetValidator(new IExternalWorkProcessConfigurationValidator());

        RuleForEach(c => c.ExternalWorkTypes)
            .NotNull()
            .NotEmpty()
            .SetValidator(new IExternalWorkTypeValidator());

        // TODO: Add validation for WorkProcessSchemes
    }
}
