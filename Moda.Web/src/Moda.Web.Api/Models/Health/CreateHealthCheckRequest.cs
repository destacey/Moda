using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Enums;
using Moda.Health.Commands;

namespace Moda.Web.Api.Models.Health;

public class CreateHealthCheckRequest
{
    public Guid ObjectId { get; set; }
    public int ContextId { get; set; }
    public int StatusId { get; set; }
    public Instant Expiration { get; set; }
    public string? Note { get; set; }

    public CreateHealthCheckCommand ToCreateHealthCheckCommand()
    {
        return new CreateHealthCheckCommand(ObjectId, (SystemContext)ContextId, (HealthStatus)StatusId, Expiration, Note);
    }
}

public sealed class CreateHealthCheckRequestValidator : CustomValidator<CreateHealthCheckRequest>
{
    public CreateHealthCheckRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(h => h.ObjectId)
            .NotEmpty();

        RuleFor(h => (SystemContext)h.ContextId)
            .IsInEnum()
            .WithMessage("A valid health check context must be selected.");

        RuleFor(h => (HealthStatus)h.StatusId)
            .IsInEnum()
            .WithMessage("A valid health status must be selected.");

        RuleFor(h => h.Expiration)
            .NotEmpty()
            .GreaterThan(dateTimeProvider.Now)
            .WithMessage("The Expiration must be in the future.");

        RuleFor(h => h.Note)
            .MaximumLength(1024);
    }
}
