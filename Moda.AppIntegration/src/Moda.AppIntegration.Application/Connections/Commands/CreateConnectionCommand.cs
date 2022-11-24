using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record CreateConnectionCommand : ICommand<Guid>
{
    public CreateConnectionCommand(string name, string? description, Connector connector)
    {
        Name = name;
        Description = description;
        Connector = connector;
    }

    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name { get; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description { get; }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public Connector Connector { get; }
}

public sealed class CreateConnectionCommandValidator : CustomValidator<CreateConnectionCommand>
{
    public CreateConnectionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class CreateConnectionCommandHandler : ICommandHandler<CreateConnectionCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateConnectionCommandHandler> _logger;

    public CreateConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<CreateConnectionCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeService.Now;
            Connection connection;

            switch (request.Connector)
            {
                case Connector.AzureDevOpsBoards:
                    connection = AzureDevOpsBoardsConnection.Create(request.Name, request.Description, timestamp);
                    break;
                default:
                    return Result.Failure<Guid>($"Connector '{request.Connector}' is not supported.");
            }

            await _appIntegrationDbContext.Connections.AddAsync(connection, cancellationToken);

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

