using Mapster;
using Moda.Common.Application.Identity.PersonalAccessTokens.Dtos;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Identity.PersonalAccessTokens.Queries;

/// <summary>
/// Query to get all personal access tokens for the current user.
/// </summary>
public sealed record GetMyPersonalAccessTokensQuery : IQuery<Result<List<PersonalAccessTokenDto>>>;

internal sealed class GetMyPersonalAccessTokensQueryHandler(IModaDbContext dbContext, ICurrentUser currentUser) : IQueryHandler<GetMyPersonalAccessTokensQuery, Result<List<PersonalAccessTokenDto>>>
{
    private readonly IModaDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;

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
