using Moda.Work.Domain.Extensions;

namespace Moda.Work.Domain.Tests.Sut.Extensions;
public class StringExtensionsTests
{
    [Theory]
    [InlineData("CS-12", true)]
    [InlineData("CORE-3456", true)]
    [InlineData("T2T-8", true)]
    [InlineData("AWESOMEPROJECT111111-9999", true)]
    [InlineData("AWESOMEPROJECT1111112-9999", false)]
    [InlineData("CS-12A", false)]
    [InlineData("123-12", false)]
    [InlineData("N-34", false)]
    [InlineData("TTt-789", false)]
    [InlineData("T.T-789", false)]
    [InlineData("CS--12", false)]
    [InlineData("CS12", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidWorkItemKeyFormat(string? key, bool expectedResult)
    {
        var result = StringExtensions.IsValidWorkItemKeyFormat(key);

        result.Should().Be(expectedResult);
    }
}
