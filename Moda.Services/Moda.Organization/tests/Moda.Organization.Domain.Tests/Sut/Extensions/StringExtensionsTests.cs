using Moda.Organization.Domain.Extensions;

namespace Moda.Organization.Domain.Tests.Sut.Extensions;
public class StringExtensionsTests
{
    [Theory]
    [InlineData("CS", true)]
    [InlineData("CORE", true)]
    [InlineData("IAMAWESOME", true)]
    [InlineData("T2T", true)]
    [InlineData("123", true)]
    [InlineData("N", false)]
    [InlineData("AWESOMETEAM", false)]
    [InlineData("TTt", false)]
    [InlineData("T.T", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidOrganizationCodeFormat(string? code, bool expectedResult)
    {
        var result = StringExtensions.IsValidTeamCodeFormat(code);

        result.Should().Be(expectedResult);
    }
}
