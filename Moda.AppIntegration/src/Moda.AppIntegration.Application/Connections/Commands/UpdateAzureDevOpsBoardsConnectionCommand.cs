using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record UpdateAzureDevOpsBoardsConnectionCommand : ICommand<Guid>
{
    public UpdateAzureDevOpsBoardsConnectionCommand(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
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
}

public sealed class UpdateAzureDevOpsBoardsConnectionCommandValidator : CustomValidator<UpdateAzureDevOpsBoardsConnectionCommand>
{
    public UpdateAzureDevOpsBoardsConnectionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateAzureDevOpsBoardsConnectionCommandHandler : ICommandHandler<UpdateAzureDevOpsBoardsConnectionCommand, Guid>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateAzureDevOpsBoardsConnectionCommandHandler> _logger;

    public UpdateAzureDevOpsBoardsConnectionCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, IDateTimeService dateTimeService, ILogger<UpdateAzureDevOpsBoardsConnectionCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateAzureDevOpsBoardsConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
                .FirstAsync(c => c.Id == request.Id, cancellationToken);
            if (connection is null)
                return Result.Failure<Guid>("Azure DevOps Boards connection not found.");

            var updateResult = connection.Update(request.Name, request.Description, _dateTimeService.Now);
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

