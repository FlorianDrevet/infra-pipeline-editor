using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ContainerAppAggregate;

public sealed class ContainerAppTests
{
    private const string DefaultName = "ca-prod-api";
    private const string DefaultDockerImage = "myregistry.azurecr.io/myapp/api";
    private const string DefaultDockerfilePath = "src/Api/Dockerfile";
    private const string DefaultApplicationName = "MyApi";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ContainerApp CreateValidContainerApp(
        bool isExisting = false,
        AzureResourceId? containerRegistryId = null,
        AcrAuthMode? acrAuthMode = null)
    {
        return ContainerApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            containerRegistryId,
            acrAuthMode,
            dockerImageName: DefaultDockerImage,
            dockerfilePath: DefaultDockerfilePath,
            applicationName: DefaultApplicationName,
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();
        var environmentId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerApp.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentId,
            containerRegistryId: null,
            acrAuthMode: null);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.ContainerAppEnvironmentId.Should().Be(environmentId);
        sut.ContainerRegistryId.Should().BeNull();
        sut.AcrAuthMode.Should().BeNull();
        sut.DockerImageName.Should().BeNull();
        sut.DockerfilePath.Should().BeNull();
        sut.ApplicationName.Should().BeNull();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullContainerRegistry_When_Create_Then_ForcesAcrAuthModeToNull()
    {
        // Act
        var sut = ContainerApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity));

        // Assert
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_ContainerRegistryId_When_Create_Then_KeepsAcrAuthMode()
    {
        // Arrange
        var registryId = AzureResourceId.CreateUnique();
        var authMode = new AcrAuthMode(AcrAuthMode.AcrAuthModeType.AdminCredentials);

        // Act
        var sut = CreateValidContainerApp(containerRegistryId: registryId, acrAuthMode: authMode);

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
            (DevEnvironment, (string?)"0.5", (string?)"1.0Gi", (int?)1, (int?)3, (bool?)true, (int?)80, (bool?)true, (string?)"http", (string?)null, (int?)null, (string?)null, (int?)null, (string?)null, (int?)null),
        };

        // Act
        var sut = ContainerApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null,
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"0.5", (string?)"1.0Gi", (int?)1, (int?)3, (bool?)true, (int?)80, (bool?)true, (string?)"http", (string?)null, (int?)null, (string?)null, (int?)null, (string?)null, (int?)null),
        };

        // Act
        var sut = ContainerApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null,
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
        var sut = CreateValidContainerApp();
        var newEnvironmentId = AzureResourceId.CreateUnique();
        var newRegistryId = AzureResourceId.CreateUnique();

        // Act
        sut.Update(
            new Name("ca-renamed"),
            new Location(Location.LocationEnum.NorthEurope),
            newEnvironmentId,
            newRegistryId,
            new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            "newimage",
            "src/New/Dockerfile",
            "NewApp");

        // Assert
        sut.Name.Value.Should().Be("ca-renamed");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.ContainerAppEnvironmentId.Should().Be(newEnvironmentId);
        sut.ContainerRegistryId.Should().Be(newRegistryId);
        sut.AcrAuthMode!.Value.Should().Be(AcrAuthMode.AcrAuthModeType.ManagedIdentity);
        sut.DockerImageName.Should().Be("newimage");
        sut.DockerfilePath.Should().Be("src/New/Dockerfile");
        sut.ApplicationName.Should().Be("NewApp");
    }

    [Fact]
    public void Given_NullContainerRegistry_When_Update_Then_ForcesAcrAuthModeToNull()
    {
        // Arrange
        var sut = CreateValidContainerApp(
            containerRegistryId: AzureResourceId.CreateUnique(),
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity));

        // Act
        sut.Update(
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: null,
            dockerfilePath: null,
            applicationName: null);

        // Assert
        sut.ContainerRegistryId.Should().BeNull();
        sut.AcrAuthMode.Should().BeNull();
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyChangesNameAndLocation()
    {
        // Arrange
        var sut = CreateValidContainerApp(isExisting: true);
        var originalEnvironmentId = sut.ContainerAppEnvironmentId;

        // Act
        sut.Update(
            new Name("renamed"),
            new Location(Location.LocationEnum.NorthEurope),
            AzureResourceId.CreateUnique(),
            containerRegistryId: AzureResourceId.CreateUnique(),
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: "newimage",
            dockerfilePath: null,
            applicationName: null);

        // Assert
        sut.Name.Value.Should().Be("renamed");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.ContainerAppEnvironmentId.Should().Be(originalEnvironmentId);
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidContainerApp();

        // Act
        sut.SetEnvironmentSettings(
            DevEnvironment, "0.5", "1.0Gi", 1, 3, true, 80, true, "http");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.EnvironmentName.Should().Be(DevEnvironment);
        entry.CpuCores.Should().Be("0.5");
        entry.MemoryGi.Should().Be("1.0Gi");
        entry.MinReplicas.Should().Be(1);
        entry.MaxReplicas.Should().Be(3);
        entry.IngressEnabled.Should().BeTrue();
        entry.IngressTargetPort.Should().Be(80);
        entry.IngressExternal.Should().BeTrue();
        entry.TransportMethod.Should().Be("http");
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidContainerApp();
        sut.SetEnvironmentSettings(DevEnvironment, "0.25", "0.5Gi", 1, 2, false, 80, false, "auto");

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "1.0", "2.0Gi", 2, 5, true, 8080, true, "http2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.CpuCores.Should().Be("1.0");
        entry.MemoryGi.Should().Be("2.0Gi");
        entry.MinReplicas.Should().Be(2);
        entry.MaxReplicas.Should().Be(5);
        entry.IngressEnabled.Should().BeTrue();
        entry.IngressTargetPort.Should().Be(8080);
        entry.IngressExternal.Should().BeTrue();
        entry.TransportMethod.Should().Be("http2");
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidContainerApp(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "0.5", "1.0Gi", 1, 3, true, 80, true, "http");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidContainerApp();
        sut.SetEnvironmentSettings(DevEnvironment, "0.25", "0.5Gi", 1, 1, false, null, null, null);
        var settings = new[]
        {
            ("staging", (string?)"0.5", (string?)"1Gi", (int?)1, (int?)2, (bool?)true, (int?)80, (bool?)false, (string?)"http", (string?)null, (int?)null, (string?)null, (int?)null, (string?)null, (int?)null),
            (ProdEnvironment, (string?)"1.0", (string?)"2Gi", (int?)2, (int?)10, (bool?)true, (int?)443, (bool?)true, (string?)"http2", (string?)null, (int?)null, (string?)null, (int?)null, (string?)null, (int?)null),
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
        var sut = CreateValidContainerApp(isExisting: true);
        var settings = new[]
        {
            (DevEnvironment, (string?)"0.5", (string?)"1Gi", (int?)1, (int?)3, (bool?)true, (int?)80, (bool?)true, (string?)"http", (string?)null, (int?)null, (string?)null, (int?)null, (string?)null, (int?)null),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
