using Moda.Links.Commands;

namespace Moda.Web.Api.Models.Links;

public sealed record CreateLinkRequest
{
    public Guid ObjectId { get; set; }
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;

    public CreateLinkCommand ToCreateLinkCommand()
    {
        return new CreateLinkCommand(ObjectId, Name, Url);
    }
}

public sealed class CreateLinkRequestValidator : CustomValidator<CreateLinkRequest>
{
    public CreateLinkRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(l => l.ObjectId)
            .NotEmpty();

        RuleFor(l => l.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(l => l.Url)
            .NotEmpty();
    }
}
