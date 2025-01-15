using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.Visions.Commands;

public sealed record CreateVisionCommand(string Description, VisionState State, LocalDate? Start, LocalDate? End) : ICommand<ObjectIdAndKey>;

public sealed class CreateVisionCommandValidator : AbstractValidator<CreateVisionCommand>
{
    public CreateVisionCommandValidator()
    {
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

internal sealed class CreateVisionCommandHandler(IStrategicManagementDbContext strategicManagementDbContext, ILogger<CreateVisionCommandHandler> logger) : ICommandHandler<CreateVisionCommand, ObjectIdAndKey>
{
    private const string AppRequestName = nameof(CreateVisionCommand);

    private readonly IStrategicManagementDbContext _strategicManagementDbContext = strategicManagementDbContext;
    private readonly ILogger<CreateVisionCommandHandler> _logger = logger;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateVisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var vision = Vision.Create(
                request.Description,
                request.State,
                request.Start,
                request.End
                );

            await _strategicManagementDbContext.Visions.AddAsync(vision, cancellationToken);
            await _strategicManagementDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vision {VisionId} created with Key {VisionKey}.", vision.Id, vision.Key);

            return new ObjectIdAndKey(vision.Id, vision.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}

