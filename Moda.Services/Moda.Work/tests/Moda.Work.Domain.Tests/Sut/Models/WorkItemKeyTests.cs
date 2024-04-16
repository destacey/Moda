using Moda.Common.Models;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Sut.Models;
public class WorkItemKeyTests
{
    [Theory]
    [InlineData("CORE-444")]
    [InlineData("MP-5")]
    [InlineData("TESTWORKSPACE-12347856")]
    [InlineData("GREATWORKSPACEA-4577553")]
    [InlineData("WORKSPACE123-453244")]
    public void New_FromString_Valid(string input)
    {
        var result = new WorkItemKey(input);

        result.Value.Should().Be(input);
    }

    [Theory]
    [InlineData("CORE", 444, "CORE-444")]
    [InlineData("MP", 5, "MP-5")]
    [InlineData("TESTWORKSPACE", 12347856, "TESTWORKSPACE-12347856")]
    [InlineData("GREATWORKSPACEA", 4577553, "GREATWORKSPACEA-4577553")]
    [InlineData("WORKSPACE123", 453244, "WORKSPACE123-453244")]
    public void New_FromWorkspaceKeyAndInt_Valid(string workspaceKey, int number, string expected)
    {
        var result = new WorkItemKey(new WorkspaceKey(workspaceKey), number);

        result.Value.Should().Be(expected);
    }

    [Fact]
    public void New_EmptyInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input WorkItemKey was empty. (Parameter 'WorkItemKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkItemKey(""));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_WhitespaceInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input WorkItemKey was empty. (Parameter 'WorkItemKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkItemKey(" "));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_NullInput_ThrowsException()
    {
        var expectedExceptionMessage = "Value cannot be null. (Parameter 'WorkItemKey')";

        var exception = Assert.Throws<ArgumentNullException>(() => new WorkItemKey(null!));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Theory]
    [InlineData("BEST WORKSPACE-123")]  // space in first block
    [InlineData("T-123")] // first block too short
    [InlineData("BESTWORKSPACETOOLONG1-123")] // first block too long
    [InlineData("WORKSPACE*-123")] // special character in first block
    [InlineData("1WORKSPACE-123")] // first block starts with number
    [InlineData("WORKSPACE-123A")] // letter in second block
    public void New_InvalidFormat_ThrowsException(string input)
    {
        var expectedExceptionMessage = "The value submitted does not meet the required format. (Parameter 'WorkItemKey')";

        var exception = Assert.Throws<ArgumentException>(() => new WorkItemKey(input));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void EqualityCheck()
    {
        var code1 = new WorkItemKey("WORKSPACEA-12");
        var code2 = new WorkItemKey("WORKSPACEA-12");
        var code3 = new WorkItemKey("WORKSPACEB-12");

        code1.Should().Be(code2);
        code1.Should().NotBe(code3);
    }

    [Fact]
    public void ImplicitOperatorString_IsTheValue()
    {
        string input = "WORKSPACEA-12";
        string result = new WorkItemKey(input);

        result.Should().Be(input);
    }

    [Fact]
    public void ExplicitOperatorString_IsTheValue()
    {
        string input = "WORKSPACEA-12";
        WorkItemKey result = (WorkItemKey)input;

        Assert.Equal(input, result);
    }
}
