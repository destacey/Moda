namespace Wayd.Common.Application.Identity.Users;

public sealed record CreateUserCommand
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public Guid? EmployeeId { get; set; }
    public string LoginProvider { get; set; } = null!;
    public string? Password { get; set; }
}

public sealed class CreateUserCommandValidator : CustomValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserService userService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.")
            .MustAsync(async (email, _) => !await userService.ExistsWithEmailAsync(email))
                .WithMessage((_, email) => string.Format("Email {0} is already registered.", email));

        RuleFor(u => u.PhoneNumber)
            .MustAsync(async (phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!))
                .WithMessage((_, phone) => string.Format("Phone number {0} is already registered.", phone))
                .Unless(u => string.IsNullOrWhiteSpace(u.PhoneNumber));

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(u => u.LoginProvider)
            .NotEmpty()
            .Must(lp => LoginProviders.All.Contains(lp))
                .WithMessage("Login provider must be one of: " + string.Join(", ", LoginProviders.All));

        RuleFor(u => u.Password)
            .NotEmpty()
                .WithMessage("Password is required for Wayd accounts.")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit.")
            .When(u => u.LoginProvider == LoginProviders.Wayd);

        RuleFor(u => u.Password)
            .Null()
                .WithMessage("Password must not be provided for non-Wayd accounts.")
            .When(u => u.LoginProvider != LoginProviders.Wayd);
    }
}
