using Moda.Common.Application.Models;

namespace Moda.StrategicManagement.Application.StrategicThemes.Commands;

public sealed record DeleteStrategicThemeCommand(Guid Id) : ICommand;
public sealed class DeleteStrategicThemeCommandValidator : AbstractValidator<DeleteStrategicThemeCommand>
{
    public DeleteStrategicThemeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal sealed class DeleteStrategicThemeCommandHandler(IStrategicManagementDbContext strategicManagementDbContext, ILogger<DeleteStrategicThemeCommandHandler> logger) : ICommandHandler<DeleteStrategicThemeCommand>
{
    private const string AppRequestName = nameof(DeleteStrategicThemeCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<DeleteStrategicThemeCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteStrategicThemeCommand request, CancellationToken cancellationToken)
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

            _strategicManagementDbContext.StrategicThemes.Remove(strategicTheme);
            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategic Theme {StrategicThemeId} deleted.", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
