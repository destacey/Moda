using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.AppIntegration.Application.Connectors.Commands;
public sealed record CreateConnectorCommand : ICommand<Guid>
{
    public CreateConnectorCommand(string name, string? description, ConnectorType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name { get; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public ConnectorType Type { get; }
}

public sealed class CreateConnectorCommandValidator : CustomValidator<CreateConnectorCommand>
{
    public CreateConnectorCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class CreateConnectorCommandHandler : ICommandHandler<CreateConnectorCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateConnectorCommandHandler> _logger;

    public CreateConnectorCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<CreateConnectorCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateConnectorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeService.Now;
            Connector connector;

            switch (request.Type)
            {
                case ConnectorType.AzureDevOpsBoards:
                    connector = AzureDevOpsBoardsConnector.Create(request.Name, request.Description, timestamp);
                    break;
                default:
                    return Result.Failure<Guid>($"Connector type '{request.Type}' is not supported.");
            }

            await _appIntegrationDbContext.Connectors.AddAsync(connector, cancellationToken);

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

