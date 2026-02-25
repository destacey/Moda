using System.Text.Json;
using Ardalis.GuardClauses;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Commands;

public sealed record UpdateAnalyticsViewCommand(
    Guid Id,
    string Name,
    string? Description,
    AnalyticsDataset Dataset,
    string DefinitionJson,
    Visibility Visibility,
    List<Guid> ManagerIds,
    bool IsActive) : ICommand<AnalyticsViewDetailsDto>;

public sealed class UpdateAnalyticsViewCommandValidator : CustomValidator<UpdateAnalyticsViewCommand>
{
    private readonly ICurrentUser _currentUser;

    public UpdateAnalyticsViewCommandValidator(ICurrentUser currentUser)
    {
        _currentUser = currentUser;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(v => v.Id)
            .NotEmpty();

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

internal sealed class UpdateAnalyticsViewCommandHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser,
    ILogger<UpdateAnalyticsViewCommandHandler> logger) : ICommandHandler<UpdateAnalyticsViewCommand, AnalyticsViewDetailsDto>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateAnalyticsViewCommandHandler> _logger = logger;

    public async Task<Result<AnalyticsViewDetailsDto>> Handle(UpdateAnalyticsViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var view = await _analyticsDbContext.AnalyticsViews
                .Include(v => v.AnalyticsViewManagers)
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (view is null)
                return Result.Failure<AnalyticsViewDetailsDto>("Analytics view not found.");

            var updateResult = view.Update(
                request.Name,
                request.Description,
                request.Dataset,
                request.DefinitionJson,
                request.Visibility,
                request.ManagerIds,
                request.IsActive,
                _currentUserEmployeeId);

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _analyticsDbContext.Entry(view).ReloadAsync(cancellationToken);

                _logger.LogError("Unable to update Analytics View {AnalyticsViewId}.  Error message: {Error}", request.Id, updateResult.Error);
                return Result.Failure<AnalyticsViewDetailsDto>(updateResult.Error);
            }

            await _analyticsDbContext.SaveChangesAsync(cancellationToken);

            var entry = _analyticsDbContext.Entry(view);
            return new AnalyticsViewDetailsDto
            {
                Id = view.Id,
                Name = view.Name,
                Description = view.Description,
                Dataset = view.Dataset,
                DefinitionJson = view.DefinitionJson,
                Visibility = view.Visibility,
                ManagerIds = view.AnalyticsViewManagers.Select(m => m.ManagerId).ToList(),
                IsActive = view.IsActive,
                Created = entry.Property<Instant>("SystemCreated").CurrentValue,
                LastModified = entry.Property<Instant>("SystemLastModified").CurrentValue
            };
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<AnalyticsViewDetailsDto>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
