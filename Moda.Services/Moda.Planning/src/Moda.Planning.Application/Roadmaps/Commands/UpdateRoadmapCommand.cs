﻿using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapCommand(Guid Id, string Name, string? Description, LocalDateRange DateRange, List<Guid> RoadmapManagerIds, Visibility Visibility, string? color) : ICommand;

public sealed class UpdateRoadmapCommandValidator : AbstractValidator<UpdateRoadmapCommand>
{
    private readonly ICurrentUser _currentUser;

    public UpdateRoadmapCommandValidator(ICurrentUser currentUser)
    {
        _currentUser = currentUser;

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.DateRange)
            .NotNull();

        RuleFor(x => x.RoadmapManagerIds)
            .NotEmpty()
            .Must(IncludeCurrentUser).WithMessage("The current user must be a manager of the Roadmap.");

        RuleForEach(x => x.RoadmapManagerIds)
            .NotEmpty();

        RuleFor(x => x.Visibility)
            .IsInEnum();

        When(x => x.color != null, () => RuleFor(x => x.color)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color code."));
    }

    public bool IncludeCurrentUser(IEnumerable<Guid> roadmapManagerIds)
    {
        var employeeId = Guard.Against.NullOrEmpty(_currentUser.GetEmployeeId());
        return  roadmapManagerIds.Contains(employeeId);
    }
}

internal sealed class UpdateRoadmapCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapCommandHandler> logger) : ICommandHandler<UpdateRoadmapCommand>
{
    private const string AppRequestName = nameof(UpdateRoadmapCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.RoadmapManagers)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.Id} not found");

            var updateResult = roadmap.Update(
                request.Name,
                request.Description,
                request.DateRange,
                request.RoadmapManagerIds,
                request.Visibility,
                request.color,
                _currentUserEmployeeId
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _planningDbContext.Entry(roadmap).ReloadAsync(cancellationToken);
                roadmap.ClearDomainEvents();

                _logger.LogError("Failure for Request {CommandName} {@Request}.  Error message: {Error}", AppRequestName, request, updateResult.Error);
                return Result.Failure(updateResult.Error);
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}
