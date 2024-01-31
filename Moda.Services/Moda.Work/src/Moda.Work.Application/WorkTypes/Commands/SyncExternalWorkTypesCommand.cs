﻿using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Work.Application.Validators;

namespace Moda.Work.Application.WorkTypes.Commands;
public sealed record SyncExternalWorkTypesCommand(IList<IExternalWorkType> WorkTypes) : ICommand;

public sealed class SyncExternalWorkTypesCommandValidator : CustomValidator<SyncExternalWorkTypesCommand>
{
    public SyncExternalWorkTypesCommandValidator()
    {
        RuleFor(c => c.WorkTypes)
            .NotEmpty();

        RuleForEach(c => c.WorkTypes)
            .NotNull()
            .SetValidator(new IExternalWorkTypeValidator());
    }
}


public sealed class SyncExternalWorkTypesCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<SyncExternalWorkTypesCommandHandler> logger) : ICommandHandler<SyncExternalWorkTypesCommand>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<SyncExternalWorkTypesCommandHandler> _logger = logger;

    public async Task<Result> Handle(SyncExternalWorkTypesCommand request, CancellationToken cancellationToken)
    {
        var existingWorkTypeNames = new HashSet<string>(await _workDbContext.WorkTypes.Select(wt => wt.Name).ToListAsync(cancellationToken));

        var workTypesToCreate = request.WorkTypes
            .Where(e => !existingWorkTypeNames.Contains(e.Name))
            .Select(e => WorkType.Create(e.Name, e.Description, _dateTimeProvider.Now))
            .ToList();

        if (workTypesToCreate.Count != 0)
        {
            _workDbContext.WorkTypes.AddRange(workTypesToCreate);
            await _workDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{@Request}: created {WorkTypeCount} work types.", nameof(SyncExternalWorkTypesCommand), workTypesToCreate.Count);
        }
        else
        {
            _logger.LogInformation("{@Request}: no new work types found.", nameof(SyncExternalWorkTypesCommand));
        }

        // Work types are global and cannot be deleted or updated at this time

        // TODO: add backlog level

        // TODO: add the ability to disable work types if they are no longer used within moda and disabled externally

        return Result.Success();
    }
}
