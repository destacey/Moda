using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Links.Models;

namespace Moda.Links.Queries;
public sealed record GetLinksQuery(Guid ObjectId) : IQuery<IReadOnlyList<LinkDto>>;

internal sealed class GetLinksQueryHandler : IQueryHandler<GetLinksQuery, IReadOnlyList<LinkDto>>
{
    private readonly ILinksDbContext _linksDbContext;

    public GetLinksQueryHandler(ILinksDbContext linksDbContext)
    {
        _linksDbContext = linksDbContext;
    }

    public async Task<IReadOnlyList<LinkDto>> Handle(GetLinksQuery request, CancellationToken cancellationToken)
    {
        return await _linksDbContext.Links
            .Where(l => l.ObjectId == request.ObjectId)
            .ProjectToType<LinkDto>()
            .ToListAsync(cancellationToken);
    }
}
