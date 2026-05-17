using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
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

namespace InfraFlowSculptor.Application.Tests.Projects.Common;

public sealed class ApplicationFolderNameResolverTests
{
    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IWebAppRepository _webAppRepository;
    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ApplicationFolderNameResolver _sut;

    public ApplicationFolderNameResolverTests()
    {
        _containerAppRepository = Substitute.For<IContainerAppRepository>();
        _webAppRepository = Substitute.For<IWebAppRepository>();
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();

        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-apps"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));

        _sut = new ApplicationFolderNameResolver(
            _containerAppRepository,
            _webAppRepository,
            _functionAppRepository);
    }

    [Fact]
    public async Task Given_ContainerAppResource_When_ResolveAsync_Then_UsesDetachedContainerAppLookupAsync()
    {
        // Arrange
        var containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null);
        var resource = CreateResourceReadModel(containerApp.Id, AzureResourceTypes.ArmTypes.ContainerAppType, "fallback-name");

        _containerAppRepository.GetByIdReadOnlyAsync(containerApp.Id, Arg.Any<CancellationToken>())
            .Returns(containerApp);

        // Act
        var result = await _sut.ResolveAsync(resource, CancellationToken.None);

        // Assert
        result.Should().Be(resource.Name);
        await _containerAppRepository.Received(1)
            .GetByIdReadOnlyAsync(containerApp.Id, Arg.Any<CancellationToken>());
        await _containerAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_WebAppResource_When_ResolveAsync_Then_UsesDetachedWebAppLookupAsync()
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
        var resource = CreateResourceReadModel(webApp.Id, AzureResourceTypes.ArmTypes.WebAppType, "fallback-name");

        _webAppRepository.GetByIdReadOnlyAsync(webApp.Id, Arg.Any<CancellationToken>())
            .Returns(webApp);

        // Act
        var result = await _sut.ResolveAsync(resource, CancellationToken.None);

        // Assert
        result.Should().Be(resource.Name);
        await _webAppRepository.Received(1)
            .GetByIdReadOnlyAsync(webApp.Id, Arg.Any<CancellationToken>());
        await _webAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_FunctionAppResource_When_ResolveAsync_Then_UsesDetachedFunctionAppLookupAsync()
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
        var resource = CreateResourceReadModel(functionApp.Id, AzureResourceTypes.ArmTypes.FunctionAppType, "fallback-name");

        _functionAppRepository.GetByIdReadOnlyAsync(functionApp.Id, Arg.Any<CancellationToken>())
            .Returns(functionApp);

        // Act
        var result = await _sut.ResolveAsync(resource, CancellationToken.None);

        // Assert
        result.Should().Be(resource.Name);
        await _functionAppRepository.Received(1)
            .GetByIdReadOnlyAsync(functionApp.Id, Arg.Any<CancellationToken>());
        await _functionAppRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    private static AzureResourceReadModel CreateResourceReadModel(
        AzureResourceId resourceId,
        string resourceType,
        string resourceName)
    {
        return new AzureResourceReadModel(
            resourceId.Value,
            resourceName,
            "francecentral",
            resourceType,
            new Dictionary<string, string>(),
            [],
            null,
            false,
            null);
    }
}
