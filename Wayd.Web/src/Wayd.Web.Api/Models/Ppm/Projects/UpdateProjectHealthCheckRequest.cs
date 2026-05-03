using Wayd.Common.Domain.Enums;

namespace Wayd.Web.Api.Models.Ppm.Projects;

public sealed record UpdateProjectHealthCheckRequest
{
    public HealthStatus Status { get; set; }
    public Instant Expiration { get; set; }
    public string? Note { get; set; }
}

public sealed class UpdateProjectHealthCheckRequestValidator
    : CustomValidator<UpdateProjectHealthCheckRequest>
{
    public UpdateProjectHealthCheckRequestValidator()
    {
        RuleFor(r => r.Status)
            .IsInEnum()
            .WithMessage("A valid health status must be selected.");

        RuleFor(r => r.Expiration)
            .NotEmpty();

        RuleFor(r => r.Note)
            .MaximumLength(1024);
    }
}
