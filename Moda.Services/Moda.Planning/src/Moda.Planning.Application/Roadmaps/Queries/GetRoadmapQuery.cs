using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Moda.Common.Application.Dtos;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;

public sealed record GetRoadmapQuery : IQuery<RoadmapDetailsDto?>
{
    public GetRoadmapQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Roadmap>();
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetRoadmapQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapQuery, RoadmapDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<RoadmapDetailsDto?> Handle(GetRoadmapQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        var roadmap = await _planningDbContext.Roadmaps
            .Where(request.IdOrKeyFilter)
            .Where(r => r.Visibility == publicVisibility || r.RoadmapManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .ProjectToType<RoadmapDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (roadmap is null)
        {
            return null;
        }

        //// TODO: make this better
        //var parentRoadmap = await _planningDbContext.RoadmapLinks
        //    .Where(r => r.ChildId == roadmap.Id)
        //    .Where(r => r.Parent!.Visibility == publicVisibility || r.Parent.Managers.Any(m => m.ManagerId == _currentUserEmployeeId))
        //    .Select(r => NavigationDto.Create(r.ParentId, r.Parent!.Key, r.Parent!.Name))
        //    .FirstOrDefaultAsync(cancellationToken);

        //roadmap.Parent = parentRoadmap;

        //var roadmapChildLinks = await _planningDbContext.RoadmapLinks
        //    .Where(r => r.ParentId == roadmap.Id)
        //    .Where(r => r.Child!.Visibility == publicVisibility || r.Child.Managers.Any(m => m.ManagerId == _currentUserEmployeeId))
        //    .ProjectToType<RoadmapChildDto>()
        //    .ToListAsync(cancellationToken);

        //roadmap.Children = roadmapChildLinks;

        return roadmap;
    }
}
