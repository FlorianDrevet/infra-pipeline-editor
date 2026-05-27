using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;

namespace InfraFlowSculptor.Domain.Tests.ContainerAppAggregate.Entities;

public sealed class ContainerAppEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";
    private const string ContainerRegistryServiceConnection = "ifs-prod-acr-sc";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var containerAppId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerAppEnvironmentSettings.Create(
            containerAppId,
            new ContainerAppEnvironmentSettingsData(
                EnvironmentName,
                CpuCores: "0.5",
                MemoryGi: "1.0Gi",
                MinReplicas: 1,
                MaxReplicas: 3,
                IngressEnabled: true,
                IngressTargetPort: 80,
                IngressExternal: true,
                TransportMethod: "http",
                ReadinessProbePath: "/ready",
                ReadinessProbePort: 8080,
                LivenessProbePath: "/live",
                LivenessProbePort: 8081,
                StartupProbePath: "/startup",
                StartupProbePort: 8082,
                ContainerRegistryServiceConnection: ContainerRegistryServiceConnection));

        // Assert
        sut.ContainerAppId.Should().Be(containerAppId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.CpuCores.Should().Be("0.5");
        sut.MemoryGi.Should().Be("1.0Gi");
        sut.MinReplicas.Should().Be(1);
        sut.MaxReplicas.Should().Be(3);
        sut.IngressEnabled.Should().BeTrue();
        sut.IngressTargetPort.Should().Be(80);
        sut.IngressExternal.Should().BeTrue();
        sut.TransportMethod.Should().Be("http");
        sut.ReadinessProbePath.Should().Be("/ready");
        sut.ReadinessProbePort.Should().Be(8080);
        sut.LivenessProbePath.Should().Be("/live");
        sut.LivenessProbePort.Should().Be(8081);
        sut.StartupProbePath.Should().Be("/startup");
        sut.StartupProbePort.Should().Be(8082);
        sut.ContainerRegistryServiceConnection.Should().Be(ContainerRegistryServiceConnection);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            new ContainerAppEnvironmentSettingsData(
                EnvironmentName,
                CpuCores: "0.25",
                MemoryGi: "0.5Gi",
                MinReplicas: 0,
                MaxReplicas: 1,
                IngressEnabled: false,
                IngressExternal: false,
                TransportMethod: "auto"));

        // Act
        sut.Update(new ContainerAppEnvironmentSettingsData(
            EnvironmentName,
            CpuCores: "1.0",
            MemoryGi: "2.0Gi",
            MinReplicas: 2,
            MaxReplicas: 10,
            IngressEnabled: true,
            IngressTargetPort: 8080,
            IngressExternal: true,
            TransportMethod: "http2",
            ReadinessProbePath: "/healthz/ready",
            ReadinessProbePort: 9000,
            LivenessProbePath: "/healthz/live",
            LivenessProbePort: 9001,
            StartupProbePath: "/healthz/startup",
            StartupProbePort: 9002,
            ContainerRegistryServiceConnection: ContainerRegistryServiceConnection));

        // Assert
        sut.CpuCores.Should().Be("1.0");
        sut.MemoryGi.Should().Be("2.0Gi");
        sut.MinReplicas.Should().Be(2);
        sut.MaxReplicas.Should().Be(10);
        sut.IngressEnabled.Should().BeTrue();
        sut.IngressTargetPort.Should().Be(8080);
        sut.IngressExternal.Should().BeTrue();
        sut.TransportMethod.Should().Be("http2");
        sut.ReadinessProbePath.Should().Be("/healthz/ready");
        sut.ReadinessProbePort.Should().Be(9000);
        sut.LivenessProbePath.Should().Be("/healthz/live");
        sut.LivenessProbePort.Should().Be(9001);
        sut.StartupProbePath.Should().Be("/healthz/startup");
        sut.StartupProbePort.Should().Be(9002);
        sut.ContainerRegistryServiceConnection.Should().Be(ContainerRegistryServiceConnection);
    }

    [Fact]
    public void Given_PipelineOnlyServiceConnection_When_ToDictionary_Then_DoesNotExposeItAsBicepOverride()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            new ContainerAppEnvironmentSettingsData(
                EnvironmentName,
                ContainerRegistryServiceConnection: ContainerRegistryServiceConnection));

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            new ContainerAppEnvironmentSettingsData(EnvironmentName));

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            new ContainerAppEnvironmentSettingsData(
                EnvironmentName,
                CpuCores: "0.5",
                MemoryGi: "1Gi",
                MinReplicas: 1,
                MaxReplicas: 3,
                IngressEnabled: true,
                IngressTargetPort: 80,
                IngressExternal: false,
                TransportMethod: "http",
                ReadinessProbePath: "/ready",
                ReadinessProbePort: 8080,
                LivenessProbePath: "/live",
                LivenessProbePort: 8081,
                StartupProbePath: "/startup",
                StartupProbePort: 8082));

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["cpuCores"].Should().Be("0.5");
        dict["memoryGi"].Should().Be("1Gi");
        dict["minReplicas"].Should().Be("1");
        dict["maxReplicas"].Should().Be("3");
        dict["ingressEnabled"].Should().Be("true");
        dict["ingressTargetPort"].Should().Be("80");
        dict["ingressExternal"].Should().Be("false");
        dict["transportMethod"].Should().Be("http");
        dict["readinessProbePath"].Should().Be("/ready");
        dict["readinessProbePort"].Should().Be("8080");
        dict["livenessProbePath"].Should().Be("/live");
        dict["livenessProbePort"].Should().Be("8081");
        dict["startupProbePath"].Should().Be("/startup");
        dict["startupProbePort"].Should().Be("8082");
    }
}
