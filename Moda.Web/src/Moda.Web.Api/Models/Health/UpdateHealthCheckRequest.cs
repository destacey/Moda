using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Enums;
using Moda.Health.Commands;

namespace Moda.Web.Api.Models.Health;

public class UpdateHealthCheckRequest
{
    public Guid Id { get; set; }
    public int ContextId { get; set; }
    public int StatusId { get; set; }
    public Instant Expiration { get; set; }
    public string? Note { get; set; }

    public UpdateHealthCheckCommand ToUpdateHealthCheckCommand()
    {
        return new UpdateHealthCheckCommand(Id, (HealthStatus)StatusId, Expiration, Note);
    }
}

public sealed class UpdateHealthCheckRequestValidator : CustomValidator<UpdateHealthCheckRequest>
{
    public UpdateHealthCheckRequestValidator(IDateTimeProvider dateTimeManager)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(h => h.Id)
            .NotEmpty();

        RuleFor(h => (HealthStatus)h.StatusId)
            .IsInEnum()
            .WithMessage("A valid health status must be selected.");

        RuleFor(h => h.Expiration)
            .NotEmpty()
            .GreaterThan(dateTimeManager.Now)
            .WithMessage("The Expiration must be in the future.");

        RuleFor(h => h.Note)
            .MaximumLength(1024);
    }
}
