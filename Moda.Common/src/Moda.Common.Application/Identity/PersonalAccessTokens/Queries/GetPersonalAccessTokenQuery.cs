using Mapster;
using Moda.Common.Application.Identity.PersonalAccessTokens.Dtos;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Identity.PersonalAccessTokens.Queries;

/// <summary>
/// Query to get a single personal access token by ID.
/// </summary>
public sealed record GetPersonalAccessTokenQuery(Guid TokenId) : IQuery<Result<PersonalAccessTokenDto>>;

public sealed class GetPersonalAccessTokenQueryValidator : CustomValidator<GetPersonalAccessTokenQuery>
{
    public GetPersonalAccessTokenQueryValidator()
    {
        RuleFor(x => x.TokenId)
            .NotEmpty();
    }
}

internal sealed class GetPersonalAccessTokenQueryHandler(IModaDbContext dbContext, ICurrentUser currentUser) : IQueryHandler<GetPersonalAccessTokenQuery, Result<PersonalAccessTokenDto>>
{
    private readonly IModaDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<PersonalAccessTokenDto>> Handle(GetPersonalAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();

        var token = await _dbContext.PersonalAccessTokens
            .Where(t => t.Id == request.TokenId && t.UserId == userId)
            .ProjectToType<PersonalAccessTokenDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null)
        {
            return Result.Failure<PersonalAccessTokenDto>("Token not found or you do not have permission to view it.");
        }

        return Result.Success(token);
    }
}
