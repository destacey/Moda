﻿using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Models;
using Moda.Work.Application.WorkProcesses.Validators;

namespace Moda.Work.Application.WorkProcesses.Commands;
public sealed record CreateExternalWorkProcessCommand(IExternalWorkProcessConfiguration ExternalWorkProcess) : ICommand<IntegrationState<Guid>>;

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
    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<CreateExternalWorkProcessCommandHandler> _logger = logger;

    public async Task<Result<IntegrationState<Guid>>> Handle(CreateExternalWorkProcessCommand request, CancellationToken cancellationToken)
    {
        if (await _workDbContext.WorkProcesses.AnyAsync(wp => wp.ExternalId == request.ExternalWorkProcess.Id, cancellationToken))
        {
            _logger.LogWarning("{AppRequestName}: work process {WorkProcessName} already exists.", nameof(CreateExternalWorkProcessCommand), request.ExternalWorkProcess.Name);
            return Result.Failure<IntegrationState<Guid>>($"Work process {request.ExternalWorkProcess.Name} already exists.");
        }

        var timestamp = _dateTimeProvider.Now;

        var workProcess = WorkProcess.CreateExternal(request.ExternalWorkProcess.Name, request.ExternalWorkProcess.Description, request.ExternalWorkProcess.Id, timestamp);

        _workDbContext.WorkProcesses.Add(workProcess);
        await _workDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{AppRequestName}: created work process {WorkProcessName}.", nameof(CreateExternalWorkProcessCommand), workProcess.Name);

        return Result.Success(IntegrationState<Guid>.Create(workProcess.Id, workProcess.IsActive));
    }
}
