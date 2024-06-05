using Microsoft.Extensions.Logging;
using Moda.Integrations.AzureDevOps.IntegrationTests.Models;
using Moda.Integrations.AzureDevOps.Services;
using Moq;

namespace Moda.Integrations.AzureDevOps.IntegrationTests.Sut.Services;

[Collection("OptionsCollection")]
public class ProcessServiceTests
{
    private readonly AzdoOrganizationOptions _azdoOrganizationOptions;
    private readonly ProcessServiceData _processServiceData;

    private readonly Mock<ILogger<ProcessService>> _mockLogger;

    public ProcessServiceTests(OptionsFixture fixture)
    {
        _azdoOrganizationOptions = fixture.AzdoOrganizationOptions;
        _processServiceData = fixture.ProcessServiceData;

        _mockLogger = new Mock<ILogger<ProcessService>>();
    }

    [Fact]
    public async Task GetProcesses_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var expectedCount = _processServiceData.ProcessListCount;
        var expectedLogMessage = $"{expectedCount} processes found.";

        var service = new ProcessService(
            _azdoOrganizationOptions.OrganizationUrl,
            _azdoOrganizationOptions.PersonalAccessToken,
            _azdoOrganizationOptions.ApiVersion,
            _mockLogger.Object);

        // Act
        var result = await service.GetProcesses(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        result.Value.Count.Should().Be(expectedCount);

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), expectedLogMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }

    [Fact]
    public async Task GetProcess_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var processId = _processServiceData.GetProcessId;
        var expectedLogMessage = $"Process {processId} found.";
        var expectedBacklogLevelsCount = _processServiceData.GetProcessBacklogLevelsCount;

        var service = new ProcessService(
            _azdoOrganizationOptions.OrganizationUrl,
            _azdoOrganizationOptions.PersonalAccessToken,
            _azdoOrganizationOptions.ApiVersion,
            _mockLogger.Object);

        // Act
        var result = await service.GetProcess(processId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), expectedLogMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        result.Value.WorkTypeLevels.Should().NotBeNull();
        result.Value.WorkTypeLevels.Should().NotBeEmpty();
        result.Value.WorkTypeLevels.Count().Should().Be(expectedBacklogLevelsCount);
    }

    [Fact]
    public async Task GetProcess_WithInValidProcessId_ReturnsFailure()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var expectedErrorMessage = "Not Found";
        var expectedLogMessage = $"Error getting process {processId} from Azure DevOps: Not Found.";

        var service = new ProcessService(
            _azdoOrganizationOptions.OrganizationUrl,
            _azdoOrganizationOptions.PersonalAccessToken,
            _azdoOrganizationOptions.ApiVersion,
            _mockLogger.Object);

        // Act
        var result = await service.GetProcess(processId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
        result.Error.Should().Be(expectedErrorMessage);

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), expectedLogMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }

    [Fact]
    public async Task GetProcess_WithInValidOranizationUrl_ReturnsFailure()
    {
        // Arrange
        var organizationUrl = "https://www.test12345678.com";
        var processId = _processServiceData.GetProcessId;
        var expectedErrorMessage = "Connection Error - A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (www.test12345678.com:443)";
        var expectedLogMessage = $"Error getting process {processId} from Azure DevOps: Connection Error - A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (www.test12345678.com:443).";

        var service = new ProcessService(
            organizationUrl,
            _azdoOrganizationOptions.PersonalAccessToken,
            _azdoOrganizationOptions.ApiVersion,
            _mockLogger.Object);

        // Act
        var result = await service.GetProcess(processId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
        result.Error.Should().Be(expectedErrorMessage);

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), expectedLogMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }

    [Fact]
    public async Task GetProcess_WithInValidPersonalAccessToken_ReturnsFailure()
    {
        // Arrange
        var personalAccessToken = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var processId = _processServiceData.GetProcessId;
        var expectedErrorMessage = "Unauthorized";
        var expectedLogMessage = $"Error getting process {processId} from Azure DevOps: Unauthorized.";

        var service = new ProcessService(
            _azdoOrganizationOptions.OrganizationUrl,
            personalAccessToken,
            _azdoOrganizationOptions.ApiVersion,
            _mockLogger.Object);

        // Act
        var result = await service.GetProcess(processId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
        result.Error.Should().Be(expectedErrorMessage);

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), expectedLogMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }
}
