﻿using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record CreateAzureDevOpsBoardsConnectionCommand : ICommand<Guid>
{
    public CreateAzureDevOpsBoardsConnectionCommand(string name, string? description, string organization, string personalAccessToken)
    {
        Name = name;
        Description = description;
        Organization = organization;
        PersonalAccessToken = personalAccessToken;
    }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name { get; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string Organization { get; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string PersonalAccessToken { get; }
}

public sealed class CreateAzureDevOpsBoardsConnectionCommandValidator : CustomValidator<CreateAzureDevOpsBoardsConnectionCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public CreateAzureDevOpsBoardsConnectionCommandValidator(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);

        RuleFor(c => c.Organization)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(async (organization, cancellationToken) => await BeUniqueOrganization(organization, cancellationToken)).WithMessage("The organization for this connection already exists.");

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }

    public async Task<bool> BeUniqueOrganization(string? organization, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organization))
            return true;

        var connections = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .Where(c => !string.IsNullOrWhiteSpace(c.ConfigurationString))
            .ToListAsync(cancellationToken);

        return connections
            .Where(c => c.Configuration is not null && !string.IsNullOrWhiteSpace(c.Configuration.Organization))
            .All(c => c.Configuration!.Organization != organization);
    }
}

internal sealed class CreateAzureDevOpsBoardsConnectionCommandHandler : ICommandHandler<CreateAzureDevOpsBoardsConnectionCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateAzureDevOpsBoardsConnectionCommandHandler> _logger;
    private readonly IAzureDevOpsService _azureDevOpsService;

    public CreateAzureDevOpsBoardsConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<CreateAzureDevOpsBoardsConnectionCommandHandler> logger, IAzureDevOpsService azureDevOpsService)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
    }

    public async Task<Result<Guid>> Handle(CreateAzureDevOpsBoardsConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeService.Now;
            var config = new AzureDevOpsBoardsConnectionConfiguration(request.Organization, request.PersonalAccessToken);

            var testConnectionResult = await _azureDevOpsService.TestConnection(config.OrganizationUrl, config.PersonalAccessToken);
                
            var connection = AzureDevOpsBoardsConnection.Create(request.Name, request.Description, config, testConnectionResult.IsSuccess, timestamp);

            await _appIntegrationDbContext.AzureDevOpsBoardsConnections.AddAsync(connection, cancellationToken);

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

