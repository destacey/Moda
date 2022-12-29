using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Work.Application.BacklogLevels.Commands;
public sealed record CreateBacklogLevelCommand : ICommand<int>
{
    public CreateBacklogLevelCommand(string name, string? description, Ownership ownership, int order)
    {
        Name = name;
        Description = description;
        Ownership = ownership;
        Order = order;
    }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }

    /// <summary>
    /// Indicates whether the backlog level is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    /// <value>The ownership.</value>
    public Ownership Ownership { get; }

    /// <summary>
    /// The rank of the backlog level. The higher the number, the higher the level.
    /// </summary>
    /// <value>The rank.</value>
    public int Order { get; }
}

public sealed class CreateBacklogLevelCommandValidator : CustomValidator<CreateBacklogLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public CreateBacklogLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
        
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueName).WithMessage("The backlog level name already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return await _workDbContext.BacklogLevels
            .AllAsync(e => e.Name != name, cancellationToken);
    }
}

internal sealed class CreateBacklogLevelCommandHandler : ICommandHandler<CreateBacklogLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateBacklogLevelCommandHandler> _logger;

    public CreateBacklogLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeService dateTimeService, ILogger<CreateBacklogLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateBacklogLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeService.Now;

            var backlogLevel = PortfolioBacklogLevel.Create(request.Name, request.Description, request.Ownership, request.Order, timestamp);

            await _workDbContext.BacklogLevels.AddAsync(backlogLevel, cancellationToken);

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(backlogLevel.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

