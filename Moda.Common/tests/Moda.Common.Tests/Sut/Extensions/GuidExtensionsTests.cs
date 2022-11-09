namespace Moda.Common.Tests.Sut.Extensions;
public class GuidExtensionsTests
{
    [Theory]
    [MemberData(nameof(GuidIsDefaultData))]
    public void GuidIsDefault(Guid value, bool expectedResult)
    {
        Assert.Equal(expectedResult, value.IsDefault());
    }

    // TODO Guid not IXunitSerializable, only shows as one test
    public static IEnumerable<object[]> GuidIsDefaultData
    {
        get
        {
            yield return new object[] { default(Guid), true };
            yield return new object[] { Guid.Empty, true };
            yield return new object[] { Guid.NewGuid(), false };
            yield return new object[] { Guid.Parse("99c03283-33cb-4e56-a010-c2bc0758ad27"), false };
        }
    }


    [Theory]
    [MemberData(nameof(OptionalGuidIsDefaultData))]
    public void OptionalGuidIsDefault(Guid? value, bool expectedResult)
    {

        Assert.Equal(expectedResult, value.IsDefault());
    }

    // TODO Guid not IXunitSerializable, only shows as one test
    public static IEnumerable<object[]> OptionalGuidIsDefaultData
    {
        get
        {
            yield return new object[] { default(Guid), true };
            yield return new object[] { Guid.Empty, true };
            yield return new object[] { Guid.NewGuid(), false };
            yield return new object[] { Guid.Parse("99c03283-33cb-4e56-a010-c2bc0758ad27"), false };
            yield return new object[] { null!, false };
        }
    }
}
