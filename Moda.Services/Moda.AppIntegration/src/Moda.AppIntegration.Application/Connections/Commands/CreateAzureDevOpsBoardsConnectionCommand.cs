using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record CreateAzureDevOpsBoardsConnectionCommand(string Name, string? Description, string Organization, string PersonalAccessToken) : ICommand<Guid>;

public sealed class CreateAzureDevOpsBoardsConnectionCommandValidator : CustomValidator<CreateAzureDevOpsBoardsConnectionCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public CreateAzureDevOpsBoardsConnectionCommandValidator(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.Organization)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniqueOrganization).WithMessage("The organization for this connection already exists in an existing connection.");

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }

    public async Task<bool> BeUniqueOrganization(string organization, CancellationToken cancellationToken)
    {
        return await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .AllAsync(c => c.Configuration!.Organization != organization, cancellationToken);
    }
}

internal sealed class CreateAzureDevOpsBoardsConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreateAzureDevOpsBoardsConnectionCommandHandler> logger, IAzureDevOpsService azureDevOpsService) : ICommandHandler<CreateAzureDevOpsBoardsConnectionCommand, Guid>
{
    private const string AppRequestName = nameof(CreateAzureDevOpsBoardsConnectionCommandHandler);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext = appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<CreateAzureDevOpsBoardsConnectionCommandHandler> _logger = logger;
    private readonly IAzureDevOpsService _azureDevOpsService = azureDevOpsService;

    public async Task<Result<Guid>> Handle(CreateAzureDevOpsBoardsConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeProvider.Now;
            var config = new AzureDevOpsBoardsConnectionConfiguration(request.Organization, request.PersonalAccessToken);

            var systemIdResult = await _azureDevOpsService.GetSystemId(config.OrganizationUrl, config.PersonalAccessToken, cancellationToken);
            if (systemIdResult.IsFailure)
            {
                _logger.LogWarning("Unable to get system id for Azure DevOps Boards connection for organization {Organization}. {Error}", request.Organization, systemIdResult.Error);
            }
            var systemId = systemIdResult.IsSuccess ? systemIdResult.Value : null;

            var connection = AzureDevOpsBoardsConnection.Create(request.Name, request.Description, systemId, config, systemIdResult.IsSuccess, null, timestamp);

            await _appIntegrationDbContext.AzureDevOpsBoardsConnections.AddAsync(connection, cancellationToken);

            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(connection.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", AppRequestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {AppRequestName} {request}");
        }
    }
}

