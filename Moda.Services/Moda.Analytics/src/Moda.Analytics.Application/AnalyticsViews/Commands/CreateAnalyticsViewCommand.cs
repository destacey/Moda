using System.Text.Json;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Commands;

public sealed record CreateAnalyticsViewCommand(
    string Name,
    string? Description,
    AnalyticsDataset Dataset,
    string DefinitionJson,
    Visibility Visibility,
    Guid? OwnerId,
    bool IsActive = true) : ICommand<Guid>;

public sealed class CreateAnalyticsViewCommandValidator : CustomValidator<CreateAnalyticsViewCommand>
{
    public CreateAnalyticsViewCommandValidator()
    {
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

        RuleFor(v => v.OwnerId)
            .Must(v => !v.HasValue || v.Value != Guid.Empty)
            .WithMessage("OwnerId must be a valid guid when provided.");
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
    ICurrentUser currentUser,
    ILogger<CreateAnalyticsViewCommandHandler> logger) : ICommandHandler<CreateAnalyticsViewCommand, Guid>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<CreateAnalyticsViewCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(CreateAnalyticsViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = request.OwnerId ?? _currentUser.GetUserId();

            var view = AnalyticsView.Create(
                request.Name,
                request.Description,
                request.Dataset,
                request.DefinitionJson,
                request.Visibility,
                ownerId,
                request.IsActive);

            await _analyticsDbContext.AnalyticsViews.AddAsync(view, cancellationToken);
            await _analyticsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(view.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
