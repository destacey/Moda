using Moda.Planning.Application.ProgramIncrements.Extensions;

namespace Moda.Planning.Application.Tests.Sut.ProgramIncrements.Extensions;
public class ObjectiveStatusExtensionTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    public void ToGoalObjectiveStatus(int enumValue, int expectedValue)
    {
        var currentEnum = (Moda.Planning.Domain.Enums.ObjectiveStatus)enumValue;

        // Act
        var result = currentEnum.ToGoalObjectiveStatus();

        // Assert
        result.Should().Be((Moda.Goals.Domain.Enums.ObjectiveStatus)expectedValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void ToGoalObjectiveStatus_ThrowsArgumentOutOfRangeException(int enumValue)
    {
        var currentEnum = (Moda.Planning.Domain.Enums.ObjectiveStatus)enumValue;

        // Act
        Action act = () => currentEnum.ToGoalObjectiveStatus();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
