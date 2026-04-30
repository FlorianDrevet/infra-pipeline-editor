using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.FunctionAppAggregate;

public sealed class FunctionAppTests
{
    private const string DefaultName = "func-prod-data";
    private const string DefaultRuntimeVersion = "8.0";
    private const string DefaultDockerImage = "myregistry.azurecr.io/myapp/func";
    private const string DefaultDockerfilePath = "src/Func/Dockerfile";
    private const string DefaultSourceCodePath = "src/Func";
    private const string DefaultBuildCommand = "dotnet publish -c Release";
    private const string DefaultApplicationName = "MyFunction";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static FunctionApp CreateValidFunctionApp(
        bool isExisting = false,
        AzureResourceId? containerRegistryId = null,
        AcrAuthMode? acrAuthMode = null)
    {
        return FunctionApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId,
            acrAuthMode,
            dockerImageName: null,
            sourceCodePath: DefaultSourceCodePath,
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();
        var planId = AzureResourceId.CreateUnique();

        // Act
        var sut = FunctionApp.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            planId,
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null,
            sourceCodePath: DefaultSourceCodePath,
            buildCommand: DefaultBuildCommand,
            applicationName: DefaultApplicationName);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.AppServicePlanId.Should().Be(planId);
        sut.RuntimeStack.Value.Should().Be(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet);
        sut.RuntimeVersion.Should().Be(DefaultRuntimeVersion);
        sut.HttpsOnly.Should().BeTrue();
        sut.DeploymentMode.Value.Should().Be(DeploymentMode.DeploymentModeType.Code);
        sut.ContainerRegistryId.Should().BeNull();
        sut.AcrAuthMode.Should().BeNull();
        sut.DockerImageName.Should().BeNull();
        sut.SourceCodePath.Should().Be(DefaultSourceCodePath);
        sut.BuildCommand.Should().Be(DefaultBuildCommand);
        sut.ApplicationName.Should().Be(DefaultApplicationName);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullContainerRegistry_When_Create_Then_ForcesAcrAuthModeToNull()
    {
        // Act
        var sut = FunctionApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: DefaultDockerImage);

        // Assert
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_ContainerRegistryId_When_Create_Then_KeepsAcrAuthMode()
    {
        // Arrange
        var registryId = AzureResourceId.CreateUnique();
        var authMode = new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity);

        // Act
        var sut = CreateValidFunctionApp(containerRegistryId: registryId, acrAuthMode: authMode);

        // Assert
        sut.ContainerRegistryId.Should().Be(registryId);
        sut.AcrAuthMode.Should().Be(authMode);
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (bool?)true, (int?)2, (string?)"latest"),
            (ProdEnvironment, (bool?)true, (int?)10, (string?)"v1"),
        };

        // Act
        var sut = FunctionApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Node),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null,
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[] { (DevEnvironment, (bool?)true, (int?)2, (string?)"latest") };

        // Act
        var sut = FunctionApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null,
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = CreateValidFunctionApp();
        var newPlanId = AzureResourceId.CreateUnique();
        var newRegistryId = AzureResourceId.CreateUnique();

        // Act
        sut.Update(
            new Name("func-updated"),
            new Location(Location.LocationEnum.NorthEurope),
            newPlanId,
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Python),
            "3.11",
            httpsOnly: false,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            newRegistryId,
            new AcrAuthMode(AcrAuthMode.AcrAuthModeType.AdminCredentials),
            DefaultDockerImage,
            DefaultDockerfilePath,
            sourceCodePath: null,
            DefaultBuildCommand,
            DefaultApplicationName);

        // Assert
        sut.Name.Value.Should().Be("func-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.AppServicePlanId.Should().Be(newPlanId);
        sut.RuntimeStack.Value.Should().Be(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Python);
        sut.RuntimeVersion.Should().Be("3.11");
        sut.HttpsOnly.Should().BeFalse();
        sut.DeploymentMode.Value.Should().Be(DeploymentMode.DeploymentModeType.Container);
        sut.ContainerRegistryId.Should().Be(newRegistryId);
        sut.AcrAuthMode!.Value.Should().Be(AcrAuthMode.AcrAuthModeType.AdminCredentials);
        sut.DockerImageName.Should().Be(DefaultDockerImage);
        sut.DockerfilePath.Should().Be(DefaultDockerfilePath);
        sut.BuildCommand.Should().Be(DefaultBuildCommand);
        sut.ApplicationName.Should().Be(DefaultApplicationName);
    }

    [Fact]
    public void Given_NullContainerRegistry_When_Update_Then_ForcesAcrAuthModeToNull()
    {
        // Arrange
        var sut = CreateValidFunctionApp(
            containerRegistryId: AzureResourceId.CreateUnique(),
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity));

        // Act
        sut.Update(
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            DefaultRuntimeVersion,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: null,
            dockerfilePath: null,
            sourceCodePath: null,
            buildCommand: null,
            applicationName: null);

        // Assert
        sut.ContainerRegistryId.Should().BeNull();
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyChangesNameAndLocation()
    {
        // Arrange
        var sut = CreateValidFunctionApp(isExisting: true);
        var originalPlanId = sut.AppServicePlanId;
        var originalRuntime = sut.RuntimeStack.Value;

        // Act
        sut.Update(
            new Name("renamed-func"),
            new Location(Location.LocationEnum.NorthEurope),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Python),
            "3.11",
            httpsOnly: false,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Container),
            containerRegistryId: AzureResourceId.CreateUnique(),
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: DefaultDockerImage,
            dockerfilePath: null,
            sourceCodePath: null,
            buildCommand: null,
            applicationName: null);

        // Assert
        sut.Name.Value.Should().Be("renamed-func");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.AppServicePlanId.Should().Be(originalPlanId);
        sut.RuntimeStack.Value.Should().Be(originalRuntime);
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidFunctionApp();

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, httpsOnly: true, maxInstanceCount: 5, dockerImageTag: "v1");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.EnvironmentName.Should().Be(DevEnvironment);
        entry.HttpsOnly.Should().BeTrue();
        entry.MaxInstanceCount.Should().Be(5);
        entry.DockerImageTag.Should().Be("v1");
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidFunctionApp();
        sut.SetEnvironmentSettings(DevEnvironment, httpsOnly: true, maxInstanceCount: 2, dockerImageTag: "v1");

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, httpsOnly: false, maxInstanceCount: 10, dockerImageTag: "v2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.HttpsOnly.Should().BeFalse();
        entry.MaxInstanceCount.Should().Be(10);
        entry.DockerImageTag.Should().Be("v2");
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidFunctionApp(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, httpsOnly: true, maxInstanceCount: 5, dockerImageTag: "v1");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidFunctionApp();
        sut.SetEnvironmentSettings(DevEnvironment, true, 1, "old");
        var settings = new[]
        {
            ("staging", (bool?)true, (int?)2, (string?)"v2"),
            (ProdEnvironment, (bool?)true, (int?)10, (string?)"v3"),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == DevEnvironment);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetAllEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidFunctionApp(isExisting: true);
        var settings = new[] { (DevEnvironment, (bool?)true, (int?)2, (string?)"v1") };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
