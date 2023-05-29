﻿using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Commands;
public sealed record CreateRiskCommand(string Summary, string? Description, Guid TeamId, 
    RiskCategory Category, RiskGrade Impact, RiskGrade Likelihood, Guid? AssigneeId, 
    LocalDate? FollowUpDate, string? Response) : ICommand<Guid>;

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

internal sealed class CreateRiskCommandHandler : ICommandHandler<CreateRiskCommand, Guid>
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

    public async Task<Result<Guid>> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid? currentUserEmployeeId = _currentUser.GetEmployeeId();
            if (currentUserEmployeeId is null)
                return Result.Failure<Guid>("Unable to determine current user's employee Id.");

            var risk = Risk.Create(
                request.Summary,
                request.Description,
                request.TeamId,
                _dateTimeService.Now,
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

            return Result.Success(risk.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}