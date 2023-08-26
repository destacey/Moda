﻿using Mapster;
using Moda.Organization.Application.Models;

namespace Moda.Organization.Application.TeamsOfTeams.Queries;
public sealed record GetTeamOfTeamsNavigationQuery : IQuery<TeamNavigationDto?>
{
    public GetTeamOfTeamsNavigationQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public GetTeamOfTeamsNavigationQuery(int teamKey)
    {
        TeamKey = teamKey;
    }

    public Guid? TeamId { get; }
    public int? TeamKey { get; }
}

internal sealed class GetTeamOfTeamsNavigationQueryHandler : IQueryHandler<GetTeamOfTeamsNavigationQuery, TeamNavigationDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<GetTeamOfTeamsNavigationQueryHandler> _logger;

    public GetTeamOfTeamsNavigationQueryHandler(IOrganizationDbContext organizationDbContext, ILogger<GetTeamOfTeamsNavigationQueryHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
    }

    public async Task<TeamNavigationDto?> Handle(GetTeamOfTeamsNavigationQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.TeamOfTeams.AsQueryable();

        if (request.TeamId.HasValue)
        {
            query = query.Where(e => e.Id == request.TeamId.Value);
        }
        else if (request.TeamKey.HasValue)
        {
            query = query.Where(e => e.Key == request.TeamKey.Value);
        }
        else
        {
            var requestName = request.GetType().Name;
            var exception = new InternalServerException("No team id or local id provided.");

            _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            throw exception;
        }

        return await query
            .ProjectToType<TeamNavigationDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
