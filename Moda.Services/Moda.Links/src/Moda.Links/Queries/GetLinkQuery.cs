using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Links.Models;

namespace Moda.Links.Queries;
public sealed record GetLinkQuery(Guid LinkId) : IQuery<LinkDto?>;

internal sealed class GetLinkQueryHandler : IQueryHandler<GetLinkQuery, LinkDto?>
{
    private readonly ILinksDbContext _linksDbContext;

    public GetLinkQueryHandler(ILinksDbContext linksDbContext)
    {
        _linksDbContext = linksDbContext;
    }

    public async Task<LinkDto?> Handle(GetLinkQuery request, CancellationToken cancellationToken)
    {
        return await _linksDbContext.Links
            .ProjectToType<LinkDto>()
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, cancellationToken);
    }
}
