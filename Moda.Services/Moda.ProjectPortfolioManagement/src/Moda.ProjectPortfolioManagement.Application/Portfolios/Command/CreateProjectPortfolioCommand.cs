using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Command;

public sealed record CreateProjectPortfolioCommand(string Name, string Description) : ICommand<ObjectIdAndKey>;

public sealed class CreateProjectPortfolioCommandValidator : AbstractValidator<CreateProjectPortfolioCommand>
{
    public CreateProjectPortfolioCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(1024);
    }
}

internal sealed class CreateProjectPortfolioCommandHandler(
    IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext,
    ILogger<CreateProjectPortfolioCommandHandler> logger,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateProjectPortfolioCommand, ObjectIdAndKey>
{
    private const string AppRequestName = nameof(CreateProjectPortfolioCommand);

    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    private readonly ILogger<CreateProjectPortfolioCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateProjectPortfolioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var portfolio = ProjectPortfolio.Create(
                request.Name,
                request.Description
                );

            await _projectPortfolioManagementDbContext.Portfolios.AddAsync(portfolio, cancellationToken);
            await _projectPortfolioManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project Portfolio {ProjectPortfolioId} created with Key {ProjectPortfolioKey}.", portfolio.Id, portfolio.Key);

            return new ObjectIdAndKey(portfolio.Id, portfolio.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
