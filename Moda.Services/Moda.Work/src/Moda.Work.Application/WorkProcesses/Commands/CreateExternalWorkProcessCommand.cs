using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Models;
using Moda.Work.Application.WorkProcesses.Validators;

namespace Moda.Work.Application.WorkProcesses.Commands;
public sealed record CreateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess, IEnumerable<IExternalWorkType> ExternalWorkTypes) : ICommand<IntegrationState<Guid>>;

public sealed class CreateExternalWorkProcessCommandValidator : CustomValidator<CreateExternalWorkProcessCommand>
{
    public CreateExternalWorkProcessCommandValidator()
    {
        RuleFor(c => c.ExternalWorkProcess)
            .NotNull()
            .SetValidator(new IExternalWorkProcessConfigurationValidator());
    }
}

internal sealed class CreateExternalWorkProcessCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreateExternalWorkProcessCommandHandler> logger) : ICommandHandler<CreateExternalWorkProcessCommand, IntegrationState<Guid>>
{
    private const string AppRequestName = nameof(CreateExternalWorkProcessCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<CreateExternalWorkProcessCommandHandler> _logger = logger;

    public async Task<Result<IntegrationState<Guid>>> Handle(CreateExternalWorkProcessCommand request, CancellationToken cancellationToken)
    {
        if (await _workDbContext.WorkProcesses.AnyAsync(wp => wp.ExternalId == request.ExternalWorkProcess.Id, cancellationToken))
        {
            _logger.LogWarning("{AppRequestName}: work process {WorkProcessName} already exists.", AppRequestName, request.ExternalWorkProcess.Name);
            return Result.Failure<IntegrationState<Guid>>($"Work process {request.ExternalWorkProcess.Name} already exists.");
        }

        var timestamp = _dateTimeProvider.Now;

        var workProcess = WorkProcess.CreateExternal(request.ExternalWorkProcess.Name, request.ExternalWorkProcess.Description, request.ExternalWorkProcess.Id, timestamp);

        var workTypes = await _workDbContext.WorkTypes.Where(wt => request.ExternalWorkTypes.Select(wt => wt.Name).Contains(wt.Name)).ToArrayAsync(cancellationToken);

        foreach (var externalWorkType in request.ExternalWorkTypes)
        {
            var workType = workTypes.FirstOrDefault(wt => wt.Name == externalWorkType.Name);
            if (workType is null)
            {
                _logger.LogError("{AppRequestName}: work type {WorkTypeName} not found.", AppRequestName, externalWorkType.Name);
                return Result.Failure<IntegrationState<Guid>>($"Work type {externalWorkType.Name} not found.");
            }
            workProcess.AddWorkType(workType.Id, externalWorkType.IsActive, timestamp);
        }

        _workDbContext.WorkProcesses.Add(workProcess);
        await _workDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{AppRequestName}: created work process {WorkProcessName}.", AppRequestName, workProcess.Name);

        return Result.Success(IntegrationState<Guid>.Create(workProcess.Id, workProcess.IsActive));
    }
}
