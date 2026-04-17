using Wayd.Common.Domain.Enums.Goals;
using Wayd.Planning.Application.PlanningIntervals.Extensions;

namespace Wayd.Planning.Application.Tests.Sut.PlanningIntervals.Extensions;

public class ObjectiveStatusExtensionTests
{
    [Theory]
    [InlineData(1, 1)] // NotStarted
    [InlineData(2, 2)] // InProgress
    [InlineData(3, 3)] // Completed
    [InlineData(4, 4)] // Canceled
    [InlineData(5, 5)] // Missed
    public void ToGoalObjectiveStatus(int enumValue, int expectedValue)
    {
        var currentEnum = (Wayd.Planning.Domain.Enums.ObjectiveStatus)enumValue;

        // Act
        var result = currentEnum.ToGoalObjectiveStatus();

        // Assert
        result.Should().Be((ObjectiveStatus)expectedValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void ToGoalObjectiveStatus_ThrowsArgumentOutOfRangeException(int enumValue)
    {
        var currentEnum = (Wayd.Planning.Domain.Enums.ObjectiveStatus)enumValue;

        // Act
        Action act = () => currentEnum.ToGoalObjectiveStatus();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
