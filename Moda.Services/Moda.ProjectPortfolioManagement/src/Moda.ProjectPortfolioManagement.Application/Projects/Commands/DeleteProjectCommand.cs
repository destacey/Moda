namespace Moda.ProjectPortfolioManagement.Application.Projects.Commands;

public sealed record DeleteProjectCommand(Guid Id) : ICommand;

public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal sealed class DeleteProjectCommandHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext, ILogger<DeleteProjectCommandHandler> logger) : ICommandHandler<DeleteProjectCommand>
{
    private const string AppRequestName = nameof(DeleteProjectCommand);
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<DeleteProjectCommandHandler> _logger = logger;
    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: rethink this approach
            var project = await _projectPortfolioManagementDbContext.Projects
                .Include(p => p.Portfolio)
                .Include(p => p.Program)
                .Include(p => p.ExpenditureCategory)
                .Include(p => p.Roles)
                .Include(p => p.StrategicThemeTags)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (project is null)
            {
                _logger.LogInformation("Project {ProjectId} not found.", request.Id);
                return Result.Failure("Project not found.");
            }

            var portfolio = project.Portfolio;

            var deleteResult = portfolio!.DeleteProject(project.Id);
            if (deleteResult.IsFailure)
            {
                _logger.LogInformation("Error deleting project {ProjectId}.", request.Id);
                return Result.Failure(deleteResult.Error);
            }

            //_projectPortfolioManagementDbContext.Projects.Remove(project);
            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {ProjectId} deleted. Key: {ProjectKey}, Name: {ProjectName}", project.Id, project.Key, project.Name);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}

