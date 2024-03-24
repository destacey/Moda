using Moda.Common.Models;

namespace Moda.Common.Tests.Sut.Models;
public class WorkspaceKeyTests
{
    [Theory]
    [InlineData("core", "CORE")]
    [InlineData("MP ", "MP")]
    [InlineData(" TESTWORKSPACE ", "TESTWORKSPACE")]
    [InlineData("GREATWORKSPACEA", "GREATWORKSPACEA")]
    [InlineData("WORKSPACE123", "WORKSPACE123")] // number
    public void New_Valid(string input, string expectedResult)
    {
        var result = new WorkspaceKey(input);

        result.Value.Should().Be(expectedResult);
    }

    [Fact]
    public void New_EmptyInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input WorkspaceKey was empty. (Parameter 'WorkspaceKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkspaceKey(""));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_WhitespaceInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input WorkspaceKey was empty. (Parameter 'WorkspaceKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkspaceKey(" "));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_NullInput_ThrowsException()
    {
        var expectedExceptionMessage = "Value cannot be null. (Parameter 'WorkspaceKey')";

        var exception = Assert.Throws<ArgumentNullException>(() => new WorkspaceKey(null!));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Theory]
    [InlineData("BEST WORKSPACE")]  // space
    [InlineData("T")] // too short
    [InlineData("BESTWORKSPACETOOLONG1")] // too long
    [InlineData("WORKSPACE*")] // special character
    [InlineData("1WORKSPACE")] // starts with number
    public void New_InvalidFormat_ThrowsException(string input)
    {
        var expectedExceptionMessage = "The value submitted does not meet the required format. (Parameter 'WorkspaceKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkspaceKey(input));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void EqualityCheck()
    {
        var code1 = new WorkspaceKey("WORKSPACEA");
        var code2 = new WorkspaceKey("WORKSPACEA");
        var code3 = new WorkspaceKey("WORKSPACEB");

        code1.Should().Be(code2);
        code1.Should().NotBe(code3);
    }

    [Fact]
    public void ImplicitOperatorString_IsTheValue()
    {
        string input = "WORKSPACEA";
        string result = new WorkspaceKey(input);

        result.Should().Be(input);
    }

    [Fact]
    public void ExplicitOperatorString_IsTheValue()
    {
        string input = "WORKSPACEA";
        WorkspaceKey result = (WorkspaceKey)input;

        Assert.Equal(input, result);
    }
}
