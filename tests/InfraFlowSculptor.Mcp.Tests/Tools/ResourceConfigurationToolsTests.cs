using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

/// <summary>
/// Unit tests for <see cref="ResourceConfigurationTools"/>.
/// </summary>
public sealed class ResourceConfigurationToolsTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    // ── Invalid GUID ───────────────────────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_InvalidResourceId_ReturnsError()
    {
        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, "not-a-guid", "KeyVault", "[]");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_resource_id");
    }

    // ── Unsupported resource type ──────────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_UnsupportedResourceType_ReturnsError()
    {
        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, Guid.NewGuid().ToString(), "UnsupportedType", "[]");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("unsupported_resource_type");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("UnsupportedType");
    }

    // ── KeyVault: success ──────────────────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_KeyVault_ValidSettings_ReturnsSuccess()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var azureResourceId = AzureResourceId.Create(resourceGuid);

        var currentKeyVault = new KeyVaultResult(
            azureResourceId,
            new ResourceGroupId(Guid.NewGuid()),
            new Name("my-kv"),
            new Location(Location.LocationEnum.WestEurope),
            EnableRbacAuthorization: true,
            EnabledForDeployment: false,
            EnabledForDiskEncryption: false,
            EnabledForTemplateDeployment: false,
            EnablePurgeProtection: true,
            EnableSoftDelete: true,
            EnvironmentSettings: []);

        _mediator
            .Send(Arg.Any<GetKeyVaultQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(currentKeyVault));

        _mediator
            .Send(Arg.Any<UpdateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(currentKeyVault));

        const string settingsJson = """[{"environmentName":"dev","sku":"Standard"},{"environmentName":"prod","sku":"Premium"}]""";

        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, resourceGuid.ToString(), "KeyVault", settingsJson);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("KeyVault");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("2 environment(s)");
    }

    // ── KeyVault: resource not found ───────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_KeyVault_NotFound_ReturnsError()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<GetKeyVaultQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(
                Error.NotFound("NOT_FOUND", "Key Vault not found.")));

        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, resourceGuid.ToString(), "KeyVault", """[{"environmentName":"dev","sku":"Standard"}]""");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("resource_not_found");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Key Vault not found.");
    }

    // ── KeyVault: invalid JSON settings ────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_KeyVault_InvalidJson_ReturnsError()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var azureResourceId = AzureResourceId.Create(resourceGuid);

        var currentKeyVault = new KeyVaultResult(
            azureResourceId,
            new ResourceGroupId(Guid.NewGuid()),
            new Name("my-kv"),
            new Location(Location.LocationEnum.WestEurope),
            EnableRbacAuthorization: true,
            EnabledForDeployment: false,
            EnabledForDiskEncryption: false,
            EnabledForTemplateDeployment: false,
            EnablePurgeProtection: true,
            EnableSoftDelete: true,
            EnvironmentSettings: []);

        _mediator
            .Send(Arg.Any<GetKeyVaultQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(currentKeyVault));

        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, resourceGuid.ToString(), "KeyVault", "not-valid-json");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_settings");
    }

    // ── ContainerApp: success ──────────────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_ContainerApp_ValidSettings_ReturnsSuccess()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var azureResourceId = AzureResourceId.Create(resourceGuid);

        var currentContainerApp = new ContainerAppResult(
            azureResourceId,
            new ResourceGroupId(Guid.NewGuid()),
            new Name("my-app"),
            new Location(Location.LocationEnum.WestEurope),
            ContainerAppEnvironmentId: Guid.NewGuid(),
            ContainerRegistryId: null,
            AcrAuthMode: null,
            AcrPullIdentityId: null,
            DockerImageName: null,
            DockerImageValidated: false,
            DockerfilePath: null,
            ApplicationName: null,
            PipelineStepOptions: null,
            EnvironmentSettings: []);

        _mediator
            .Send(Arg.Any<GetContainerAppQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppResult>>(currentContainerApp));

        _mediator
            .Send(Arg.Any<UpdateContainerAppCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppResult>>(currentContainerApp));

        const string settingsJson = """[{"environmentName":"dev","cpuCores":"0.25","memoryGi":"0.5","minReplicas":1,"maxReplicas":3}]""";

        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, resourceGuid.ToString(), "ContainerApp", settingsJson);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("ContainerApp");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("1 environment(s)");
    }

    // ── ContainerApp: update error ─────────────────────────────────────

    [Fact]
    public async Task SetResourceEnvironmentSettings_ContainerApp_UpdateFails_ReturnsError()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var azureResourceId = AzureResourceId.Create(resourceGuid);

        var currentContainerApp = new ContainerAppResult(
            azureResourceId,
            new ResourceGroupId(Guid.NewGuid()),
            new Name("my-app"),
            new Location(Location.LocationEnum.WestEurope),
            ContainerAppEnvironmentId: Guid.NewGuid(),
            ContainerRegistryId: null,
            AcrAuthMode: null,
            AcrPullIdentityId: null,
            DockerImageName: null,
            DockerImageValidated: false,
            DockerfilePath: null,
            ApplicationName: null,
            PipelineStepOptions: null,
            EnvironmentSettings: []);

        _mediator
            .Send(Arg.Any<GetContainerAppQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppResult>>(currentContainerApp));

        _mediator
            .Send(Arg.Any<UpdateContainerAppCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppResult>>(
                Error.Failure("UPD_FAIL", "Validation failed.")));

        const string settingsJson = """[{"environmentName":"dev","cpuCores":"0.25","memoryGi":"0.5"}]""";

        // Act
        var json = await ResourceConfigurationTools.SetResourceEnvironmentSettings(
            _mediator, resourceGuid.ToString(), "ContainerApp", settingsJson);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("update_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Validation failed.");
    }
}
