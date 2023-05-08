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

        RuleFor(e => e.Summary)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.Category)
            .NotNull();

        RuleFor(e => e.Impact)
            .NotNull();

        RuleFor(e => e.Likelihood)
            .NotNull();
    }
}

internal sealed class CreateRiskCommandHandler : ICommandHandler<CreateRiskCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateRiskCommandHandler> _logger;
    private readonly ICurrentUser _currentUser;

    public CreateRiskCommandHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService, ILogger<CreateRiskCommandHandler> logger, ICurrentUser currentUser)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var risk = Risk.Create(
                request.Summary,
                request.Description,
                request.TeamId,
                _dateTimeService.Now,
                _currentUser.GetUserId(),
                request.Category,
                request.Impact,
                request.Likelihood,
                request.AssigneeId,
                request.FollowUpDate,
                request.Response
                );

            await _planningDbContext.Risks.AddAsync(risk, cancellationToken);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(risk.LocalId);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
