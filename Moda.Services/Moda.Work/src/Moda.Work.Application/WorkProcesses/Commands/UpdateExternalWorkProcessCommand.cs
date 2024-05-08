using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Work.Application.WorkProcesses.Validators;

namespace Moda.Work.Application.WorkProcesses.Commands;
public sealed record UpdateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess, IEnumerable<int> WorkTypeIds) : ICommand;

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
    private readonly Instant _timestamp = dateTimeProvider.Now;
    private readonly ILogger<UpdateExternalWorkProcessCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateExternalWorkProcessCommand request, CancellationToken cancellationToken)
    {
        var workProcess = await _workDbContext.WorkProcesses
            .Include(wp => wp.Schemes)
            .FirstOrDefaultAsync(wp => wp.ExternalId == request.ExternalWorkProcess.Id, cancellationToken);
        if (workProcess is null)
        {
            _logger.LogWarning("{AppRequestName}: work process {WorkProcessExternalId} not found.", AppRequestName, request.ExternalWorkProcess.Id);
            return Result.Failure($"Work process {request.ExternalWorkProcess.Id} not found.");
        }

        var updateResult = workProcess.Update(request.ExternalWorkProcess.Name, request.ExternalWorkProcess.Description, _timestamp);
        if (updateResult.IsFailure)
        {
            _logger.LogError("{AppRequestName}: failed to update work process {WorkProcessId}. Error: {Error}", AppRequestName, workProcess.Id, updateResult.Error);
            return Result.Failure(updateResult.Error);
        }

        var syncWorkTypesResult = await SyncWorkTypes(workProcess, request.WorkTypeIds, cancellationToken);
        if (syncWorkTypesResult.IsFailure)
        {
            return syncWorkTypesResult;
        }


        _workDbContext.WorkProcesses.Update(workProcess);
        await _workDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{AppRequestName}: updated work process {WorkProcessName}.", AppRequestName, workProcess.Name);

        return Result.Success();
    }

    private async Task<Result> SyncWorkTypes(WorkProcess workProcess, IEnumerable<int> workTypeIds, CancellationToken cancellationToken)
    {
        var disableSchemes = workProcess.Schemes.Where(s => s.IsActive && !workTypeIds.Contains(s.WorkTypeId)).Select(s => s.WorkTypeId).ToArray();
        foreach (var scheme in disableSchemes)
        {
            var deactivateResult = workProcess.DisableWorkType(scheme, _timestamp);
            if (deactivateResult.IsFailure)
            {
                _logger.LogError("{AppRequestName}: failed to deactivate work type {WorkTypeId} for work process {WorkProcessId}. Error: {Error}", AppRequestName, scheme, workProcess.Id, deactivateResult.Error);
                return Result.Failure(deactivateResult.Error);
            }
        }

        var enableSchemes = workProcess.Schemes.Where(s => !s.IsActive && workTypeIds.Contains(s.WorkTypeId)).Select(s => s.WorkTypeId).ToArray();
        foreach (var scheme in enableSchemes)
        {
            var activateResult = workProcess.EnableWorkType(scheme, _timestamp);
            if (activateResult.IsFailure)
            {
                _logger.LogError("{AppRequestName}: failed to activate work type {WorkTypeId} for work process {WorkProcessId}. Error: {Error}", AppRequestName, scheme, workProcess.Id, activateResult.Error);
                return Result.Failure(activateResult.Error);
            }
        }

        var newSchemes = workTypeIds.Where(wt => !workProcess.Schemes.Any(s => s.WorkTypeId == wt)).ToArray();
        if (newSchemes.Length == 0)
            return Result.Success();

        var workTypes = await _workDbContext.WorkTypes.Where(wt => newSchemes.Contains(wt.Id)).ToArrayAsync(cancellationToken);
        foreach (var workType in workTypes)
        {
            var addResult = workProcess.AddWorkType(workType, _timestamp);
            if (addResult.IsFailure)
            {
                _logger.LogError("{AppRequestName}: failed to add work type {WorkTypeId} to work process {WorkProcessId}. Error: {Error}", AppRequestName, workType.Id, workProcess.Id, addResult.Error);
                return Result.Failure(addResult.Error);
            }
        }

        return Result.Success();
    }
}
