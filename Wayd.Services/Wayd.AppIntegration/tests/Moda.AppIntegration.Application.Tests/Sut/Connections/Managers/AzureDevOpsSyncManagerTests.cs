using CSharpFunctionalExtensions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Wayd.AppIntegration.Application.Connections.Dtos;
using Wayd.AppIntegration.Application.Connections.Dtos.AzureDevOps;
using Wayd.AppIntegration.Application.Connections.Managers;
using Wayd.AppIntegration.Application.Connections.Queries;
using Wayd.AppIntegration.Application.Connections.Queries.AzureDevOps;
using Wayd.AppIntegration.Application.Interfaces;
using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Enums;
using Wayd.Common.Application.Interfaces;
using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Application.Models;
using Wayd.Common.Application.Requests.Planning.Iterations;
using Wayd.Common.Application.Requests.WorkManagement.Commands;
using Wayd.Common.Application.Requests.WorkManagement.Interfaces;
using Wayd.Common.Application.Requests.WorkManagement.Queries;
using Wayd.Common.Domain.Enums.AppIntegrations;
using Moq;
using Moq.AutoMock;
using NodaTime;

namespace Wayd.AppIntegration.Application.Tests.Sut.Connections.Managers;

public class AzureDevOpsSyncManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly AzureDevOpsSyncManager _sut;

    public AzureDevOpsSyncManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<AzureDevOpsSyncManager>>(Mock.Of<ILogger<AzureDevOpsSyncManager>>());
        _sut = _mocker.CreateInstance<AzureDevOpsSyncManager>();
    }

    #region Helpers

    private static ConnectionListDto CreateConnectionDto(
        Guid? id = null,
        bool isValidConfiguration = true,
        bool isSyncEnabled = true,
        string? systemId = null)
    {
        return new ConnectionListDto
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = "Test Connection",
            SystemId = systemId ?? "test-system-id",
            Connector = new SimpleNavigationDto { Id = (int)Connector.AzureDevOps, Name = "Azure DevOps" },
            IsActive = true,
            IsValidConfiguration = isValidConfiguration,
            IsSyncEnabled = isSyncEnabled,
            CanSync = isValidConfiguration && isSyncEnabled
        };
    }

    private static AzureDevOpsConnectionDetailsDto CreateConnectionDetails(
        Guid connectionId,
        string? systemId = null,
        List<AzureDevOpsWorkProcessDto>? workProcesses = null,
        List<AzureDevOpsWorkspaceDto>? workspaces = null,
        List<AzureDevOpsWorkspaceTeamDto>? workspaceTeams = null)
    {
        return new AzureDevOpsConnectionDetailsDto
        {
            Id = connectionId,
            Name = "Test Connection",
            Connector = new SimpleNavigationDto { Id = (int)Connector.AzureDevOps, Name = "Azure DevOps" },
            IsActive = true,
            IsValidConfiguration = true,
            SystemId = systemId ?? "test-system-id",
            IsSyncEnabled = true,
            Configuration = new AzureDevOpsConnectionConfigurationDto
            {
                Organization = "TestOrg",
                PersonalAccessToken = "test-pat",
                OrganizationUrl = "https://dev.azure.com/TestOrg",
                WorkProcesses = workProcesses ?? [],
                Workspaces = workspaces ?? []
            },
            TeamConfiguration = new AzureDevOpsTeamConfigurationDto
            {
                WorkspaceTeams = workspaceTeams ?? []
            }
        };
    }

    private static AzureDevOpsWorkProcessDto CreateWorkProcess(
        Guid? externalId = null,
        Guid? internalId = null,
        bool isActive = true)
    {
        return new AzureDevOpsWorkProcessDto
        {
            ExternalId = externalId ?? Guid.CreateVersion7(),
            Name = "Agile",
            Description = "Agile work process",
            IntegrationState = isActive
                ? new IntegrationStateDto { InternalId = internalId ?? Guid.CreateVersion7(), IsActive = true }
                : null
        };
    }

    private static AzureDevOpsWorkspaceDto CreateWorkspace(
        Guid workProcessExternalId,
        Guid? externalId = null,
        Guid? internalId = null,
        string name = "TestProject",
        bool isActive = true)
    {
        return new AzureDevOpsWorkspaceDto
        {
            ExternalId = externalId ?? Guid.CreateVersion7(),
            Name = name,
            Description = "Test workspace",
            WorkProcessId = workProcessExternalId,
            IntegrationState = isActive
                ? new IntegrationStateDto { InternalId = internalId ?? Guid.CreateVersion7(), IsActive = true }
                : null
        };
    }

    /// <summary>
    /// Sets up the standard mock chain for a full sync through one connection with one work process and one workspace.
    /// Returns the IDs needed for assertions.
    /// </summary>
    private (Guid ConnectionId, Guid WorkProcessInternalId, Guid WorkspaceInternalId, Guid WorkProcessExternalId, Guid WorkspaceExternalId) SetupHappyPathMocks(SyncType syncType = SyncType.Differential)
    {
        var connectionId = Guid.CreateVersion7();
        var wpExternalId = Guid.CreateVersion7();
        var wpInternalId = Guid.CreateVersion7();
        var wsExternalId = Guid.CreateVersion7();
        var wsInternalId = Guid.CreateVersion7();
        var systemId = "test-system-id";

        var connectionDto = CreateConnectionDto(id: connectionId, systemId: systemId);
        var workProcess = CreateWorkProcess(externalId: wpExternalId, internalId: wpInternalId);
        var workspace = CreateWorkspace(wpExternalId, externalId: wsExternalId, internalId: wsInternalId);

        var connectionDetails = CreateConnectionDetails(
            connectionId,
            systemId: systemId,
            workProcesses: [workProcess],
            workspaces: [workspace]);

        // GetConnectionsQuery
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        // SyncOrganizationConfiguration
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connectionId, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        // GetAzureDevOpsConnectionQuery
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionDetails);

        // SyncWorkProcess - GetWorkProcess
        var mockWorkProcessConfig = new Mock<IExternalWorkProcessConfiguration>();
        mockWorkProcessConfig.Setup(p => p.Name).Returns("Agile");
        mockWorkProcessConfig.Setup(p => p.WorkTypes).Returns(new List<IExternalWorkTypeWorkflow>());
        mockWorkProcessConfig.Setup(p => p.WorkStatuses).Returns(new List<IExternalWorkStatus>());
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), wpExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mockWorkProcessConfig.Object));

        // GetWorkProcessSchemesQuery
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetWorkProcessSchemesQuery>(q => q.WorkProcessId == wpInternalId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IWorkProcessSchemeDto>().AsReadOnly() as IReadOnlyList<IWorkProcessSchemeDto>);

        // UpdateExternalWorkProcessCommand
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<UpdateExternalWorkProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // SyncWorkspace - GetWorkspace
        var mockWorkspaceConfig = new Mock<IExternalWorkspaceConfiguration>();
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkspace(It.IsAny<string>(), It.IsAny<string>(), wsExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mockWorkspaceConfig.Object));

        // UpdateExternalWorkspaceCommand
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<UpdateExternalWorkspaceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // SyncIterations - GetIterations
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetIterations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalIteration<AzdoIterationMetadata>>()));

        // SyncAzureDevOpsIterationsCommand
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<SyncAzureDevOpsIterationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // GetWorkspaceWorkTypesQuery
        var workTypes = new List<IWorkTypeDto>().AsReadOnly() as IReadOnlyList<IWorkTypeDto>;
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkspaceWorkTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(workTypes));

        // GetWorkspaceMostRecentChangeDateQuery
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkspaceMostRecentChangeDateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Instant?>(null));

        // SyncWorkItems - GetWorkItems
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkItems(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalWorkItem>()));

        // GetIterationMappingsQuery
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetIterationMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Guid>());

        // SyncWorkItemParentChanges - GetParentLinkChanges (only for Differential)
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetParentLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalWorkItemLink>()));

        // SyncWorkItemDependencyChanges - GetDependencyLinkChanges
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetDependencyLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalWorkItemLink>()));

        // SyncDeletedWorkItems - GetDeletedWorkItemIds
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetDeletedWorkItemIds(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Array.Empty<int>()));

        // ProcessDependenciesCommand
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        return (connectionId, wpInternalId, wsInternalId, wpExternalId, wsExternalId);
    }

    #endregion

    #region No Active Connections

    [Fact]
    public async Task Sync_WithNoConnections_ReturnsFailure()
    {
        // Arrange
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto>().AsReadOnly());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No active Azure DevOps connections found");
    }

    [Fact]
    public async Task Sync_WithOnlyInvalidConnections_ReturnsFailure()
    {
        // Arrange
        var invalidConnection = CreateConnectionDto(isValidConfiguration: false, isSyncEnabled: true);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { invalidConnection }.AsReadOnly());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Sync_WithSyncDisabledConnections_ReturnsFailure()
    {
        // Arrange
        var disabledConnection = CreateConnectionDto(isValidConfiguration: true, isSyncEnabled: false);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { disabledConnection }.AsReadOnly());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task Sync_WhenCancelledBeforeConnectionLoop_ReturnsSuccess()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var connectionDto = CreateConnectionDto();
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        // Cancel before the init manager is called
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .Callback(() => cts.Cancel())
            .ReturnsAsync(Result.Success());

        var connectionDetails = CreateConnectionDetails(connectionDto.Id,
            workProcesses: [CreateWorkProcess()]);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetAzureDevOpsConnectionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionDetails);

        // Act
        var result = await _sut.Sync(SyncType.Full, cts.Token);

        // Assert - cancellation at the work process loop level returns success
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Organization Sync Failures

    [Fact]
    public async Task Sync_WhenOrgSyncFails_ContinuesToNextConnection()
    {
        // Arrange
        var connection1 = CreateConnectionDto();
        var connection2 = CreateConnectionDto();

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connection1, connection2 }.AsReadOnly());

        // First connection fails org sync
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connection1.Id, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Failure("Org sync failed"));

        // Second connection succeeds org sync but has no work processes
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connection2.Id, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        var details2 = CreateConnectionDetails(connection2.Id);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connection2.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details2);

        // ProcessDependenciesCommand for connection2
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert - sync completes successfully despite first connection failing
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Verify(m => m.SyncOrganizationConfiguration(connection2.Id, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()), Times.Once);
    }

    #endregion

    #region No Active Work Processes

    [Fact]
    public async Task Sync_WithNoActiveWorkProcesses_ContinuesToNextConnection()
    {
        // Arrange
        var connectionId = Guid.CreateVersion7();
        var connectionDto = CreateConnectionDto(id: connectionId);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connectionId, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        // Connection details with no active work processes (IntegrationState is null)
        var inactiveWp = CreateWorkProcess(isActive: false);
        var details = CreateConnectionDetails(connectionId, workProcesses: [inactiveWp]);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should never attempt to get a work process from Azure DevOps
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Work Process Sync Failures

    [Fact]
    public async Task Sync_WhenWorkProcessSyncFails_ContinuesToNextWorkProcess()
    {
        // Arrange
        var connectionId = Guid.CreateVersion7();
        var wp1ExternalId = Guid.CreateVersion7();
        var wp1InternalId = Guid.CreateVersion7();
        var wp2ExternalId = Guid.CreateVersion7();
        var wp2InternalId = Guid.CreateVersion7();

        var connectionDto = CreateConnectionDto(id: connectionId);
        var wp1 = CreateWorkProcess(externalId: wp1ExternalId, internalId: wp1InternalId);
        var wp2 = CreateWorkProcess(externalId: wp2ExternalId, internalId: wp2InternalId);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connectionId, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        var details = CreateConnectionDetails(connectionId, workProcesses: [wp1, wp2]);
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);

        // First work process fails
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), wp1ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IExternalWorkProcessConfiguration>("Failed to get work process"));

        // Second work process succeeds
        var mockWpConfig = new Mock<IExternalWorkProcessConfiguration>();
        mockWpConfig.Setup(p => p.Name).Returns("Scrum");
        mockWpConfig.Setup(p => p.WorkTypes).Returns(new List<IExternalWorkTypeWorkflow>());
        mockWpConfig.Setup(p => p.WorkStatuses).Returns(new List<IExternalWorkStatus>());
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), wp2ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mockWpConfig.Object));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkProcessSchemesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IWorkProcessSchemeDto>().AsReadOnly() as IReadOnlyList<IWorkProcessSchemeDto>);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<UpdateExternalWorkProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), wp2ExternalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Full Sync vs Differential Sync

    [Fact]
    public async Task Sync_FullSync_DoesNotQueryMostRecentChangeDate()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Full);

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<ISender>()
            .Verify(s => s.Send(It.IsAny<GetWorkspaceMostRecentChangeDateQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Sync_DifferentialSync_QueriesMostRecentChangeDate()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Differential);

        // Act
        var result = await _sut.Sync(SyncType.Differential, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<ISender>()
            .Verify(s => s.Send(It.IsAny<GetWorkspaceMostRecentChangeDateQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sync_DifferentialSync_SyncsParentChanges()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Differential);

        // Act
        var result = await _sut.Sync(SyncType.Differential, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetParentLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sync_FullSync_DoesNotSyncParentChanges()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Full);

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetParentLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Workspace Sync Failures

    [Fact]
    public async Task Sync_WhenWorkspaceSyncFails_ContinuesToNextWorkspace()
    {
        // Arrange
        var connectionId = Guid.CreateVersion7();
        var wpExternalId = Guid.CreateVersion7();
        var wpInternalId = Guid.CreateVersion7();
        var ws1ExternalId = Guid.CreateVersion7();
        var ws1InternalId = Guid.CreateVersion7();
        var ws2ExternalId = Guid.CreateVersion7();
        var ws2InternalId = Guid.CreateVersion7();

        var connectionDto = CreateConnectionDto(id: connectionId);
        var workProcess = CreateWorkProcess(externalId: wpExternalId, internalId: wpInternalId);
        var ws1 = CreateWorkspace(wpExternalId, externalId: ws1ExternalId, internalId: ws1InternalId, name: "Project1");
        var ws2 = CreateWorkspace(wpExternalId, externalId: ws2ExternalId, internalId: ws2InternalId, name: "Project2");

        var details = CreateConnectionDetails(connectionId, workProcesses: [workProcess], workspaces: [ws1, ws2]);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connectionId, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);

        // Work process sync succeeds
        var mockWpConfig = new Mock<IExternalWorkProcessConfiguration>();
        mockWpConfig.Setup(p => p.Name).Returns("Agile");
        mockWpConfig.Setup(p => p.WorkTypes).Returns(new List<IExternalWorkTypeWorkflow>());
        mockWpConfig.Setup(p => p.WorkStatuses).Returns(new List<IExternalWorkStatus>());
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), wpExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mockWpConfig.Object));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkProcessSchemesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IWorkProcessSchemeDto>().AsReadOnly() as IReadOnlyList<IWorkProcessSchemeDto>);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<UpdateExternalWorkProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // First workspace fails
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkspace(It.IsAny<string>(), It.IsAny<string>(), ws1ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IExternalWorkspaceConfiguration>("Workspace sync failed"));

        // Second workspace succeeds
        var mockWsConfig = new Mock<IExternalWorkspaceConfiguration>();
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkspace(It.IsAny<string>(), It.IsAny<string>(), ws2ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mockWsConfig.Object));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<UpdateExternalWorkspaceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Iterations, work items, etc. for ws2
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetIterations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalIteration<AzdoIterationMetadata>>()));
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<SyncAzureDevOpsIterationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var workTypes = new List<IWorkTypeDto>().AsReadOnly() as IReadOnlyList<IWorkTypeDto>;
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkspaceWorkTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(workTypes));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetWorkspaceMostRecentChangeDateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Instant?>(null));

        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkItems(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalWorkItem>()));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetIterationMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Guid>());

        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetDependencyLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<IExternalWorkItemLink>()));

        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetDeletedWorkItemIds(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Array.Empty<int>()));

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert - sync continues and processes second workspace
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkspace(It.IsAny<string>(), It.IsAny<string>(), ws2ExternalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Happy Path

    [Fact]
    public async Task Sync_HappyPath_DifferentialSync_Succeeds()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Differential);

        // Act
        var result = await _sut.Sync(SyncType.Differential, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify the full pipeline executed
        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Verify(m => m.SyncOrganizationConfiguration(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<Guid?>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkspace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetIterations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkItems(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetDependencyLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetDeletedWorkItemIds(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<ISender>()
            .Verify(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sync_HappyPath_FullSync_Succeeds()
    {
        // Arrange
        SetupHappyPathMocks(SyncType.Full);

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Work Item Sync Failure - Independent Steps

    [Fact]
    public async Task Sync_WhenWorkItemSyncFails_StillAttempsRemainingWorkspaceSteps()
    {
        // Arrange
        var ids = SetupHappyPathMocks(SyncType.Differential);

        // Override: make GetWorkItems fail
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetWorkItems(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<IExternalWorkItem>>("Work items sync failed"));

        // Act
        var result = await _sut.Sync(SyncType.Differential, CancellationToken.None);

        // Assert - sync still succeeds overall
        result.IsSuccess.Should().BeTrue();

        // Parent changes, dependency changes, and deleted items should still be attempted
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetParentLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetDependencyLinkChanges(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetDeletedWorkItemIds(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Connection Details Null

    [Fact]
    public async Task Sync_WhenConnectionDetailsNull_ContinuesToNextConnection()
    {
        // Arrange
        var connectionId = Guid.CreateVersion7();
        var connectionDto = CreateConnectionDto(id: connectionId);

        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<GetConnectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConnectionListDto> { connectionDto }.AsReadOnly());

        _mocker.GetMock<IAzureDevOpsInitManager>()
            .Setup(m => m.SyncOrganizationConfiguration(connectionId, It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(Result.Success());

        // Return null connection details
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.Is<GetAzureDevOpsConnectionQuery>(q => q.ConnectionId == connectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzureDevOpsConnectionDetailsDto?)null);

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Iteration Sync Failures

    [Fact]
    public async Task Sync_WhenIterationSyncFails_ContinuesToNextWorkspace()
    {
        // Arrange
        var ids = SetupHappyPathMocks(SyncType.Full);

        // Override: make iteration sync fail
        _mocker.GetMock<IAzureDevOpsService>()
            .Setup(s => s.GetIterations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<IExternalIteration<AzdoIterationMetadata>>>("Iteration sync failed"));

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert - sync still succeeds overall, but work items were never attempted
        result.IsSuccess.Should().BeTrue();
        _mocker.GetMock<IAzureDevOpsService>()
            .Verify(s => s.GetWorkItems(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string[]>(), It.IsAny<Dictionary<Guid, Guid?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Process Dependencies

    [Fact]
    public async Task Sync_WhenProcessDependenciesFails_ContinuesToNextConnection()
    {
        // Arrange
        var ids = SetupHappyPathMocks(SyncType.Full);

        // Override: make ProcessDependencies fail
        _mocker.GetMock<ISender>()
            .Setup(s => s.Send(It.IsAny<ProcessDependenciesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Process dependencies failed"));

        // Act
        var result = await _sut.Sync(SyncType.Full, CancellationToken.None);

        // Assert - sync still completes
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
