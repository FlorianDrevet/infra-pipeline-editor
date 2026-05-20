using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Common.Services;

public sealed class AppPipelineRequestFactoryTests
{
    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IWebAppRepository _webAppRepository;
    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly IContainerRegistryRepository _containerRegistryRepository;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly AppPipelineRequestFactory _sut;

    public AppPipelineRequestFactoryTests()
    {
        _containerAppRepository = Substitute.For<IContainerAppRepository>();
        _webAppRepository = Substitute.For<IWebAppRepository>();
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();
        _containerRegistryRepository = Substitute.For<IContainerRegistryRepository>();

        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));

        _sut = new AppPipelineRequestFactory(
            _containerAppRepository,
            _webAppRepository,
            _functionAppRepository,
            _containerRegistryRepository);
    }

    [Fact]
    public async Task Given_ContainerAppWithRegistry_When_CreateAsync_Then_ReturnsContainerPipelineRequestAsync()
    {
        // Arrange
        var registry = ContainerRegistry.Create(
            _resourceGroup.Id,
            new Name("acrshared"),
            new Location(Location.LocationEnum.FranceCentral));
        var containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: registry.Id,
            acrAuthMode: null);
        _containerAppRepository.GetByIdReadOnlyAsync(containerApp.Id, Arg.Any<CancellationToken>())
            .Returns(containerApp);
        _containerRegistryRepository.GetByIdReadOnlyAsync(registry.Id, Arg.Any<CancellationToken>())
            .Returns(registry);

        // Act
        var result = await _sut.CreateAsync(
            containerApp.Id,
            AzureResourceTypes.ArmTypes.ContainerAppType,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ResourceName.Should().Be(containerApp.Name.Value);
        result.ResourceType.Should().Be(AzureResourceTypes.ContainerApp);
        result.ContainerRegistryName.Should().Be(registry.Name.Value);
        await _containerAppRepository.Received(1)
            .GetByIdReadOnlyAsync(containerApp.Id, Arg.Any<CancellationToken>());
        await _containerRegistryRepository.Received(1)
            .GetByIdReadOnlyAsync(registry.Id, Arg.Any<CancellationToken>());
        await _containerAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _containerRegistryRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ContainerAppWithEnvironmentAcrServiceConnection_When_CreateAsync_Then_ReturnsPipelineRequestWithConnectionAsync()
    {
        // Arrange
        const string environmentName = "dev";
        const string serviceConnection = "ifs-dev-acr-sc";
        var registry = ContainerRegistry.Create(
            _resourceGroup.Id,
            new Name("acrshared"),
            new Location(Location.LocationEnum.FranceCentral));
        var containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: registry.Id,
            acrAuthMode: null,
            environmentSettings:
            [
                new ContainerAppEnvironmentSettingsData(
                    EnvironmentName: environmentName,
                    CpuCores: null,
                    MemoryGi: null,
                    MinReplicas: null,
                    MaxReplicas: null,
                    IngressEnabled: null,
                    IngressTargetPort: null,
                    IngressExternal: null,
                    TransportMethod: null,
                    ContainerRegistryServiceConnection: serviceConnection),
            ]);
        _containerAppRepository.GetByIdReadOnlyAsync(containerApp.Id, Arg.Any<CancellationToken>())
            .Returns(containerApp);
        _containerRegistryRepository.GetByIdReadOnlyAsync(registry.Id, Arg.Any<CancellationToken>())
            .Returns(registry);

        // Act
        var result = await _sut.CreateAsync(
            containerApp.Id,
            AzureResourceTypes.ArmTypes.ContainerAppType,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ContainerRegistryServiceConnections.Should().ContainSingle(connection =>
            connection.EnvironmentName == environmentName &&
            connection.ServiceConnectionName == serviceConnection);
    }

    [Fact]
    public async Task Given_WebAppCodeDeployment_When_CreateAsync_Then_ReturnsWebAppPipelineRequestAsync()
    {
        // Arrange
        var webApp = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
        _webAppRepository.GetByIdReadOnlyAsync(webApp.Id, Arg.Any<CancellationToken>())
            .Returns(webApp);

        // Act
        var result = await _sut.CreateAsync(
            webApp.Id,
            AzureResourceTypes.ArmTypes.WebAppType,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ResourceType.Should().Be(AzureResourceTypes.WebApp);
        result.DeploymentMode.Should().Be(DeploymentMode.DeploymentModeType.Code.ToString());
        result.RuntimeVersion.Should().Be("8.0");
        result.ContainerRegistryName.Should().BeNull();
        await _webAppRepository.Received(1)
            .GetByIdReadOnlyAsync(webApp.Id, Arg.Any<CancellationToken>());
        await _webAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_FunctionAppCodeDeployment_When_CreateAsync_Then_ReturnsFunctionAppPipelineRequestAsync()
    {
        // Arrange
        var functionApp = FunctionApp.Create(
            _resourceGroup.Id,
            new Name("func-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            "8.0",
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
        _functionAppRepository.GetByIdReadOnlyAsync(functionApp.Id, Arg.Any<CancellationToken>())
            .Returns(functionApp);

        // Act
        var result = await _sut.CreateAsync(
            functionApp.Id,
            AzureResourceTypes.ArmTypes.FunctionAppType,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ResourceType.Should().Be(AzureResourceTypes.FunctionApp);
        result.DeploymentMode.Should().Be(DeploymentMode.DeploymentModeType.Code.ToString());
        result.RuntimeVersion.Should().Be("8.0");
        result.ContainerRegistryName.Should().BeNull();
        await _functionAppRepository.Received(1)
            .GetByIdReadOnlyAsync(functionApp.Id, Arg.Any<CancellationToken>());
        await _functionAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_UnsupportedResourceType_When_CreateAsync_Then_ReturnsNullAsync()
    {
        // Act
        var result = await _sut.CreateAsync(
            AzureResourceId.CreateUnique(),
            AzureResourceTypes.ArmTypes.StorageAccountType,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _containerAppRepository.DidNotReceive()
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _webAppRepository.DidNotReceive()
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _functionAppRepository.DidNotReceive()
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _containerAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _webAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _functionAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}
