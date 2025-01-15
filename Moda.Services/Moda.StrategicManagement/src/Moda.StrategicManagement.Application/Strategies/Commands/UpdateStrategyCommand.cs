﻿using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.Strategies.Commands;

public sealed record UpdateStrategyCommand(Guid Id, string Name, string Description, StrategyStatus Status, LocalDate? Start, LocalDate? End) : ICommand;

public sealed class UpdateStrategyCommandValidator : AbstractValidator<UpdateStrategyCommand>
{
    public UpdateStrategyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Description)
            .MaximumLength(3000);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Start)
            .NotEmpty()
            .When(x => x.End.HasValue)
            .WithMessage("Start date must be set if End date is provided.");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .When(x => x.Start.HasValue && x.End.HasValue)
            .WithMessage("End date must be greater than Start date.");
    }
}

internal sealed class UpdateStrategyCommandHandler(IStrategicManagementDbContext strategicManagementDbContext, ILogger<UpdateStrategyCommandHandler> logger) : ICommandHandler<UpdateStrategyCommand>
{
    private const string AppRequestName = nameof(UpdateStrategyCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<UpdateStrategyCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateStrategyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var strategy = await _strategicManagementDbContext.Strategies
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (strategy is null)
            {
                _logger.LogInformation("Strategy {StrategyId} not found.", request.Id);
                return Result.Failure($"Strategy {request.Id} not found.");
            }

            var updateResult = strategy.Update(
                request.Name,
                request.Description,
                request.Status,
                request.Start,
                request.End
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _strategicManagementDbContext.Entry(strategy).ReloadAsync(cancellationToken);
                strategy.ClearDomainEvents();

                _logger.LogError("Unable to update Strategy {StrategyId}.  Error message: {Error}", request.Id, updateResult.Error);
                return Result.Failure(updateResult.Error);
            }

            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy {StrategyId} updated.", strategy.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}