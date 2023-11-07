using CSharpFunctionalExtensions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Validation;
using Moda.Common.Domain.Enums;
using Moda.Health.Dtos;
using NodaTime;

namespace Moda.Health.Commands;
public sealed record UpdateHealthCheckCommand(Guid Id, HealthStatus Status, Instant Expiration, string? Note) : ICommand<HealthCheckDto>;

public sealed class UpdateHealthCheckCommandValidator : CustomValidator<UpdateHealthCheckCommand>
{
    public UpdateHealthCheckCommandValidator(IDateTimeService dateTimeService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(h => h.Id)
            .NotEmpty();

        RuleFor(h => h.Status)
            .NotEmpty();

        RuleFor(h => h.Expiration)
            .NotEmpty()
            .GreaterThan(dateTimeService.Now)
            .WithMessage("The Expiration must be in the future.");

        RuleFor(h => h.Note)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateHealthCheckCommandHandler : ICommandHandler<UpdateHealthCheckCommand, HealthCheckDto>
{
    private readonly IHealthDbContext _healthDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateHealthCheckCommandHandler> _logger;

    public UpdateHealthCheckCommandHandler(IHealthDbContext healthDbContext, IDateTimeService dateTimeService, ILogger<UpdateHealthCheckCommandHandler> logger)
    {
        _healthDbContext = healthDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<HealthCheckDto>> Handle(UpdateHealthCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var healthCheck = await _healthDbContext.HealthChecks
                .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);
            if (healthCheck is null)
            {
                _logger.LogError("Unable to update health check. Health check with id {HealthCheckId} not found.", request.Id);
                return Result.Failure<HealthCheckDto>($"Health check with id {request.Id} not found.");
            }

            var updateResult = healthCheck.Update(request.Status, request.Expiration, request.Note, _dateTimeService.Now);
            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _healthDbContext.Entry(healthCheck).ReloadAsync(cancellationToken);
                healthCheck.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);

                return Result.Failure<HealthCheckDto>(updateResult.Error);
            }
            await _healthDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(healthCheck.Adapt<HealthCheckDto>());
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<HealthCheckDto>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
