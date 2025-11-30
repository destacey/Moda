using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Identity.PersonalAccessTokens;

/// <summary>
/// Command to revoke a personal access token.
/// Revoked tokens are kept in the database for audit purposes.
/// </summary>
public sealed record RevokePersonalAccessTokenCommand : ICommand
{
    public Guid TokenId { get; init; }
}

public sealed class RevokePersonalAccessTokenCommandValidator : CustomValidator<RevokePersonalAccessTokenCommand>
{
    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RevokePersonalAccessTokenCommandValidator(IModaDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.TokenId)
            .NotEmpty()
            .MustAsync(TokenExistsAndBelongsToUser)
                .WithMessage("Token not found or you do not have permission to revoke it.");
    }

    private async Task<bool> TokenExistsAndBelongsToUser(Guid tokenId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return await _dbContext.PersonalAccessTokens
            .AnyAsync(t => t.Id == tokenId && t.UserId == userId, cancellationToken);
    }
}

internal sealed class RevokePersonalAccessTokenCommandHandler : ICommandHandler<RevokePersonalAccessTokenCommand>
{
    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<RevokePersonalAccessTokenCommandHandler> _logger;

    public RevokePersonalAccessTokenCommandHandler(
        IModaDbContext dbContext,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        ILogger<RevokePersonalAccessTokenCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokePersonalAccessTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var userIdString = userId.ToString();

            var token = await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.Id == request.TokenId && t.UserId == userIdString, cancellationToken);

            if (token == null)
            {
                return Result.Failure("Token not found or you do not have permission to revoke it.");
            }

            var revokeResult = token.Revoke(userId, _dateTimeProvider.Now);
            if (revokeResult.IsFailure)
            {
                return revokeResult;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Personal access token revoked. UserId: {UserId}, TokenId: {TokenId}, TokenName: {TokenName}",
                userIdString, token.Id, token.Name);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure($"Failed to revoke personal access token: {ex.Message}");
        }
    }
}
