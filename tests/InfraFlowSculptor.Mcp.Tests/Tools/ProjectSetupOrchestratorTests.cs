using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class ProjectSetupOrchestratorTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    // ── CreateInfrastructureAsync ──────────────────────────────────────

    [Fact]
    public async Task CreateInfrastructureAsync_Success_ReturnsConfigAndRgIds()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var configId = new InfrastructureConfigId(Guid.NewGuid());
        var rgId = new ResourceGroupId(Guid.NewGuid());

        var configResult = new GetInfrastructureConfigResult(
            configId, new Name("TestProject-config"), new ProjectId(projectId),
            null, false, [], [], [], 0, 0, 0, null, null);

        _mediator.Send(
                Arg.Any<IRequest<ErrorOr<GetInfrastructureConfigResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(configResult));

        var rgResult = new ResourceGroupResult(
            rgId, configId,
            new Location(Location.LocationEnum.WestEurope),
            new Name("TestProject-rg"), []);

        _mediator.Send(
                Arg.Any<IRequest<ErrorOr<ResourceGroupResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ResourceGroupResult>>(rgResult));

        // Act
        var result = await ProjectSetupOrchestrator.CreateInfrastructureAsync(
            _mediator, projectId, "TestProject", "francecentral");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ConfigId.Should().Be(configId);
        result.Value.RgId.Should().Be(rgId);

        await _mediator.Received(1).Send(
            Arg.Is<IRequest<ErrorOr<ResourceGroupResult>>>(request =>
                MatchesResourceGroupLocation(request, Location.LocationEnum.FranceCentral)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInfrastructureAsync_ConfigCommandFails_ReturnsError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        ErrorOr<GetInfrastructureConfigResult> errorResult =
            Error.Failure("InfraConfig.Failed", "Config creation failed");

        _mediator.Send(
                Arg.Any<IRequest<ErrorOr<GetInfrastructureConfigResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(errorResult));

        // Act
        var result = await ProjectSetupOrchestrator.CreateInfrastructureAsync(
            _mediator, projectId, "TestProject", "francecentral");

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Description == "Config creation failed");
    }

    [Fact]
    public async Task CreateInfrastructureAsync_RgCommandFails_ReturnsError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var configId = new InfrastructureConfigId(Guid.NewGuid());
        var configResult = new GetInfrastructureConfigResult(
            configId, new Name("TestProject-config"), new ProjectId(projectId),
            null, false, [], [], [], 0, 0, 0, null, null);

        _mediator.Send(
                Arg.Any<IRequest<ErrorOr<GetInfrastructureConfigResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(configResult));

        ErrorOr<ResourceGroupResult> rgError =
            Error.Failure("ResourceGroup.Failed", "RG creation failed");

        _mediator.Send(
                Arg.Any<IRequest<ErrorOr<ResourceGroupResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rgError));

        // Act
        var result = await ProjectSetupOrchestrator.CreateInfrastructureAsync(
            _mediator, projectId, "TestProject", "francecentral");

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Description == "RG creation failed");
    }

    // ── CreateResourcesAsync ──────────────────────────────────────────

    [Fact]
    public async Task CreateResourcesAsync_SimpleResource_ReturnsCreated()
    {
        // Arrange
        var rgId = new ResourceGroupId(Guid.NewGuid());
        var kvId = new AzureResourceId(Guid.NewGuid());
        var kvResult = new KeyVaultResult(
            kvId, rgId, new Name("test-kv"),
            new Location(Location.LocationEnum.WestEurope),
            false, false, false, false, false, false, []);

        _mediator.Send(Arg.Any<IBaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(
                (ErrorOr<KeyVaultResult>)kvResult));

        var resources = new List<ResourceInput>
        {
            new() { ResourceType = AzureResourceTypes.KeyVault, Name = "test-kv", Location = "francecentral" },
        };

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, resources);

        // Assert
        created.Should().ContainSingle();
        created[0].ResourceType.Should().Be(AzureResourceTypes.KeyVault);
        created[0].ResourceId.Should().Be(kvId.Value.ToString());
        created[0].Name.Should().Be("test-kv");
        skipped.Should().BeEmpty();

        await _mediator.Received(1).Send(
            Arg.Is<IBaseRequest>(request =>
                MatchesKeyVaultLocation(request, Location.LocationEnum.FranceCentral)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateResourcesAsync_DistinctNamedDependencies_UsesMatchingDependencyIds()
    {
        // Arrange
        var rgId = new ResourceGroupId(Guid.NewGuid());
        var lawEastId = new AzureResourceId(Guid.NewGuid());
        var lawWestId = new AzureResourceId(Guid.NewGuid());
        var aiEastId = new AzureResourceId(Guid.NewGuid());
        var aiWestId = new AzureResourceId(Guid.NewGuid());

        var lawEastResult = new LogAnalyticsWorkspaceResult(
            lawEastId,
            rgId,
            new Name("law-east"),
            new Location(Location.LocationEnum.FranceCentral),
            []);

        var lawWestResult = new LogAnalyticsWorkspaceResult(
            lawWestId,
            rgId,
            new Name("law-west"),
            new Location(Location.LocationEnum.FranceCentral),
            []);

        var aiEastResult = new ApplicationInsightsResult(
            aiEastId,
            rgId,
            new Name("ai-east"),
            new Location(Location.LocationEnum.FranceCentral),
            lawEastId.Value,
            []);

        var aiWestResult = new ApplicationInsightsResult(
            aiWestId,
            rgId,
            new Name("ai-west"),
            new Location(Location.LocationEnum.FranceCentral),
            lawWestId.Value,
            []);

        _mediator.Send(
                Arg.Any<IBaseRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<IBaseRequest>();

                return request switch
                {
                    CreateLogAnalyticsWorkspaceCommand command when command.Name.Value == "law-east"
                        => Task.FromResult<object?>((ErrorOr<LogAnalyticsWorkspaceResult>)lawEastResult),
                    CreateLogAnalyticsWorkspaceCommand command when command.Name.Value == "law-west"
                        => Task.FromResult<object?>((ErrorOr<LogAnalyticsWorkspaceResult>)lawWestResult),
                    CreateApplicationInsightsCommand command when command.Name.Value == "ai-east"
                        => Task.FromResult<object?>((ErrorOr<ApplicationInsightsResult>)aiEastResult),
                    CreateApplicationInsightsCommand command when command.Name.Value == "ai-west"
                        => Task.FromResult<object?>((ErrorOr<ApplicationInsightsResult>)aiWestResult),
                    _ => Task.FromResult<object?>(null),
                };
            });

        var resources = new List<ResourceInput>
        {
            new() { ResourceType = AzureResourceTypes.LogAnalyticsWorkspace, Name = "law-east", Location = "francecentral" },
            new() { ResourceType = AzureResourceTypes.LogAnalyticsWorkspace, Name = "law-west", Location = "francecentral" },
            new()
            {
                ResourceType = AzureResourceTypes.ApplicationInsights,
                Name = "ai-east",
                Location = "francecentral",
                DependencyResourceNames = ["law-east"],
            },
            new()
            {
                ResourceType = AzureResourceTypes.ApplicationInsights,
                Name = "ai-west",
                Location = "francecentral",
                DependencyResourceNames = ["law-west"],
            },
        };

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, resources);

        // Assert
        created.Should().HaveCount(4);
        skipped.Should().BeEmpty();

        await _mediator.Received(1).Send(
            Arg.Is<IBaseRequest>(request =>
                MatchesApplicationInsightsDependency(request, "ai-east", lawEastId.Value)),
            Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(
            Arg.Is<IBaseRequest>(request =>
                MatchesApplicationInsightsDependency(request, "ai-west", lawWestId.Value)),
            Arg.Any<CancellationToken>());
    }

    private static bool MatchesResourceGroupLocation(
        IRequest<ErrorOr<ResourceGroupResult>> request,
        Location.LocationEnum expectedLocation)
    {
        var command = request as CreateResourceGroupCommand;
        return command is not null && command.Location.Value == expectedLocation;
    }

    private static bool MatchesKeyVaultLocation(IBaseRequest request, Location.LocationEnum expectedLocation)
    {
        var command = request as CreateKeyVaultCommand;
        return command is not null && command.Location.Value == expectedLocation;
    }

    private static bool MatchesLogAnalyticsWorkspaceName(IBaseRequest request, string expectedName)
    {
        var command = request as CreateLogAnalyticsWorkspaceCommand;
        return command is not null && command.Name.Value == expectedName;
    }

    private static bool MatchesApplicationInsightsName(IBaseRequest request, string expectedName)
    {
        var command = request as CreateApplicationInsightsCommand;
        return command is not null && command.Name.Value == expectedName;
    }

    private static bool MatchesApplicationInsightsDependency(IBaseRequest request, string expectedName, Guid expectedWorkspaceId)
    {
        var command = request as CreateApplicationInsightsCommand;
        return command is not null
               && command.Name.Value == expectedName
               && command.LogAnalyticsWorkspaceId == expectedWorkspaceId;
    }

    [Fact]
    public async Task CreateResourcesAsync_UnknownResourceType_SkipsIt()
    {
        // Arrange
        var rgId = new ResourceGroupId(Guid.NewGuid());
        var resources = new List<ResourceInput>
        {
            new() { ResourceType = "VirtualMachine", Name = "test-vm" },
        };

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, resources);

        // Assert
        created.Should().BeEmpty();
        skipped.Should().ContainSingle();
        skipped[0].ResourceType.Should().Be("VirtualMachine");
        skipped[0].Reason.Should().Contain("not supported");
    }

    [Fact]
    public async Task CreateResourcesAsync_DependentWithoutDependency_SkipsWithReason()
    {
        // Arrange — WebApp without AppServicePlan
        var rgId = new ResourceGroupId(Guid.NewGuid());
        var resources = new List<ResourceInput>
        {
            new() { ResourceType = AzureResourceTypes.WebApp, Name = "test-webapp" },
        };

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, resources);

        // Assert
        created.Should().BeEmpty();
        skipped.Should().ContainSingle();
        skipped[0].Reason.Should().Contain(AzureResourceTypes.AppServicePlan);
    }

    [Fact]
    public async Task CreateResourcesAsync_MultipleResources_OrdersByDependency()
    {
        // Arrange — KeyVault + StorageAccount, both should succeed
        var rgId = new ResourceGroupId(Guid.NewGuid());
        var kvId = new AzureResourceId(Guid.NewGuid());
        var saId = new AzureResourceId(Guid.NewGuid());

        var kvResult = new KeyVaultResult(
            kvId, rgId, new Name("test-kv"),
            new Location(Location.LocationEnum.WestEurope),
            false, false, false, false, false, false, []);

        var saResult = new StorageAccountResult(
            saId, rgId, new Name("test-sa"),
            new Location(Location.LocationEnum.WestEurope),
            "StorageV2", "Hot", false, true, "TLS1_2", [], [], [], [], [], [], []);

        _mediator.Send(Arg.Any<IBaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    var cmd = callInfo.Arg<IBaseRequest>();
                    if (cmd is CreateKeyVaultCommand)
                        return Task.FromResult<object?>(
                            (ErrorOr<KeyVaultResult>)kvResult);
                    if (cmd is CreateStorageAccountCommand)
                        return Task.FromResult<object?>(
                            (ErrorOr<StorageAccountResult>)saResult);
                    return Task.FromResult<object?>(null);
                });

        var resources = new List<ResourceInput>
        {
            new() { ResourceType = AzureResourceTypes.KeyVault, Name = "test-kv" },
            new() { ResourceType = AzureResourceTypes.StorageAccount, Name = "test-sa" },
        };

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, resources);

        // Assert
        created.Should().HaveCount(2);
        skipped.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateResourcesAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var rgId = new ResourceGroupId(Guid.NewGuid());

        // Act
        var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
            _mediator, rgId, []);

        // Assert
        created.Should().BeEmpty();
        skipped.Should().BeEmpty();
    }
}
