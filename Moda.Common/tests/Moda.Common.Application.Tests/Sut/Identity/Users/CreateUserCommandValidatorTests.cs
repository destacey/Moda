using FluentValidation.TestHelper;
using Moda.Common.Application.Identity;
using Moda.Common.Application.Identity.Users;

namespace Moda.Common.Application.Tests.Sut.Identity.Users;

public class CreateUserCommandValidatorTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly CreateUserCommandValidator _sut;

    public CreateUserCommandValidatorTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockUserService.Setup(x => x.ExistsWithEmailAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _mockUserService.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserService.Setup(x => x.ExistsWithPhoneNumberAsync(It.IsAny<string>(), null)).ReturnsAsync(false);

        _sut = new CreateUserCommandValidator(_mockUserService.Object);
    }

    private static CreateUserCommand CreateValidModaCommand() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        LoginProvider = LoginProviders.Moda,
        Password = "Password123!",
    };

    private static CreateUserCommand CreateValidEntraIdCommand() => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane.doe@example.com",
        LoginProvider = LoginProviders.MicrosoftEntraId,
        Password = null,
    };

    #region Valid Commands

    [Fact]
    public async Task Validate_ShouldPass_WhenModaCommandIsValid()
    {
        // Arrange
        var command = CreateValidModaCommand();

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenEntraIdCommandIsValid()
    {
        // Arrange
        var command = CreateValidEntraIdCommand();

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region LoginProvider Validation

    [Fact]
    public async Task Validate_ShouldFail_WhenLoginProviderIsEmpty()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.LoginProvider = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LoginProvider);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLoginProviderIsInvalid()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.LoginProvider = "InvalidProvider";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LoginProvider)
            .WithErrorMessage("Login provider must be one of: " + string.Join(", ", LoginProviders.All));
    }

    #endregion

    #region Password Validation

    [Fact]
    public async Task Validate_ShouldFail_WhenModaAccountHasNoPassword()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Password = null;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required for Moda accounts.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenModaAccountPasswordIsTooShort()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Password = "short";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPasswordHasNoUppercase()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Password = "password1";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPasswordHasNoLowercase()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Password = "PASSWORD1";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPasswordHasNoDigit()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Password = "Passwords";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNonModaAccountHasPassword()
    {
        // Arrange
        var command = CreateValidEntraIdCommand();
        command.Password = "ShouldNotBeHere123!";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must not be provided for non-Moda accounts.");
    }

    #endregion

    #region Email Validation

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Email = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.Email = "not-an-email";

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid Email Address.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = CreateValidModaCommand();
        _mockUserService.Setup(x => x.ExistsWithEmailAsync(command.Email, null)).ReturnsAsync(true);

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage($"Email {command.Email} is already registered.");
    }

    #endregion

    #region Name Validation

    [Fact]
    public async Task Validate_ShouldFail_WhenFirstNameIsEmpty()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.FirstName = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(101)]
    public async Task Validate_ShouldFail_WhenFirstNameExceedsMaxLength(int length)
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.FirstName = new string('A', length);

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLastNameIsEmpty()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.LastName = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    #endregion

    #region PhoneNumber Validation

    [Fact]
    public async Task Validate_ShouldPass_WhenPhoneNumberIsNull()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.PhoneNumber = null;

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPhoneNumberAlreadyExists()
    {
        // Arrange
        var command = CreateValidModaCommand();
        command.PhoneNumber = "555-1234";
        _mockUserService.Setup(x => x.ExistsWithPhoneNumberAsync("555-1234", null)).ReturnsAsync(true);

        // Act
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion
}
