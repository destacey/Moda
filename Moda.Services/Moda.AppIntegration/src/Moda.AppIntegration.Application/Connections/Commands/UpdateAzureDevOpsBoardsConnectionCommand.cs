using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record UpdateAzureDevOpsBoardsConnectionCommand : ICommand<Guid>
{
    public UpdateAzureDevOpsBoardsConnectionCommand(Guid id, string name, string? description, string organization, string personalAccessToken)
    {
        Id = id;
        Name = name;
        Description = description;
        Organization = organization;
        PersonalAccessToken = personalAccessToken;
    }

    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public string Name { get; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description { get; }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string Organization { get; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string PersonalAccessToken { get; }
}

public sealed class UpdateAzureDevOpsBoardsConnectionCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectionCommand>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public UpdateAzureDevOpsBoardsConnectionCommandValidator(IAppIntegrationDbContext appIntegrationDbContext)
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
            .MustAsync(async (cmd, organization, cancellationToken) => await BeUniqueOrganization(cmd.Id, organization, cancellationToken)).WithMessage("The organization for this connection already exists in an existing connection.");

        RuleFor(c => c.PersonalAccessToken)
            .NotEmpty()
            .MaximumLength(128);
    }

    public async Task<bool> BeUniqueOrganization(Guid id, string organization, CancellationToken cancellationToken)
    {        
        return await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .Where(c => c.Id != id)
            .AllAsync(c => c.Configuration!.Organization != organization, cancellationToken);
    }
}

internal sealed class UpdateAzureDevOpsBoardsConnectionCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsConnectionCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeProvider _dateTimeManager;
    private readonly ILogger<UpdateAzureDevOpsBoardsConnectionCommandHandler> _logger;
    private readonly IAzureDevOpsService _azureDevOpsService;

    public UpdateAzureDevOpsBoardsConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeProvider dateTimeManager, ILogger<UpdateAzureDevOpsBoardsConnectionCommandHandler> logger, IAzureDevOpsService azureDevOpsService)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeManager = dateTimeManager;
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
    }

    public async Task<Result<Guid>> Handle(UpdateAzureDevOpsBoardsConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (connection is null)
                return Result.Failure<Guid>("Azure DevOps Boards connection not found.");

            // do the first four characters of the PersonalAccessToken match the existing one?
            var pat = connection.Configuration!.PersonalAccessToken.Length == request.PersonalAccessToken.Length 
                && connection.Configuration!.PersonalAccessToken?[..4] == request.PersonalAccessToken[..4]
                    ? connection.Configuration.PersonalAccessToken
                    : request.PersonalAccessToken;

            var testConfig = new AzureDevOpsBoardsConnectionConfiguration(request.Organization, pat);
            var testConnectionResult = await _azureDevOpsService.TestConnection(testConfig.OrganizationUrl, testConfig.PersonalAccessToken);

            var updateResult = connection.Update(request.Name, request.Description, request.Organization, pat, testConnectionResult.IsSuccess, _dateTimeManager.Now);
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

