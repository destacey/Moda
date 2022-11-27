using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.Work.Application.WorkTypes.Commands;
public sealed record CreateWorkTypeCommand : ICommand<int>
{
    public CreateWorkTypeCommand(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }
}

public sealed class CreateWorkTypeCommandValidator : CustomValidator<CreateWorkTypeCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public CreateWorkTypeCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;
        
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueName).WithMessage("The work type already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkTypes
            .AllAsync(e => e.Name != name, cancellationToken);
    }
}

internal sealed class CreateWorkTypeCommandHandler : ICommandHandler<CreateWorkTypeCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateWorkTypeCommandHandler> _logger;

    public CreateWorkTypeCommandHandler(IWorkDbContext workDbContext, IDateTimeService dateTimeService, ILogger<CreateWorkTypeCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateWorkTypeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Instant timestamp = _dateTimeService.Now;

            var workType = WorkType.Create(request.Name, request.Description, timestamp);

            await _workDbContext.WorkTypes.AddAsync(workType, cancellationToken);

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(workType.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

