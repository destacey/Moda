using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record UpdateAzureDevOpsBoardsConnectionConfigurationCommand : ICommand<Guid>
{
    public UpdateAzureDevOpsBoardsConnectionConfigurationCommand(Guid id, string? organization, string? personalAccessToken)
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

public sealed class UpdateAzureDevOpsBoardsConnectionConfigurationCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectionConfigurationCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public UpdateAzureDevOpsBoardsConnectionConfigurationCommandValidator(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Organization)
            .MaximumLength(128)
            .MustAsync(async (cmd, organization, cancellationToken) => await BeUniqueOrganization(cmd.Id, organization, cancellationToken)).WithMessage("The organization for this connection already exists.");

        RuleFor(c => c.PersonalAccessToken)
            .MaximumLength(128);
    }

    public async Task<bool> BeUniqueOrganization(Guid id, string? organization, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organization))
            return true;

        var connections = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .Where(c => c.Id != id && !string.IsNullOrWhiteSpace(c.ConfigurationString))
            .ToListAsync(cancellationToken);

        return connections
            .Where(c => c.Configuration is not null && !string.IsNullOrWhiteSpace(c.Configuration.Organization))
            .All(c => c.Configuration!.Organization != organization);
    }
}

internal sealed class UpdateAzureDevOpsBoardsConnectionConfigurationCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsConnectionConfigurationCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateAzureDevOpsBoardsConnectionConfigurationCommandHandler> _logger;

    public UpdateAzureDevOpsBoardsConnectionConfigurationCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<UpdateAzureDevOpsBoardsConnectionConfigurationCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateAzureDevOpsBoardsConnectionConfigurationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
                .FirstAsync(c => c.Id == request.Id, cancellationToken);
            if (connection is null)
                return Result.Failure<Guid>("Azure DevOps Boards Connection not found.");

            var config = new AzureDevOpsBoardsConnectionConfiguration(request.Organization, request.PersonalAccessToken);

            var updateResult = connection.UpdateConfiguration(config, _dateTimeService.Now);
            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _appIntegrationDbContext.Entry(connection).ReloadAsync(cancellationToken);
                connection.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<Guid>(updateResult.Error);
            }

            await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(connection.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

