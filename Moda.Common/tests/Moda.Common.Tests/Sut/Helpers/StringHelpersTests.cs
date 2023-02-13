using Moda.Common.Helpers;

namespace Moda.Common.Tests.Sut.Helpers;

public class StringHelpersTests
{

    [Theory]
    [InlineData("This is a test", "This", "is", "a", "test")]
    [InlineData("This is test", "This", "is", " ", "test")]
    [InlineData("", "", " ")]
    [InlineData("")]
    public void Concat_Valid(string expected, params string?[] words)
    {
        // Act
        var result = StringHelpers.Concat(words);

        // Arrange
        Assert.Equal(expected, result);
    }
}
