using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Commands;

public sealed record UpdateProjectCommand(Guid Id, string Name, string Description, int ExpenditureCategoryId, LocalDateRange? DateRange, List<Guid>? SponsorIds, List<Guid>? OwnerIds, List<Guid>? ManagerIds, List<Guid>? StrategicThemeIds) : ICommand;

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.ExpenditureCategoryId)
            .GreaterThan(0);

        RuleFor(x => x.SponsorIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("SponsorIds cannot contain empty GUIDs.");

        RuleFor(x => x.OwnerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("OwnerIds cannot contain empty GUIDs.");

        RuleFor(x => x.ManagerIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("ManagerIds cannot contain empty GUIDs.");

        RuleFor(x => x.StrategicThemeIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("StrategicThemeIds cannot contain empty GUIDs.");
    }
}

internal sealed class UpdateProjectCommandHandler(
    IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext,
    ILogger<UpdateProjectCommandHandler> logger)
    : ICommandHandler<UpdateProjectCommand>
{
    private const string AppRequestName = nameof(UpdateProjectCommand);

    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<UpdateProjectCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectPortfolioManagementDbContext.Projects
                .Include(p => p.Roles)
                .Include(p => p.StrategicThemeTags)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (project is null)
            {
                _logger.LogInformation("Project {ProjectId} not found.", request.Id);
                return Result.Failure<ObjectIdAndKey>("Project not found.");
            }

            var updateResult = project.UpdateDetails(
                request.Name,
                request.Description,
                request.ExpenditureCategoryId
            );
            if (updateResult.IsFailure)
            {
                return await HandleDomainFailure(project, updateResult, cancellationToken);
            }

            var updateTimelineResult = project.UpdateTimeline(request.DateRange);
            if (updateTimelineResult.IsFailure)
            {
                return await HandleDomainFailure(project, updateTimelineResult, cancellationToken);
            }

            var roles = GetRoles(request);
            var updateRolesResult = project.UpdateRoles(roles);
            if (updateRolesResult.IsFailure)
            {
                return await HandleDomainFailure(project, updateRolesResult, cancellationToken);
            }

            var strategicThemes = request.StrategicThemeIds?.Distinct().ToHashSet() ?? [];
            var updateStrategicThemesResult = project.UpdateStrategicThemes(strategicThemes);
            if (updateStrategicThemesResult.IsFailure)
            {
                return await HandleDomainFailure(project, updateStrategicThemesResult, cancellationToken);
            }

            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {ProjectId} updated.", request.Id);

            return Result.Success(new ObjectIdAndKey(project.Id, project.Key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }

    private static Dictionary<ProjectRole, HashSet<Guid>> GetRoles(UpdateProjectCommand request)
    {
        Dictionary<ProjectRole, HashSet<Guid>> roles = [];

        if (request.SponsorIds != null && request.SponsorIds.Count != 0)
        {
            roles.Add(ProjectRole.Sponsor, [.. request.SponsorIds]);
        }
        if (request.OwnerIds != null && request.OwnerIds.Count != 0)
        {
            roles.Add(ProjectRole.Owner, [.. request.OwnerIds]);
        }
        if (request.ManagerIds != null && request.ManagerIds.Count != 0)
        {
            roles.Add(ProjectRole.Manager, [.. request.ManagerIds]);
        }

        return roles;
    }

    private async Task<Result> HandleDomainFailure(Project project, Result errorResult, CancellationToken cancellationToken)
    {
        // Reset the entity
        await _projectPortfolioManagementDbContext.Entry(project).ReloadAsync(cancellationToken);
        project.ClearDomainEvents();

        _logger.LogError("Unable to update Project {ProjectId}.  Error message: {Error}", project.Id, errorResult.Error);
        return Result.Failure(errorResult.Error);
    }
}
