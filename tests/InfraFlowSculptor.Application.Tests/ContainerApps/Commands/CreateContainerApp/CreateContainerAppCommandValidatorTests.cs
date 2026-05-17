using FluentAssertions;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Commands.CreateContainerApp;

public sealed class CreateContainerAppCommandValidatorTests
{
    private readonly CreateContainerAppCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = CreateCommand() with { Name = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerAppCommand.Name));
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = CreateCommand() with { ResourceGroupId = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerAppCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyContainerAppEnvironmentId_When_Validate_Then_FailsOnContainerAppEnvironmentId()
    {
        // Arrange
        var command = CreateCommand(containerAppEnvironmentId: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerAppCommand.ContainerAppEnvironmentId));
    }

    [Fact]
    public void Given_InvalidReadinessProbePort_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(readinessProbePort: 0),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ReadinessProbePort"));
    }

    [Fact]
    public void Given_InvalidReadinessProbePath_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(readinessProbePath: "health"),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ReadinessProbePath"));
    }

    [Fact]
    public void Given_InvalidLivenessProbePort_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(livenessProbePort: 70000),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("LivenessProbePort"));
    }

    [Fact]
    public void Given_InvalidLivenessProbePath_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(livenessProbePath: "alive"),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("LivenessProbePath"));
    }

    [Fact]
    public void Given_InvalidStartupProbePort_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(startupProbePort: -1),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("StartupProbePort"));
    }

    [Fact]
    public void Given_InvalidStartupProbePath_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateCommand(environmentSettings:
        [
            CreateEnvConfig(startupProbePath: "startup"),
        ]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("StartupProbePath"));
    }

    private static CreateContainerAppCommand CreateCommand(
        Guid? containerAppEnvironmentId = null,
        IReadOnlyList<ContainerAppEnvironmentConfigData>? environmentSettings = null)
    {
        return new CreateContainerAppCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-container-app"),
            new Location(Location.LocationEnum.WestEurope),
            containerAppEnvironmentId ?? Guid.NewGuid(),
            ContainerRegistryId: null,
            EnvironmentSettings: environmentSettings);
    }

    private static ContainerAppEnvironmentConfigData CreateEnvConfig(
        int? readinessProbePort = null,
        string? readinessProbePath = null,
        int? livenessProbePort = null,
        string? livenessProbePath = null,
        int? startupProbePort = null,
        string? startupProbePath = null)
    {
        return new ContainerAppEnvironmentConfigData(
            EnvironmentName: "dev",
            CpuCores: null,
            MemoryGi: null,
            MinReplicas: null,
            MaxReplicas: null,
            IngressEnabled: null,
            IngressTargetPort: null,
            IngressExternal: null,
            TransportMethod: null,
            ReadinessProbePath: readinessProbePath,
            ReadinessProbePort: readinessProbePort,
            LivenessProbePath: livenessProbePath,
            LivenessProbePort: livenessProbePort,
            StartupProbePath: startupProbePath,
            StartupProbePort: startupProbePort);
    }
}
