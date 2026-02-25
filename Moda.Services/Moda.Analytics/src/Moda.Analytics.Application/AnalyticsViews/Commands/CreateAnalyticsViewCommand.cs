using System.Text.Json;
using Ardalis.GuardClauses;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Commands;

public sealed record CreateAnalyticsViewCommand(
    string Name,
    string? Description,
    AnalyticsDataset Dataset,
    string DefinitionJson,
    Visibility Visibility,
    List<Guid> ManagerIds,
    bool IsActive = true) : ICommand<Guid>;

public sealed class CreateAnalyticsViewCommandValidator : CustomValidator<CreateAnalyticsViewCommand>
{
    private readonly ICurrentUser _currentUser;

    public CreateAnalyticsViewCommandValidator(ICurrentUser currentUser)
    {
        _currentUser = currentUser;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(v => v.Description)
            .MaximumLength(2048);

        RuleFor(v => v.DefinitionJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("DefinitionJson must contain valid JSON.");

        RuleFor(v => v.ManagerIds)
            .NotEmpty()
            .Must(IncludeCurrentUser).WithMessage("The current user must be a manager of the Analytics View.");

        RuleForEach(v => v.ManagerIds)
            .NotEmpty();
    }

    private bool IncludeCurrentUser(IEnumerable<Guid> managerIds)
    {
        var employeeId = Guard.Against.NullOrEmpty(_currentUser.GetEmployeeId());
        return managerIds.Contains(employeeId);
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal sealed class CreateAnalyticsViewCommandHandler(
    IAnalyticsDbContext analyticsDbContext,
    ILogger<CreateAnalyticsViewCommandHandler> logger) : ICommandHandler<CreateAnalyticsViewCommand, Guid>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly ILogger<CreateAnalyticsViewCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(CreateAnalyticsViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = AnalyticsView.Create(
                request.Name,
                request.Description,
                request.Dataset,
                request.DefinitionJson,
                request.Visibility,
                request.ManagerIds,
                request.IsActive);

            if (result.IsFailure)
            {
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, result.Error);
                return Result.Failure<Guid>(result.Error);
            }

            await _analyticsDbContext.AnalyticsViews.AddAsync(result.Value, cancellationToken);
            await _analyticsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(result.Value.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
