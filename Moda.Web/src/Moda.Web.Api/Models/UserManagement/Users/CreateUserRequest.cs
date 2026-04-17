namespace Wayd.Web.Api.Models.UserManagement.Users;

public sealed record CreateUserRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public Guid? EmployeeId { get; set; }
    public string LoginProvider { get; set; } = null!;
    public string? Password { get; set; }

    public CreateUserCommand ToCreateUserCommand()
        => new()
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            EmployeeId = EmployeeId,
            LoginProvider = LoginProvider,
            Password = Password
        };
}
