using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.Work.Application.WorkStates.Commands;
public sealed record CreateWorkStateCommand : ICommand<int>
{
    public CreateWorkStateCommand(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>The name of the work state.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description { get; }
}

public sealed class CreateWorkStateCommandValidator : CustomValidator<CreateWorkStateCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public CreateWorkStateCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(64)
            .MustAsync(BeUniqueName).WithMessage("The work state already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkStates
            .AllAsync(e => e.Name != name, cancellationToken);
    }
}

internal sealed class CreateWorkStateCommandHandler : ICommandHandler<CreateWorkStateCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateWorkStateCommandHandler> _logger;

    public CreateWorkStateCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreateWorkStateCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateWorkStateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeProvider.Now;

            var workState = WorkState.Create(request.Name, request.Description, timestamp);

            await _workDbContext.WorkStates.AddAsync(workState, cancellationToken);

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

