using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.TeamsOfTeams.Commands;
public sealed record AddTeamMembershipCommand : ICommand
{
    public AddTeamMembershipCommand(Guid teamId, Guid parentTeamId, MembershipDateRange dateRange)
    {
        TeamId = teamId;
        ParentTeamId = parentTeamId;
        DateRange = dateRange;
    }

    public Guid TeamId { get; }
    public Guid ParentTeamId { get; }
    public MembershipDateRange DateRange { get; }
}

public sealed class AddTeamMembershipCommandValidator : CustomValidator<AddTeamMembershipCommand>
{
    public AddTeamMembershipCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.ParentTeamId)
            .NotEmpty();

        RuleFor(t => t.DateRange)
            .NotNull();
    }
}

internal sealed class AddTeamMembershipCommandHandler : ICommandHandler<AddTeamMembershipCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AddTeamMembershipCommandHandler> _logger;

    public AddTeamMembershipCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<AddTeamMembershipCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTeamMembershipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await GetTeamWithAllChildMemberships(request.TeamId, cancellationToken);

            var parentTeam = await _organizationDbContext.TeamOfTeams
                .SingleAsync(t => t.Id == request.ParentTeamId);

            var result = team.AddTeamMembership(parentTeam, request.DateRange, _dateTimeService.Now);
            if (result.IsFailure)
                return result;

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }

    private async Task<TeamOfTeams> GetTeamWithAllChildMemberships(Guid teamId, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.InUtc().Date;
        var team = await _organizationDbContext.TeamOfTeams
            .Include(t => t.ParentMemberships)
            .Include(t => t.ChildMemberships.Where(m => m.DateRange != null && (today <= m.DateRange.Start || (!m.DateRange.End.HasValue && m.DateRange.Start <= today) || (m.DateRange.Start <= today && today <= m.DateRange.End))))
                .ThenInclude(m => m.Source)
            .SingleAsync(t => t.Id == teamId, cancellationToken);

        if (team.ChildMemberships.Any())
        {
            foreach (var childTeam in team.ChildMemberships.Where(m => m.IsActiveOn(today) || m.IsFutureOn(today)).Select(m => m.Source))
            {
                if (childTeam is TeamOfTeams childTeamOfTeams)
                {
                    await GetTeamWithAllChildMemberships(childTeamOfTeams.Id, cancellationToken);
                }
            }
        }

        return team;
    }
}