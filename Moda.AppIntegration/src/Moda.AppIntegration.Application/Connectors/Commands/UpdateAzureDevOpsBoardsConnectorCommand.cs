using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connectors.Commands;
public sealed record UpdateAzureDevOpsBoardsConnectorCommand : ICommand<Guid>
{
    public UpdateAzureDevOpsBoardsConnectorCommand(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name { get; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; }
}

public sealed class UpdateAzureDevOpsBoardsConnectorCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectorCommand>
{
    public UpdateAzureDevOpsBoardsConnectorCommandValidator()
    {        
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateAzureDevOpsBoardsConnectorCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsConnectorCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateAzureDevOpsBoardsConnectorCommandHandler> _logger;

    public UpdateAzureDevOpsBoardsConnectorCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<UpdateAzureDevOpsBoardsConnectorCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateAzureDevOpsBoardsConnectorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connector = await _appIntegrationDbContext.AzureDevOpsBoardsConnectors
                .FirstAsync(c => c.Id == request.Id, cancellationToken);
            if (connector is null)
                return Result.Failure<Guid>("Azure DevOps Boards Connector not found.");
            
            var updateResult = connector.Update(request.Name, request.Description, _dateTimeService.Now);
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

