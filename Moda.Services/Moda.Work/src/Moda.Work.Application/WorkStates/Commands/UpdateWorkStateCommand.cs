using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Work.Application.WorkStates.Commands;
public sealed record UpdateWorkStateCommand : ICommand<int>
{
    public UpdateWorkStateCommand(int id, string? description)
    {
        Id = id;
        Description = description;
    }

    public int Id { get; }

    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description { get; }
}

public sealed class UpdateWorkStateCommandValidator : CustomValidator<UpdateWorkStateCommand>
{
    public UpdateWorkStateCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateWorkStateCommandHandler : ICommandHandler<UpdateWorkStateCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateWorkStateCommandHandler> _logger;

    public UpdateWorkStateCommandHandler(IWorkDbContext workDbContext, IDateTimeService dateTimeService, ILogger<UpdateWorkStateCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateWorkStateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workState = await _workDbContext.WorkStates
                .FirstAsync(p => p.Id == request.Id, cancellationToken);
            if (workState is null)
                return Result.Failure<int>("Work State not found.");

            var updateResult = workState.Update(request.Description, _dateTimeService.Now);

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _workDbContext.Entry(workState).ReloadAsync(cancellationToken);
                workState.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(workState.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

