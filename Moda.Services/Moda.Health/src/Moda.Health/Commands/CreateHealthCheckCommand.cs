using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Validation;
using Moda.Common.Domain.Enums;
using Moda.Health.Models;
using NodaTime;

namespace Moda.Health.Commands;
public sealed record CreateHealthCheckCommand(Guid ObjectId, SystemContext Context, HealthStatus Status, Instant Expiration, string? Note) : ICommand<Guid>;

public sealed class CreateHealthCheckCommandValidator : CustomValidator<CreateHealthCheckCommand>
{
    public CreateHealthCheckCommandValidator(IDateTimeProvider dateTimeManager)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(h => h.ObjectId)
            .NotEmpty();

        RuleFor(h => h.Context)
            .NotEmpty();

        RuleFor(h => h.Status)
            .NotEmpty();

        RuleFor(h => h.Expiration)
            .NotEmpty()
            .GreaterThan(dateTimeManager.Now)
            .WithMessage("The Expiration must be in the future.");

        RuleFor(h => h.Note)
            .MaximumLength(1024);
    }
}

internal sealed class CreateHealthCheckCommandHandler : ICommandHandler<CreateHealthCheckCommand, Guid>
{
    private readonly IHealthDbContext _healthDbContext;
    private readonly IDateTimeProvider _dateTimeManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateHealthCheckCommandHandler> _logger;

    public CreateHealthCheckCommandHandler(IHealthDbContext healthDbContext, IDateTimeProvider dateTimeManager, ICurrentUser currentUser, ILogger<CreateHealthCheckCommandHandler> logger)
    {
        _healthDbContext = healthDbContext;
        _dateTimeManager = dateTimeManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateHealthCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid? currentUserEmployeeId = _currentUser.GetEmployeeId();
            if (currentUserEmployeeId is null)
                return Result.Failure<Guid>("Unable to determine current user's employee Id.");

            var objectHealthChecks = await _healthDbContext.HealthChecks
                .Where(hr => hr.ObjectId == request.ObjectId)
                .ToListAsync(cancellationToken);

            var healthReport = new HealthReport(objectHealthChecks);

            var healthCheck = healthReport.AddHealthCheck(request.ObjectId, request.Context, request.Status, currentUserEmployeeId.Value, _dateTimeManager.Now, request.Expiration, request.Note);

            await _healthDbContext.HealthChecks.AddAsync(healthCheck, cancellationToken);

            await _healthDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(healthCheck.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
