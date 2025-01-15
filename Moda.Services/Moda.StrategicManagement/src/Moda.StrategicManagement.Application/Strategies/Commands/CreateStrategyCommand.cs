using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.Strategies.Commands;

public sealed record CreateStrategyCommand(string Name, string Description, StrategyStatus Status, LocalDate? Start, LocalDate? End) : ICommand<ObjectIdAndKey>;

public sealed class CreateStrategyCommandValidator : AbstractValidator<CreateStrategyCommand>
{
    public CreateStrategyCommandValidator()
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

internal sealed class CreateStrategyCommandHandler(IStrategicManagementDbContext strategicManagementDbContext, ILogger<CreateStrategyCommandHandler> logger) : ICommandHandler<CreateStrategyCommand, ObjectIdAndKey>
{
    private const string AppRequestName = nameof(CreateStrategyCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<CreateStrategyCommandHandler> _logger = logger;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateStrategyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var strategy = Strategy.Create(
                request.Name,
                request.Description,
                request.Status,
                request.Start,
                request.End
                );

            await _strategicManagementDbContext.Strategies.AddAsync(strategy, cancellationToken);
            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy {StrategyId} created with Key {StrategyKey}.", strategy.Id, strategy.Key);

            return new ObjectIdAndKey(strategy.Id, strategy.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}

