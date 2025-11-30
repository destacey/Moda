using Mapster;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Identity.PersonalAccessTokens;

/// <summary>
/// Query to get all personal access tokens for the current user.
/// </summary>
public sealed record GetMyPersonalAccessTokensQuery : IQuery<Result<List<PersonalAccessTokenDto>>>;

internal sealed class GetMyPersonalAccessTokensQueryHandler : IQueryHandler<GetMyPersonalAccessTokensQuery, Result<List<PersonalAccessTokenDto>>>
{
    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetMyPersonalAccessTokensQueryHandler(IModaDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<List<PersonalAccessTokenDto>>> Handle(GetMyPersonalAccessTokensQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();

        var tokens = await _dbContext.PersonalAccessTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ExpiresAt)
            .ProjectToType<PersonalAccessTokenDto>()
            .ToListAsync(cancellationToken);

        return Result.Success(tokens);
    }
}

/// <summary>
/// DTO for personal access token information.
/// Note: The token value is NEVER included - it's only shown once at creation.
/// </summary>
public sealed record PersonalAccessTokenDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public Instant ExpiresAt { get; init; }
    public Instant? LastUsedAt { get; init; }
    public Instant? RevokedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsExpired { get; init; }
    public bool IsRevoked { get; init; }
    public string? Scopes { get; init; }
}
