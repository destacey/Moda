using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Domain.Tests.Sut.Models.PlanningPoker;

public class EstimationScaleTests
{
    private static readonly (string Value, int Order)[] FibonacciValues =
    [
        ("0", 0), ("1", 1), ("2", 2), ("3", 3), ("5", 4), ("8", 5), ("13", 6), ("21", 7), ("?", 8)
    ];

    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = EstimationScale.Create("Fibonacci", "Standard Fibonacci", false, FibonacciValues);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Fibonacci");
        result.Value.Description.Should().Be("Standard Fibonacci");
        result.Value.IsPreset.Should().BeFalse();
        result.Value.Values.Should().HaveCount(9);
    }

    [Fact]
    public void CreatePreset_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = EstimationScale.CreatePreset("Fibonacci", "Standard Fibonacci", FibonacciValues);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPreset.Should().BeTrue();
    }

    [Fact]
    public void Create_LessThanTwoValues_ShouldReturnFailure()
    {
        // Arrange
        var values = new[] { ("1", 0) };

        // Act
        var result = EstimationScale.Create("Test", null, false, values);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least 2 values");
    }

    [Fact]
    public void Create_DuplicateOrders_ShouldReturnFailure()
    {
        // Arrange
        var values = new[] { ("1", 0), ("2", 0), ("3", 1) };

        // Act
        var result = EstimationScale.Create("Test", null, false, values);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unique order");
    }

    [Fact]
    public void Create_ValuesAreOrdered_ShouldReturnOrderedValues()
    {
        // Arrange
        var values = new[] { ("5", 2), ("1", 0), ("3", 1) };

        // Act
        var result = EstimationScale.Create("Test", null, false, values);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var orderedValues = result.Value.Values.ToList();
        orderedValues[0].Value.Should().Be("1");
        orderedValues[1].Value.Should().Be("3");
        orderedValues[2].Value.Should().Be("5");
    }

    [Fact]
    public void Update_NonPreset_ShouldReturnSuccess()
    {
        // Arrange
        var scale = EstimationScale.Create("Original", "Original desc", false, FibonacciValues).Value;

        // Act
        var result = scale.Update("Updated", "Updated desc");

        // Assert
        result.IsSuccess.Should().BeTrue();
        scale.Name.Should().Be("Updated");
        scale.Description.Should().Be("Updated desc");
    }

    [Fact]
    public void Update_Preset_ShouldReturnFailure()
    {
        // Arrange
        var scale = EstimationScale.CreatePreset("Fibonacci", null, FibonacciValues).Value;

        // Act
        var result = scale.Update("Changed", "Changed desc");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preset");
    }

    [Fact]
    public void SetValues_NonPreset_ShouldReplaceValues()
    {
        // Arrange
        var scale = EstimationScale.Create("Test", null, false, FibonacciValues).Value;
        var newValues = new[] { ("S", 0), ("M", 1), ("L", 2) };

        // Act
        var result = scale.SetValues(newValues);

        // Assert
        result.IsSuccess.Should().BeTrue();
        scale.Values.Should().HaveCount(3);
        scale.Values.First().Value.Should().Be("S");
    }

    [Fact]
    public void SetValues_Preset_ShouldReturnFailure()
    {
        // Arrange
        var scale = EstimationScale.CreatePreset("Fibonacci", null, FibonacciValues).Value;

        // Act
        var result = scale.SetValues([("1", 0), ("2", 1)]);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preset");
    }

    [Fact]
    public void SetValues_LessThanTwoValues_ShouldReturnFailure()
    {
        // Arrange
        var scale = EstimationScale.Create("Test", null, false, FibonacciValues).Value;

        // Act
        var result = scale.SetValues([("1", 0)]);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least 2 values");
    }
}
