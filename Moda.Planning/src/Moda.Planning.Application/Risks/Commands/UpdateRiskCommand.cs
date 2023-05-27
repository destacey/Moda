using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Commands;
public sealed record UpdateRiskCommand(Guid Id, string Summary, string? Description, Guid TeamId, 
    RiskStatus Status, RiskCategory Category, RiskGrade Impact, RiskGrade Likelihood, Guid? AssigneeId, 
    LocalDate? FollowUpDate, string? Response) : ICommand<int>;

public sealed class UpdateRiskCommandValidator : CustomValidator<UpdateRiskCommand>
{
    public UpdateRiskCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Summary)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.Status)
            .IsInEnum()
            .WithMessage("A valid status must be selected.");

        RuleFor(e => e.Category)
            .IsInEnum()
            .WithMessage("A valid category must be selected.");

        RuleFor(e => e.Impact)
            .IsInEnum()
            .WithMessage("A valid impact must be selected.");

        RuleFor(e => e.Likelihood)
            .IsInEnum()
            .WithMessage("A valid likelihood must be selected.");

        RuleFor(e => e.Response)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateRiskCommandHandler : ICommandHandler<UpdateRiskCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateRiskCommandHandler> _logger;

    public UpdateRiskCommandHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService, ILogger<UpdateRiskCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var risk = await _planningDbContext.Risks
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (risk is null)
                return Result.Failure<int>("Risk not found.");

            var updateResult = risk.Update(
                request.Summary,
                request.Description,
                request.TeamId,
                request.Status,
                request.Category,
                request.Impact,
                request.Likelihood,
                request.AssigneeId,
                request.FollowUpDate,
                request.Response,
                _dateTimeService.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _planningDbContext.Entry(risk).ReloadAsync(cancellationToken);
                risk.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

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
