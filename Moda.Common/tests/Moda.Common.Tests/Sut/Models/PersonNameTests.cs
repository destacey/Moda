using Moda.Common.Models;

namespace Moda.Common.Tests.Sut.Models;

public class PersonNameTests
{
    [Theory]
    [InlineData("John", "Doe", "Smith", "Jr", "Dr.")]
    [InlineData("John", null, "Smith", null, null)]
    [InlineData("John", null, "Smith", "Jr", null)]
    [InlineData("John", null, "Smith", null, "Mr.")]
    public void New_Valid(string firstName, string? middleName, string lastName, string? suffix, string? title)
    {
        // Act
        var result = new PersonName(firstName, middleName, lastName, suffix, title);

        // Assert
        Assert.Equal(firstName, result.FirstName);
        Assert.Equal(middleName, result.MiddleName);
        Assert.Equal(lastName, result.LastName);
        Assert.Equal(suffix, result.Suffix);
        Assert.Equal(title, result.Title);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void New_InvalidFirstNameInput_ThrowsException(string firstName)
    {
        var expectedExceptionMessage = $"{Constants.IsNullOrWhiteSpaceExceptionMessage} (Parameter 'firstName')";

        var exception = Assert.Throws<ArgumentException>(() => new PersonName(firstName, null, "Test"));

        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void New_InvalidLastNameInput_ThrowsException(string lastName)
    {
        var expectedExceptionMessage = $"{Constants.IsNullOrWhiteSpaceExceptionMessage} (Parameter 'lastName')";

        var exception = Assert.Throws<ArgumentException>(() => new PersonName("John", null, lastName));

        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public void EqualityCheck()
    {
        var personName1 = new PersonName("John", null, "Smith");
        var personName2 = new PersonName("John", "", "Smith", "", "");
        var personName3 = new PersonName("John", "Doe", "Smith");

        Assert.Equal(personName1, personName2);
        Assert.NotEqual(personName1, personName3);
    }

    [Theory]
    [InlineData("John", "Doe", "Smith", "Jr", "Dr.", "John Smith")]
    [InlineData("John", null, "Smith", null, null, "John Smith")]
    public void DisplayName_IncludesFirstLastOnly(string firstName, string? middleName, string lastName, string? suffix, string? title, string expected)
    {
        // Arrange
        var personName = new PersonName(firstName, middleName, lastName, suffix, title);

        // ACT
        var result = personName.DisplayName;
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("John", "Doe", "Smith", "Jr", "Dr.", "Dr. John Doe Smith Jr")]
    [InlineData("John", null, "Smith", null, null, "John Smith")]
    [InlineData("John", null, "Smith", "Jr", null, "John Smith Jr")]
    public void FullName_IncludesFirstLastOnly(string firstName, string? middleName, string lastName, string? suffix, string? title, string expected)
    {
        // Arrange
        var personName = new PersonName(firstName, middleName, lastName, suffix, title);

        // ACT
        var result = personName.FullName;

        // Assert
        Assert.Equal(expected, result);
    }
}
