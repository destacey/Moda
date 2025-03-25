namespace Moda.Web.Api.Models.UserManagement.Profiles;

public sealed record UpdateProfileRequest
{
    public string Id { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }

    public UpdateUserCommand ToUpdateUserCommand()
        => new(Id, FirstName, LastName, Email, PhoneNumber);
}

public sealed class UpdateProfileRequestValidator : CustomValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.");
    }
}