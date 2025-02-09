using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.StrategicThemes.Commands;

public sealed record CreateStrategicThemeCommand(string Name, string Description, StrategicThemeState State) : ICommand<ObjectIdAndKey>;

public sealed class CreateStrategicThemeCommandValidator : AbstractValidator<CreateStrategicThemeCommand>
{
    public CreateStrategicThemeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.State)
            .IsInEnum();
    }
}

internal sealed class CreateStrategicThemeCommandHandler(
    IStrategicManagementDbContext strategicManagementDbContext, 
    ILogger<CreateStrategicThemeCommandHandler> logger,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateStrategicThemeCommand, ObjectIdAndKey>
{
    private const string AppRequestName = nameof(CreateStrategicThemeCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<CreateStrategicThemeCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateStrategicThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var strategicTheme = StrategicTheme.Create(
                request.Name,
                request.Description,
                request.State,
                _dateTimeProvider.Now
                );

            await _strategicManagementDbContext.StrategicThemes.AddAsync(strategicTheme, cancellationToken);
            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategic Theme {StrategicThemeId} created with Key {StrategicThemeKey}.", strategicTheme.Id, strategicTheme.Key);

            return new ObjectIdAndKey(strategicTheme.Id, strategicTheme.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}

