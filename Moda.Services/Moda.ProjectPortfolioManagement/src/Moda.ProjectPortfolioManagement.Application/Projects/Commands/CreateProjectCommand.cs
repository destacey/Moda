﻿using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Commands;

public sealed record CreateProjectCommand(string Name, string Description, int ExpenditureCategoryId, LocalDateRange? DateRange, Guid PortfolioId, Guid? ProgramId, List<Guid>? SponsorIds, List<Guid>? OwnerIds, List<Guid>? ManagerIds, List<Guid>? StrategicThemeIds) : ICommand<ObjectIdAndKey>;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.ExpenditureCategoryId)
            .GreaterThan(0);

        RuleFor(x => x.PortfolioId)
            .NotEmpty();

        RuleFor(x => x.ProgramId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ProgramId cannot be an empty GUID.");

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

internal sealed class CreateProjectCommandHandler(
    IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext,
    ILogger<CreateProjectCommandHandler> logger)
    : ICommandHandler<CreateProjectCommand, ObjectIdAndKey>
{
    private const string AppRequestName = nameof(CreateProjectCommand);

    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<CreateProjectCommandHandler> _logger = logger;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var portfolio = await _projectPortfolioManagementDbContext.Portfolios
                .Include(p => p.Programs)
                .FirstOrDefaultAsync(p => p.Id == request.PortfolioId, cancellationToken);
            if (portfolio == null) {
                _logger.LogInformation("Portfolio with Id {PortfolioId} not found.", request.PortfolioId);
                return Result.Failure<ObjectIdAndKey>("Portfolio not found.");
            }

            var roles = GetRoles(request);

            var strategicThemes = request.StrategicThemeIds?.Distinct().ToHashSet();

            var createResult = portfolio.CreateProject(
                request.Name,
                request.Description,
                request.ExpenditureCategoryId,
                request.DateRange,
                request.ProgramId,
                roles,
                strategicThemes
                );
            if (createResult.IsFailure)
            {
                _logger.LogInformation("Error creating project {ProjectName} for Portfolio {PortfolioId}.", request.Name, request.PortfolioId);
                return Result.Failure<ObjectIdAndKey>(createResult.Error);
            }

            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            var project = createResult.Value;

            _logger.LogInformation("Project {ProjectId} created with Key {ProjectKey}.", project.Id, project.Key);

            return Result.Success(new ObjectIdAndKey(project.Id, project.Key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }

    private static Dictionary<ProjectRole, HashSet<Guid>> GetRoles(CreateProjectCommand request)
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
}
