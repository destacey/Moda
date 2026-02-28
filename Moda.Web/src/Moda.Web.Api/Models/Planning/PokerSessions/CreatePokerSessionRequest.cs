using Moda.Planning.Application.PokerSessions.Commands;

namespace Moda.Web.Api.Models.Planning.PokerSessions;

public class CreatePokerSessionRequest
{
    public string Name { get; set; } = default!;
    public int EstimationScaleId { get; set; }

    public CreatePokerSessionCommand ToCreatePokerSessionCommand()
    {
        return new CreatePokerSessionCommand(Name, EstimationScaleId);
    }
}

public sealed class CreatePokerSessionRequestValidator : CustomValidator<CreatePokerSessionRequest>
{
    public CreatePokerSessionRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Name).NotEmpty().MaximumLength(256);
        RuleFor(r => r.EstimationScaleId).GreaterThan(0);
    }
}
