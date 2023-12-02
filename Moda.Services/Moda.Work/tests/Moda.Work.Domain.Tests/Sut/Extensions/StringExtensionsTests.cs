using Moda.Work.Domain.Extensions;

namespace Moda.Work.Domain.Tests.Sut.Extensions;
public class StringExtensionsTests
{
    [Theory]
    [InlineData("CS", true)]
    [InlineData("CORE", true)]
    [InlineData("T2T", true)]
    [InlineData("AWESOMEPROJECT111111", true)]
    [InlineData("AWESOMEPROJECT1111112", false)]
    [InlineData("123", false)]
    [InlineData("N", false)]
    [InlineData("TTt", false)]
    [InlineData("T.T", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidWorkspaceKeyFormat(string? key, bool expectedResult)
    {
        var result = StringExtensions.IsValidWorkspaceKeyFormat(key);

        result.Should().Be(expectedResult);
    }
}
