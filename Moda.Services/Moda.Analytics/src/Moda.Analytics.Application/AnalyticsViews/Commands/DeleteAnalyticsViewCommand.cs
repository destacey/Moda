using Ardalis.GuardClauses;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Commands;

public sealed record DeleteAnalyticsViewCommand(Guid Id) : ICommand;

public sealed class DeleteAnalyticsViewCommandValidator : CustomValidator<DeleteAnalyticsViewCommand>
{
    public DeleteAnalyticsViewCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}

internal sealed class DeleteAnalyticsViewCommandHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser,
    ILogger<DeleteAnalyticsViewCommandHandler> logger) : ICommandHandler<DeleteAnalyticsViewCommand>
{
    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<DeleteAnalyticsViewCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteAnalyticsViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var view = await _analyticsDbContext.AnalyticsViews
                .Include(v => v.AnalyticsViewManagers)
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (view is null)
                return Result.Failure("Analytics view not found.");

            var canManageResult = view.CanEmployeeManage(_currentUserEmployeeId);
            if (canManageResult.IsFailure)
                return Result.Failure("You do not have permission to delete this analytics view.");

            _analyticsDbContext.AnalyticsViews.Remove(view);
            await _analyticsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
