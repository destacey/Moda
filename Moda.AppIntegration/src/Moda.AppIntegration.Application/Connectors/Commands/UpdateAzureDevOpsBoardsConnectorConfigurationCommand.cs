using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connectors.Commands;
public sealed record UpdateAzureDevOpsBoardsConnectorConfigurationCommand : ICommand<Guid>
{
    public UpdateAzureDevOpsBoardsConnectorConfigurationCommand(Guid id, string? organization, string? personalAccessToken)
    {
        Id = id;
        Organization = organization;
        PersonalAccessToken = personalAccessToken;
    }

    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string? Organization { get; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string? PersonalAccessToken { get; }
}

public sealed class UpdateAzureDevOpsBoardsConnectorConfigurationCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectorConfigurationCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public UpdateAzureDevOpsBoardsConnectorConfigurationCommandValidator(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Organization)
            .MaximumLength(256)
            .MustAsync(async (cmd, organization, cancellationToken) => await BeUniqueOrganization(cmd.Id, organization, cancellationToken)).WithMessage("The organization for this connector already exists.");

        RuleFor(c => c.PersonalAccessToken)
            .MaximumLength(256);
    }

    public async Task<bool> BeUniqueOrganization(Guid id, string? organization, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organization))
            return true;
        
        var connectors = await _appIntegrationDbContext.AzureDevOpsBoardsConnectors
            .Where(c => c.Id != id && !string.IsNullOrWhiteSpace(c.ConfigurationString))
            .ToListAsync(cancellationToken);
        
        return connectors
            .Where(c => c.Configuration is not null && !string.IsNullOrWhiteSpace(c.Configuration.Organization))
            .All(c => c.Configuration!.Organization != organization);
    }
}

internal sealed class UpdateAzureDevOpsBoardsConnectorConfigurationCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsConnectorConfigurationCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateAzureDevOpsBoardsConnectorConfigurationCommandHandler> _logger;

    public UpdateAzureDevOpsBoardsConnectorConfigurationCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<UpdateAzureDevOpsBoardsConnectorConfigurationCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateAzureDevOpsBoardsConnectorConfigurationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connector = await _appIntegrationDbContext.AzureDevOpsBoardsConnectors
                .FirstAsync(c => c.Id == request.Id, cancellationToken);
            if (connector is null)
                return Result.Failure<Guid>("Azure DevOps Boards Connector not found.");
            
            var config = new AzureDevOpsBoardsConnectorConfiguration(request.Organization, request.PersonalAccessToken);
            
            var updateResult = connector.UpdateConfiguration(config, _dateTimeService.Now);
            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _appIntegrationDbContext.Entry(connector).ReloadAsync(cancellationToken);
                connector.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<Guid>(updateResult.Error);
            };

            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(connector.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

