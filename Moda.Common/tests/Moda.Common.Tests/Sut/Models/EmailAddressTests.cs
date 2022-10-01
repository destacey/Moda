using Moda.Common.Models;

namespace Moda.Common.Tests.Sut.Models;

public class EmailAddressTests
{
    [Theory]
    [InlineData("test@test.com", "test@test.com")]
    [InlineData("test.me@test.com", "test.me@test.com")]
    [InlineData(" test@test.com ", "test@test.com")]
    public void New_Valid(string input, string expected)
    {
        var result = new EmailAddress(input);

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void New_EmptyInput_ThrowsException()
    {
        var expectedExceptionMessage = $"The value cannot be an empty string. (Parameter 'EmailAddress')";

        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(""));

        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public void New_NullInput_ThrowsException()
    {
        var expectedExceptionMessage = "Value cannot be null. (Parameter 'EmailAddress')";

        var exception = Assert.Throws<ArgumentNullException>(() => new EmailAddress(null!));

        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("test.com")]
    [InlineData("test@com")]
    [InlineData("@test.com")]
    [InlineData("test@test@test.com")]
    [InlineData("test..test@test.com")]
    [InlineData("test.test@-test.com")]
    [InlineData("test.test@test..com")]
    [InlineData(" ")]
    public void New_InvalidFormat_ThrowsException(string input)
    {
        var expectedExceptionMessage = "The value submitted does not meet the required format. (Parameter 'EmailAddress')";

        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(input));

        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public void EqualityCheck()
    {
        var email1 = new EmailAddress("test@test.com");
        var email2 = new EmailAddress("test@test.com");
        var email3 = new EmailAddress("different@test.com");

        Assert.Equal(email1, email2);
        Assert.NotEqual(email1, email3);
    }

    [Fact]
    public void ToString_IsTheValue()
    {
        var result = new EmailAddress("test@test.com");

        Assert.Equal(result.ToString(), result.Value);
    }
}
