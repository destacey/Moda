using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Identity.PersonalAccessTokens;

/// <summary>
/// Command to permanently delete a personal access token.
/// Unlike revoke, this removes the token from the database entirely.
/// </summary>
public sealed record DeletePersonalAccessTokenCommand(Guid TokenId) : ICommand;

public sealed class DeletePersonalAccessTokenCommandValidator : CustomValidator<DeletePersonalAccessTokenCommand>
{
    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DeletePersonalAccessTokenCommandValidator(IModaDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.TokenId)
            .NotEmpty()
            .MustAsync(TokenExistsAndBelongsToUser)
                .WithMessage("Token not found or you do not have permission to delete it.");
    }

    private async Task<bool> TokenExistsAndBelongsToUser(Guid tokenId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return await _dbContext.PersonalAccessTokens
            .AnyAsync(t => t.Id == tokenId && t.UserId == userId, cancellationToken);
    }
}

internal sealed class DeletePersonalAccessTokenCommandHandler(
    IModaDbContext dbContext,
    ICurrentUser currentUser,
    ILogger<DeletePersonalAccessTokenCommandHandler> logger) : ICommandHandler<DeletePersonalAccessTokenCommand>
{
    private readonly IModaDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<DeletePersonalAccessTokenCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeletePersonalAccessTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUser.GetUserId().ToString();

            var token = await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.Id == request.TokenId && t.UserId == userIdString, cancellationToken);

            if (token == null)
            {
                return Result.Failure("Token not found or you do not have permission to delete it.");
            }

            _dbContext.PersonalAccessTokens.Remove(token);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Personal access token deleted. UserId: {UserId}, TokenId: {TokenId}, TokenName: {TokenName}",
                userIdString, token.Id, token.Name);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure($"Failed to delete personal access token: {ex.Message}");
        }
    }
}
