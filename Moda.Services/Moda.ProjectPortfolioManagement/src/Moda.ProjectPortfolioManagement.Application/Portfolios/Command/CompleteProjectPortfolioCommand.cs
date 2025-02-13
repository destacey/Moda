namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

public sealed record CompleteProjectPortfolioCommand(Guid Id) : ICommand;

public sealed class CompleteProjectPortfolioCommandValidator : AbstractValidator<CompleteProjectPortfolioCommand>
{
    public CompleteProjectPortfolioCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}

internal sealed class CompleteProjectPortfolioCommandHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext, ILogger<CompleteProjectPortfolioCommandHandler> logger, IDateTimeProvider dateTimeProvider) : ICommandHandler<CompleteProjectPortfolioCommand>
{
    private const string AppRequestName = nameof(CompleteProjectPortfolioCommand);

    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<CompleteProjectPortfolioCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result> Handle(CompleteProjectPortfolioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var portfolio = await _projectPortfolioManagementDbContext.Portfolios
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
            if (portfolio is null)
            {
                _logger.LogInformation("Project Portfolio {ProjectPortfolioId} not found.", request.Id);
                return Result.Failure("Project Portfolio not found.");
            }

            var completeResult = portfolio.Complete(_dateTimeProvider.Today);
            if (completeResult.IsFailure)
            {
                // Reset the entity
                await _projectPortfolioManagementDbContext.Entry(portfolio).ReloadAsync(cancellationToken);
                portfolio.ClearDomainEvents();

                _logger.LogError("Unable to complete Project Portfolio {ProjectPortfolioId}.  Error message: {Error}", request.Id, completeResult.Error);
                return Result.Failure(completeResult.Error);
            }
            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project Portfolio {ProjectPortfolioId} completed.", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}