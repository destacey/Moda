using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Tests.Sut.Extensions;
public class EnumExtensionsTests
{
    [Theory]
    [InlineData(0, "Has Name And Description")]
    [InlineData(1, "Has Name")]
    [InlineData(2, "HasDescription")]  // falls back to the enumValue.ToString()
    [InlineData(3, "NoNameOrDescription")]  // falls back to the enumValue.ToString()
    public void GetDisplayName(int enumValue, string? expectedName)
    {
        var currentEnum = (EnumExtensionsTestsEnum)enumValue;

        // Act
        var result = currentEnum.GetDisplayName();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(0, "Test description")]
    [InlineData(1, null)]
    [InlineData(2, "Has description")]
    [InlineData(3, null)]
    public void GetDisplayDescription(int enumValue, string? expectedName)
    {
        var currentEnum = (EnumExtensionsTestsEnum)enumValue;

        // Act
        var result = currentEnum.GetDisplayDescription();

        // Assert
        Assert.Equal(expectedName, result);
    }
}

internal enum EnumExtensionsTestsEnum
{
    [Display(Name = "Has Name And Description", Description = "Test description")]
    HasNameAndDescription = 0,

    [Display(Name = "Has Name")]
    HasName = 1,

    [Display(Description = "Has description")]
    HasDescription = 2,

    NoNameOrDescription = 3
}
