using Moda.Common.Domain.Models.Organizations;

namespace Moda.Common.Tests.Sut.Models;

public class TeamCodeTests
{
    [Theory]
    [InlineData("core", "CORE")]
    [InlineData("MP ", "MP")]
    [InlineData(" TESTTEAM ", "TESTTEAM")]
    [InlineData("GREATTEAMA", "GREATTEAMA")]
    [InlineData("TEAM1", "TEAM1")] // number
    [InlineData("1TEAM1", "1TEAM1")] // number
    public void New_Valid(string input, string expectedResult)
    {
        var result = new TeamCode(input);

        result.Value.Should().Be(expectedResult);
    }

    [Fact]
    public void New_EmptyInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input TeamCode was empty. (Parameter 'TeamCode')";

        var exception = Assert.Throws<ArgumentException>(() => new TeamCode(""));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_WhitespaceInput_ThrowsException()
    {
        var expectedExceptionMessage = $"Required input TeamCode was empty. (Parameter 'TeamCode')";

        var exception = Assert.Throws<ArgumentException>(() => new TeamCode(" "));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void New_NullInput_ThrowsException()
    {
        var expectedExceptionMessage = "Value cannot be null. (Parameter 'TeamCode')";

        var exception = Assert.Throws<ArgumentNullException>(() => new TeamCode(null!));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Theory]
    [InlineData("BEST TEAM")]  // space
    [InlineData("T")] // too short
    [InlineData("BESTTEAMTWO")] // too long
    [InlineData("TEAM*")] // special character
    public void New_InvalidFormat_ThrowsException(string input)
    {
        var expectedExceptionMessage = "The value submitted does not meet the required format. (Parameter 'TeamCode')";

        var exception = Assert.Throws<ArgumentException>(() => new TeamCode(input));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void EqualityCheck()
    {
        var code1 = new TeamCode("TEAMA");
        var code2 = new TeamCode("TEAMA");
        var code3 = new TeamCode("TEAMB");

        code1.Should().Be(code2);
        code1.Should().NotBe(code3);
    }

    [Fact]
    public void ImplicitOperatorString_IsTheValue()
    {
        string input = "TEAMA";
        string result = new TeamCode(input);

        result.Should().Be(input);
    }

    [Fact]
    public void ExplicitOperatorString_IsTheValue()
    {
        string input = "TEAMA";
        TeamCode result = (TeamCode)input;

        Assert.Equal(input, result);
    }
}