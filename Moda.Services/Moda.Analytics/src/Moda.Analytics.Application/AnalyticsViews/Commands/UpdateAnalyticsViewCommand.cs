using System.Text.Json;
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
    Guid? OwnerId,
    bool IsActive) : ICommand<AnalyticsViewDetailsDto>;

public sealed class UpdateAnalyticsViewCommandValidator : CustomValidator<UpdateAnalyticsViewCommand>
{
    public UpdateAnalyticsViewCommandValidator()
    {
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

internal sealed class UpdateAnalyticsViewCommandHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser,
    ILogger<UpdateAnalyticsViewCommandHandler> logger) : ICommandHandler<UpdateAnalyticsViewCommand, AnalyticsViewDetailsDto>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<UpdateAnalyticsViewCommandHandler> _logger = logger;

    public async Task<Result<AnalyticsViewDetailsDto>> Handle(UpdateAnalyticsViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var view = await _analyticsDbContext.AnalyticsViews
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (view is null)
                return Result.Failure<AnalyticsViewDetailsDto>("Analytics view not found.");

            var currentUserId = _currentUser.GetUserId();
            if (view.Visibility == Visibility.Private && view.OwnerId != currentUserId)
                return Result.Failure<AnalyticsViewDetailsDto>("You do not have permission to update this analytics view.");

            var ownerId = request.OwnerId ?? view.OwnerId;

            var result = view.Update(
                request.Name,
                request.Description,
                request.Dataset,
                request.DefinitionJson,
                request.Visibility,
                ownerId,
                request.IsActive);

            if (result.IsFailure)
                return Result.Failure<AnalyticsViewDetailsDto>(result.Error);

            await _analyticsDbContext.SaveChangesAsync(cancellationToken);

            return view.Adapt<AnalyticsViewDetailsDto>();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<AnalyticsViewDetailsDto>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
