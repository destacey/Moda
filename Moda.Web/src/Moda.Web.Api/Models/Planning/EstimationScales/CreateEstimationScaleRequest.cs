using Moda.Planning.Application.EstimationScales.Commands;

namespace Moda.Web.Api.Models.Planning.EstimationScales;

public class CreateEstimationScaleRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<ScaleValueRequest> Values { get; set; } = [];

    public CreateEstimationScaleCommand ToCreateEstimationScaleCommand()
    {
        return new CreateEstimationScaleCommand(
            Name,
            Description,
            Values.Select(v => new CreateEstimationScaleCommand.ScaleValue(v.Value, v.Order)).ToList());
    }
}

public class ScaleValueRequest
{
    public string Value { get; set; } = default!;
    public int Order { get; set; }
}

public sealed class CreateEstimationScaleRequestValidator : CustomValidator<CreateEstimationScaleRequest>
{
    public CreateEstimationScaleRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Name).NotEmpty().MaximumLength(128);
        RuleFor(r => r.Description).MaximumLength(1024);
        RuleFor(r => r.Values).NotEmpty().Must(v => v.Count >= 2).WithMessage("At least 2 values are required.");
    }
}
