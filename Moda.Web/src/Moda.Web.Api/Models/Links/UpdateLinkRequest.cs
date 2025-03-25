using Moda.Links.Commands;

namespace Moda.Web.Api.Models.Links;

public sealed record UpdateLinkRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;

    public UpdateLinkCommand ToUpdateLinkCommand()
    {
        return new UpdateLinkCommand(Id, Name, Url);
    }
}

public sealed class UpdateLinkRequestValidator : CustomValidator<UpdateLinkRequest>
{
    public UpdateLinkRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(l => l.Id)
            .NotEmpty();

        RuleFor(l => l.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(l => l.Url)
            .NotEmpty();
    }
}
