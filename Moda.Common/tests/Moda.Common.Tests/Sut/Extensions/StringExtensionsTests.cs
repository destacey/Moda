using Moda.Common.Extensions;

namespace Moda.Common.Tests.Sut.Extensions;
public class StringExtensionsTests
{
    [Theory]
    [InlineData("test@test.com", true)]
    [InlineData("t.t@t.c", true)]
    [InlineData("t_t@t.c", true)]
    [InlineData("t@t", false)]
    [InlineData("t@t.", false)]
    [InlineData(".t@t.x", false)]
    [InlineData("@t.x", false)]
    [InlineData("t@", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmailAddressFormat(string emailAddress, bool expectedResult)
    {
        var result = StringExtensions.IsValidEmailAddressFormat(emailAddress);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("0000000000", true)]
    [InlineData("4053649191", true)]
    [InlineData("123456789", false)]
    [InlineData("405-333-4125", false)]
    [InlineData("A234567899", false)]
    [InlineData("44455566667", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidPhoneNumberFormat(string phoneNumber, bool expectedResult)
    {
        var result = StringExtensions.IsValidPhoneNumberFormat(phoneNumber);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("73072", true)]
    [InlineData("00001", true)]
    [InlineData("85010", true)]
    [InlineData("73071-0000", true)]
    [InlineData("73071-1004", true)]
    [InlineData("7307", false)]
    [InlineData("810", false)]
    [InlineData("0000-1002", false)]
    [InlineData("730711002", false)]
    [InlineData("73071-00005", false)]
    [InlineData("-73071-1004", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidZipCodeFormat(string zipCode, bool expectedResult)
    {
        var result = StringExtensions.IsValidZipCodeFormat(zipCode);
        Assert.Equal(expectedResult, result);
    }
}
