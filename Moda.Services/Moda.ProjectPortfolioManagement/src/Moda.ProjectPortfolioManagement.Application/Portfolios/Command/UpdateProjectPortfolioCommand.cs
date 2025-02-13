using Moda.Common.Application.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

public sealed record UpdateProjectPortfolioCommand(Guid Id, string Name, string Description) : ICommand;

public sealed class UpdateProjectPortfolioCommandValidator : AbstractValidator<UpdateProjectPortfolioCommand>
{
    public UpdateProjectPortfolioCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateProjectPortfolioCommandHandler(
    IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext,
    ILogger<UpdateProjectPortfolioCommandHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateProjectPortfolioCommand>
{
    private const string AppRequestName = nameof(UpdateProjectPortfolioCommand);

    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<UpdateProjectPortfolioCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result> Handle(UpdateProjectPortfolioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var portfolio = await _projectPortfolioManagementDbContext.Portfolios
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (portfolio is null)
            {
                _logger.LogInformation("Project Portfolio {ProjectPortfolioId} not found.", request.Id);
                return Result.Failure("Project Portfolio not found.");
            }

            var updateResult = portfolio.UpdateDetails(
                request.Name,
                request.Description
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _projectPortfolioManagementDbContext.Entry(portfolio).ReloadAsync(cancellationToken);
                portfolio.ClearDomainEvents();

                _logger.LogError("Unable to update Project Portfolio {ProjectPortfolioId}.  Error message: {Error}", request.Id, updateResult.Error);
                return Result.Failure(updateResult.Error);
            }

            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project Portfolio {ProjectPortfolioId} updated.", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
