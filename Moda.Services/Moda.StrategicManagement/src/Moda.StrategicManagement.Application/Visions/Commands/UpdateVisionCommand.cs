using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.Visions.Commands;

public sealed record UpdateVisionCommand(Guid Id, string Description, VisionState State, LocalDate? Start, LocalDate? End) : ICommand;

public sealed class UpdateVisionCommandValidator : AbstractValidator<UpdateVisionCommand>
{
    public UpdateVisionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(3000);

        RuleFor(x => x.State)
            .IsInEnum();

        RuleFor(x => x.Start)
            .NotEmpty()
            .When(x => x.End.HasValue)
            .WithMessage("Start date must be set if End date is provided.");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .When(x => x.Start.HasValue && x.End.HasValue)
            .WithMessage("End date must be greater than Start date.");
    }
}

internal sealed class UpdateVisionCommandHandler(IStrategicManagementDbContext strategicManagementDbContext, ILogger<UpdateVisionCommandHandler> logger) : ICommandHandler<UpdateVisionCommand>
{
    private const string AppRequestName = nameof(UpdateVisionCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<UpdateVisionCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateVisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var vision = await _strategicManagementDbContext.Visions
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (vision is null)
            {
                _logger.LogInformation("Vision {VisionId} not found.", request.Id);
                return Result.Failure($"Vision {request.Id} not found.");
            }

            var updateResult = vision.Update(
                request.Description,
                request.State,
                request.Start,
                request.End
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _strategicManagementDbContext.Entry(vision).ReloadAsync(cancellationToken);
                vision.ClearDomainEvents();

                _logger.LogError("Unable to update Vision {VisionId}.  Error message: {Error}", request.Id, updateResult.Error);
                return Result.Failure(updateResult.Error);
            }

            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vision {VisionId} updated.", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
