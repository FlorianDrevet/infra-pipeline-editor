using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ContainerAppAggregate.Entities;

public sealed class ContainerAppEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var containerAppId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerAppEnvironmentSettings.Create(
            containerAppId,
            EnvironmentName,
            cpuCores: "0.5",
            memoryGi: "1.0Gi",
            minReplicas: 1,
            maxReplicas: 3,
            ingressEnabled: true,
            ingressTargetPort: 80,
            ingressExternal: true,
            transportMethod: "http",
            readinessProbePath: "/ready",
            readinessProbePort: 8080,
            livenessProbePath: "/live",
            livenessProbePort: 8081,
            startupProbePath: "/startup",
            startupProbePort: 8082);

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
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            cpuCores: "0.25", memoryGi: "0.5Gi", minReplicas: 0, maxReplicas: 1,
            ingressEnabled: false, ingressTargetPort: null, ingressExternal: false,
            transportMethod: "auto");

        // Act
        sut.Update(
            cpuCores: "1.0", memoryGi: "2.0Gi", minReplicas: 2, maxReplicas: 10,
            ingressEnabled: true, ingressTargetPort: 8080, ingressExternal: true,
            transportMethod: "http2",
            readinessProbePath: "/healthz/ready", readinessProbePort: 9000,
            livenessProbePath: "/healthz/live", livenessProbePort: 9001,
            startupProbePath: "/healthz/startup", startupProbePort: 9002);

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
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ContainerAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            cpuCores: null, memoryGi: null, minReplicas: null, maxReplicas: null,
            ingressEnabled: null, ingressTargetPort: null, ingressExternal: null,
            transportMethod: null);

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
            AzureResourceId.CreateUnique(), EnvironmentName,
            cpuCores: "0.5", memoryGi: "1Gi", minReplicas: 1, maxReplicas: 3,
            ingressEnabled: true, ingressTargetPort: 80, ingressExternal: false,
            transportMethod: "http",
            readinessProbePath: "/ready", readinessProbePort: 8080,
            livenessProbePath: "/live", livenessProbePort: 8081,
            startupProbePath: "/startup", startupProbePort: 8082);

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
