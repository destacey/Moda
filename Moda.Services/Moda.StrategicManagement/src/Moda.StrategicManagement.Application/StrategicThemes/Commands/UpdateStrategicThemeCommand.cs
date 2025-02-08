using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.StrategicThemes.Commands;

public sealed record UpdateStrategicThemeCommand(Guid Id, string Name, string Description, StrategicThemeState State) : ICommand;

public sealed class UpdateStrategicThemeCommandValidator : AbstractValidator<UpdateStrategicThemeCommand>
{
    public UpdateStrategicThemeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.State)
            .IsInEnum();
    }
}

internal sealed class UpdateStrategicThemeCommandHandler(
    IStrategicManagementDbContext strategicManagementDbContext, 
    ILogger<UpdateStrategicThemeCommandHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateStrategicThemeCommand>
{
    private const string AppRequestName = nameof(UpdateStrategicThemeCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<UpdateStrategicThemeCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result> Handle(UpdateStrategicThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var strategicTheme = await _strategicManagementDbContext.StrategicThemes
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (strategicTheme is null)
            {
                _logger.LogInformation("Strategic Theme {StrategicThemeId} not found.", request.Id);
                return Result.Failure($"Strategic Theme {request.Id} not found.");
            }

            var updateResult = strategicTheme.Update(
                request.Name,
                request.Description,
                request.State,
                _dateTimeProvider.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _strategicManagementDbContext.Entry(strategicTheme).ReloadAsync(cancellationToken);
                strategicTheme.ClearDomainEvents();

                _logger.LogError("Unable to update Strategic Theme {StrategicThemeId}.  Error message: {Error}", request.Id, updateResult.Error);
                return Result.Failure(updateResult.Error);
            }

            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stratic Theme {StrategicThemeId} updated.", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
