using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Work.Application.WorkProcesses.Validators;

namespace Moda.Work.Application.WorkProcesses.Commands;
public sealed record UpdateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess) : ICommand;

public sealed class UpdateExternalWorkProcessCommandValidator : CustomValidator<UpdateExternalWorkProcessCommand>
{
    public UpdateExternalWorkProcessCommandValidator()
    {
        RuleFor(c => c.ExternalWorkProcess)
            .NotNull()
            .SetValidator(new IExternalWorkProcessConfigurationValidator());
    }
}

internal sealed class UpdateExternalWorkProcessCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateExternalWorkProcessCommandHandler> logger) : ICommandHandler<UpdateExternalWorkProcessCommand>
{
    private const string AppRequestName = nameof(UpdateExternalWorkProcessCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<UpdateExternalWorkProcessCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateExternalWorkProcessCommand request, CancellationToken cancellationToken)
    {
        var workProcess = await _workDbContext.WorkProcesses.FirstOrDefaultAsync(wp => wp.ExternalId == request.ExternalWorkProcess.Id, cancellationToken);
        if (workProcess is null)
        {
            _logger.LogWarning("{AppRequestName}: work process {WorkProcessExternalId} not found.", AppRequestName, request.ExternalWorkProcess.Id);
            return Result.Failure($"Work process {request.ExternalWorkProcess.Id} not found.");
        }

        var updateResult = workProcess.Update(request.ExternalWorkProcess.Name, request.ExternalWorkProcess.Description, _dateTimeProvider.Now);
        if (updateResult.IsFailure)
        {
            _logger.LogError("{AppRequestName}: failed to update work process {WorkProcessId}. Error: {Error}", AppRequestName, workProcess.Id, updateResult.Error);
            return Result.Failure(updateResult.Error);
        }

        _workDbContext.WorkProcesses.Update(workProcess);
        await _workDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{AppRequestName}: updated work process {WorkProcessName}.", AppRequestName, workProcess.Name);

        return Result.Success();
    }
}
