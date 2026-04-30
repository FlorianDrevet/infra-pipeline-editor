using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.WebAppAggregate;

public sealed class WebAppTests
{
    private const string DefaultWebAppName = "wa-prod-front";
    private const string DefaultRuntimeVersion = "8.0";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static WebApp CreateValidWebApp(
        bool isExisting = false,
        AzureResourceId? containerRegistryId = null,
        AcrAuthMode? acrAuthMode = null,
        DeploymentMode.DeploymentModeType deploymentMode = DeploymentMode.DeploymentModeType.Code)
    {
        return WebApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultWebAppName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(deploymentMode),
            containerRegistryId,
            acrAuthMode,
            dockerImageName: null,
            isExisting: isExisting);
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Act
        var sut = CreateValidWebApp();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Value.Should().Be(DefaultWebAppName);
        sut.RuntimeStack.Value.Should().Be(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet);
        sut.RuntimeVersion.Should().Be(DefaultRuntimeVersion);
        sut.AlwaysOn.Should().BeTrue();
        sut.HttpsOnly.Should().BeTrue();
        sut.DeploymentMode.Value.Should().Be(DeploymentMode.DeploymentModeType.Code);
        sut.ContainerRegistryId.Should().BeNull();
        sut.AcrAuthMode.Should().BeNull();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullContainerRegistry_When_Create_Then_AcrAuthModeIsCleared()
    {
        // Arrange — pass an ACR auth mode but no registry id
        var sut = WebApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultWebAppName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: "myorg/myapp");

        // Assert
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_ContainerRegistryAndAuthMode_When_Create_Then_AcrAuthModeIsPreserved()
    {
        // Arrange
        var registryId = AzureResourceId.CreateUnique();
        var authMode = new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity);

        // Act
        var sut = WebApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultWebAppName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            containerRegistryId: registryId,
            acrAuthMode: authMode,
            dockerImageName: "myorg/myapp");

        // Assert
        sut.ContainerRegistryId.Should().Be(registryId);
        sut.AcrAuthMode.Should().Be(authMode);
    }

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidWebApp();
        var newName = new Name("wa-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        var newPlanId = AzureResourceId.CreateUnique();
        var newStack = new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.Node);

        // Act
        sut.Update(
            newName,
            newLocation,
            newPlanId,
            newStack,
            "20",
            alwaysOn: false,
            httpsOnly: false,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null,
            dockerfilePath: null,
            sourceCodePath: "src/api",
            buildCommand: "npm run build",
            applicationName: "Front API");

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.AppServicePlanId.Should().Be(newPlanId);
        sut.RuntimeStack.Should().Be(newStack);
        sut.RuntimeVersion.Should().Be("20");
        sut.AlwaysOn.Should().BeFalse();
        sut.HttpsOnly.Should().BeFalse();
        sut.SourceCodePath.Should().Be("src/api");
        sut.BuildCommand.Should().Be("npm run build");
        sut.ApplicationName.Should().Be("Front API");
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidWebApp(isExisting: true);
        var initialPlanId = sut.AppServicePlanId;
        var newName = new Name("wa-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        // Act
        sut.Update(
            newName,
            newLocation,
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.Node),
            "20",
            alwaysOn: false,
            httpsOnly: false,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null,
            dockerfilePath: null,
            sourceCodePath: null,
            buildCommand: null,
            applicationName: null);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.AppServicePlanId.Should().Be(initialPlanId);
        sut.RuntimeStack.Value.Should().Be(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet);
        sut.RuntimeVersion.Should().Be(DefaultRuntimeVersion);
    }

    [Fact]
    public void Given_UpdateWithNullRegistry_When_Update_Then_AcrAuthModeIsCleared()
    {
        // Arrange
        var sut = CreateValidWebApp();

        // Act
        sut.Update(
            sut.Name,
            sut.Location,
            sut.AppServicePlanId,
            sut.RuntimeStack,
            sut.RuntimeVersion,
            sut.AlwaysOn,
            sut.HttpsOnly,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.AdminCredentials),
            dockerImageName: "myorg/img",
            dockerfilePath: null,
            sourceCodePath: null,
            buildCommand: null,
            applicationName: null);

        // Assert
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidWebApp();

        // Act
        sut.SetEnvironmentSettings("prod", alwaysOn: true, httpsOnly: false, dockerImageTag: "v1.2.3");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.AlwaysOn.Should().BeTrue();
        entry.HttpsOnly.Should().BeFalse();
        entry.DockerImageTag.Should().Be("v1.2.3");
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidWebApp();
        sut.SetEnvironmentSettings("prod", alwaysOn: false, httpsOnly: false, dockerImageTag: "v1");

        // Act
        sut.SetEnvironmentSettings("prod", alwaysOn: true, httpsOnly: true, dockerImageTag: "v2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().DockerImageTag.Should().Be("v2");
        sut.EnvironmentSettings.Single().AlwaysOn.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidWebApp(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("prod", alwaysOn: true, httpsOnly: true, dockerImageTag: "v1");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidWebApp();
        sut.SetEnvironmentSettings("dev", alwaysOn: false, httpsOnly: false, dockerImageTag: "old");
        var settings = new[]
        {
            ("staging", (bool?)false, (bool?)true, (string?)"v1"),
            ("prod", (bool?)true, (bool?)true, (string?)"v2"),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }
}
