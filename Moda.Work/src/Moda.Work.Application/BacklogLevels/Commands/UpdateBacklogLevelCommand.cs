using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Work.Application.BacklogLevels.Commands;
public sealed record UpdateBacklogLevelCommand : ICommand<int>
{
    public UpdateBacklogLevelCommand(int id, string name, string? description, int rank)
    {
        Id = id;
        Name = name;
        Description = description;
        Rank = rank;
    }

    public int Id { get; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }

    /// <summary>
    /// The rank of the backlog level. The higher the number, the higher the level.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; }
}

public sealed class UpdateBacklogLevelCommandValidator : CustomValidator<UpdateBacklogLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;
    
    public UpdateBacklogLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
        
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(async (cmd, backlogLevel, cancellationToken) => await BeUniqueName(cmd.Id, backlogLevel, cancellationToken)).WithMessage("The backlog level name already exists."); ;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(int id, string name, CancellationToken cancellationToken)
    {
        return await _workDbContext.BacklogLevels
            .Where(c => c.Id != id)
            .AllAsync(e => e.Name != name, cancellationToken);
    }
}

internal sealed class UpdateBacklogLevelCommandHandler : ICommandHandler<UpdateBacklogLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateBacklogLevelCommandHandler> _logger;

    public UpdateBacklogLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeService dateTimeService, ILogger<UpdateBacklogLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateBacklogLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var backlogLevel = await _workDbContext.BacklogLevels
                .FirstAsync(p => p.Id == request.Id, cancellationToken);
            if (backlogLevel is null)
                return Result.Failure<int>("Backlog Level not found.");

            var updateResult = backlogLevel.Update(request.Name, request.Description, request.Rank, _dateTimeService.Now);

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _workDbContext.Entry(backlogLevel).ReloadAsync(cancellationToken);
                backlogLevel.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

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

