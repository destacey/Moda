using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.Planning.Application.PokerSessions.Dtos;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Queries;

public sealed record GetPokerSessionQuery : IQuery<PokerSessionDetailsDto?>
{
    public GetPokerSessionQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<PokerSession>();
    }

    public Expression<Func<PokerSession, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetPokerSessionQueryHandler(IPlanningDbContext planningDbContext) : IQueryHandler<GetPokerSessionQuery, PokerSessionDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<PokerSessionDetailsDto?> Handle(GetPokerSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _planningDbContext.PokerSessions
            .Where(request.IdOrKeyFilter)
            .ProjectToType<PokerSessionDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (session is not null)
        {
            session.HideUnrevealedVotes();
        }

        return session;
    }
}
