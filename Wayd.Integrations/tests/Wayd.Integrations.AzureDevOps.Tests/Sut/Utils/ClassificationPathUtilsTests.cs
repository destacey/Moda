using Wayd.Integrations.AzureDevOps.Utils;

namespace Wayd.Integrations.AzureDevOps.Tests.Sut.Utils;

public class ClassificationPathUtilsTests
{
    [Theory]
    [InlineData("\\Wayd\\Iteration", "\\Wayd")]
    [InlineData("\\Wayd\\Area", "\\Wayd")]
    [InlineData("\\ProjectName\\Iteration", "\\ProjectName")]
    public void RemoveClassificationTypeFromPath_WithRootPath_ReturnsProjectOnly(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Wayd\\Iteration\\Team Wayd", "\\Wayd\\Team Wayd")]
    [InlineData("\\Wayd\\Iteration\\Wayd Integrations Team\\Test", "\\Wayd\\Wayd Integrations Team\\Test")]
    [InlineData("\\Wayd\\Area\\Product\\Feature", "\\Wayd\\Product\\Feature")]
    public void RemoveClassificationTypeFromPath_WithSingleLevelHierarchy_RemovesClassificationType(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Wayd\\Iteration\\Team Wayd\\2022\\22.3\\22.3.6", "\\Wayd\\Team Wayd\\2022\\22.3\\22.3.6")]
    [InlineData("\\Wayd\\Iteration\\Team Wayd\\2022", "\\Wayd\\Team Wayd\\2022")]
    [InlineData("\\Wayd\\Area\\Product\\Feature\\SubFeature\\Component", "\\Wayd\\Product\\Feature\\SubFeature\\Component")]
    public void RemoveClassificationTypeFromPath_WithDeepHierarchy_RemovesClassificationType(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Wayd\\Iteration\\Team Iteration Name", "\\Wayd\\Team Iteration Name")]
    [InlineData("\\Wayd\\Area\\Area Team\\Iteration Area", "\\Wayd\\Area Team\\Iteration Area")]
    public void RemoveClassificationTypeFromPath_WithClassificationTypeInHierarchyNames_OnlyRemovesSecondNode(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Wayd")]
    [InlineData("\\Wayd")]
    public void RemoveClassificationTypeFromPath_WithInvalidPath_ReturnsOriginalPath(string input)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void RemoveClassificationTypeFromPath_WithRealWorldIterationPaths_TransformsCorrectly()
    {
        // Arrange - Real paths from Azure DevOps
        var testCases = new Dictionary<string, string>
        {
            { "\\Wayd\\Iteration", "\\Wayd" },
            { "\\Wayd\\Iteration\\Wayd Integrations Team", "\\Wayd\\Wayd Integrations Team" },
            { "\\Wayd\\Iteration\\Wayd Integrations Team\\Test", "\\Wayd\\Wayd Integrations Team\\Test" },
            { "\\Wayd\\Iteration\\Team Wayd", "\\Wayd\\Team Wayd" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022", "\\Wayd\\Team Wayd\\2022" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.3", "\\Wayd\\Team Wayd\\2022\\22.3" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.3\\22.3.6", "\\Wayd\\Team Wayd\\2022\\22.3\\22.3.6" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.3\\22.3.7", "\\Wayd\\Team Wayd\\2022\\22.3\\22.3.7" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.4", "\\Wayd\\Team Wayd\\2022\\22.4" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.4\\22.4.1", "\\Wayd\\Team Wayd\\2022\\22.4\\22.4.1" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.4\\22.4.2", "\\Wayd\\Team Wayd\\2022\\22.4\\22.4.2" },
            { "\\Wayd\\Iteration\\Team Wayd\\2022\\22.4\\22.4.3", "\\Wayd\\Team Wayd\\2022\\22.4\\22.4.3" },
            { "\\Wayd\\Iteration\\Team Wayd\\2023", "\\Wayd\\Team Wayd\\2023" },
            { "\\Wayd\\Iteration\\Team Wayd\\2024", "\\Wayd\\Team Wayd\\2024" }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(testCase.Key);
            result.Should().Be(testCase.Value, $"because '{testCase.Key}' should transform to '{testCase.Value}'");
        }
    }
}
