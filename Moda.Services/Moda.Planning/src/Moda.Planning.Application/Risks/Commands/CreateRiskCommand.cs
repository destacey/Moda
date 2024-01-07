using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Commands;
public sealed record CreateRiskCommand(string Summary, string? Description, Guid TeamId, 
    RiskCategory Category, RiskGrade Impact, RiskGrade Likelihood, Guid? AssigneeId, 
    LocalDate? FollowUpDate, string? Response) : ICommand<int>;

public sealed class CreateRiskCommandValidator : CustomValidator<CreateRiskCommand>
{
    public CreateRiskCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.TeamId)
            .NotEmpty();

        RuleFor(r => r.Summary)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(r => r.Description)
            .MaximumLength(1024);

        RuleFor(r => r.Category)
            .IsInEnum()
            .WithMessage("A valid category must be selected.");

        RuleFor(r => r.Impact)
            .IsInEnum()
            .WithMessage("A valid impact must be selected.");

        RuleFor(r => r.Likelihood)
            .IsInEnum()
            .WithMessage("A valid likelihood must be selected.");

        RuleFor(r => r.Response)
            .MaximumLength(1024);
    }
}

internal sealed class CreateRiskCommandHandler : ICommandHandler<CreateRiskCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeProvider _dateTimeManager;
    private readonly ILogger<CreateRiskCommandHandler> _logger;
    private readonly ICurrentUser _currentUser;

    public CreateRiskCommandHandler(IPlanningDbContext planningDbContext, IDateTimeProvider dateTimeManager, ILogger<CreateRiskCommandHandler> logger, ICurrentUser currentUser)
    {
        _planningDbContext = planningDbContext;
        _dateTimeManager = dateTimeManager;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid? currentUserEmployeeId = _currentUser.GetEmployeeId();
            if (currentUserEmployeeId is null)
                return Result.Failure<int>("Unable to determine current user's employee Id.");

            var risk = Risk.Create(
                request.Summary,
                request.Description,
                request.TeamId,
                _dateTimeManager.Now,
                currentUserEmployeeId.Value,
                request.Category,
                request.Impact,
                request.Likelihood,
                request.AssigneeId,
                request.FollowUpDate,
                request.Response
                );

            await _planningDbContext.Risks.AddAsync(risk, cancellationToken);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(risk.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
