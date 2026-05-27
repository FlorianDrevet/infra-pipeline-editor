using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

/// <summary>
/// Unit tests for <see cref="AppSettingsTools"/>.
/// </summary>
public sealed class AppSettingsToolsTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    // ── AddAppSetting ──────────────────────────────────────────────────

    [Fact]
    public async Task AddAppSetting_InvalidResourceId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.AddAppSetting(
            _mediator, "not-a-guid", "MY_VAR", """{"dev":"val"}""");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_resource_id");
    }

    [Fact]
    public async Task AddAppSetting_InvalidEnvironmentValues_ReturnsError()
    {
        // Arrange
        var resourceId = Guid.NewGuid().ToString();

        // Act
        var json = await AppSettingsTools.AddAppSetting(
            _mediator, resourceId, "MY_VAR", "not-json");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_environment_values");
    }

    [Fact]
    public async Task AddAppSetting_ValidInput_SendsCommandAndReturnsSuccess()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var settingGuid = Guid.NewGuid();
        var appSettingResult = new AppSettingResult(
            new AppSettingId(settingGuid),
            AzureResourceId.Create(resourceGuid),
            "MY_VAR",
            new Dictionary<string, string> { ["dev"] = "val1", ["prod"] = "val2" },
            SourceResourceId: null,
            SourceOutputName: null,
            IsOutputReference: false,
            KeyVaultResourceId: null,
            SecretName: null,
            IsKeyVaultReference: false,
            HasKeyVaultAccess: null,
            SecretValueAssignment: null,
            VariableGroupId: null,
            PipelineVariableName: null,
            VariableGroupName: null,
            IsViaVariableGroup: false);

        _mediator
            .Send(Arg.Any<AddAppSettingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppSettingResult>>(appSettingResult));

        // Act
        var json = await AppSettingsTools.AddAppSetting(
            _mediator, resourceGuid.ToString(), "MY_VAR", """{"dev":"val1","prod":"val2"}""");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("appSettingId").GetString().Should().Be(settingGuid.ToString());
        doc.RootElement.GetProperty("name").GetString().Should().Be("MY_VAR");
        doc.RootElement.GetProperty("environmentCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task AddAppSetting_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<AddAppSettingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppSettingResult>>(
                Error.Failure("FAIL", "Resource not found.")));

        // Act
        var json = await AppSettingsTools.AddAppSetting(
            _mediator, resourceGuid.ToString(), "MY_VAR", """{"dev":"val"}""");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Resource not found.");
    }

    // ── AddOutputReferenceAppSetting ───────────────────────────────────

    [Fact]
    public async Task AddOutputReferenceAppSetting_InvalidResourceId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.AddOutputReferenceAppSetting(
            _mediator, "bad", "MY_VAR", Guid.NewGuid().ToString(), "PrimaryKey");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_resource_id");
    }

    [Fact]
    public async Task AddOutputReferenceAppSetting_InvalidSourceResourceId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.AddOutputReferenceAppSetting(
            _mediator, Guid.NewGuid().ToString(), "MY_VAR", "bad-source", "PrimaryKey");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_source_resource_id");
    }

    [Fact]
    public async Task AddOutputReferenceAppSetting_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var sourceGuid = Guid.NewGuid();
        var settingGuid = Guid.NewGuid();
        var appSettingResult = new AppSettingResult(
            new AppSettingId(settingGuid),
            AzureResourceId.Create(resourceGuid),
            "REDIS_CONN",
            EnvironmentValues: null,
            SourceResourceId: AzureResourceId.Create(sourceGuid),
            SourceOutputName: "PrimaryConnectionString",
            IsOutputReference: true,
            KeyVaultResourceId: null,
            SecretName: null,
            IsKeyVaultReference: false,
            HasKeyVaultAccess: null,
            SecretValueAssignment: null,
            VariableGroupId: null,
            PipelineVariableName: null,
            VariableGroupName: null,
            IsViaVariableGroup: false);

        _mediator
            .Send(Arg.Any<AddAppSettingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppSettingResult>>(appSettingResult));

        // Act
        var json = await AppSettingsTools.AddOutputReferenceAppSetting(
            _mediator, resourceGuid.ToString(), "REDIS_CONN",
            sourceGuid.ToString(), "PrimaryConnectionString");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("sourceResourceId").GetString().Should().Be(sourceGuid.ToString());
        doc.RootElement.GetProperty("sourceOutputName").GetString().Should().Be("PrimaryConnectionString");
    }

    // ── ListAppSettings ────────────────────────────────────────────────

    [Fact]
    public async Task ListAppSettings_InvalidResourceId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.ListAppSettings(_mediator, "invalid");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_resource_id");
    }

    [Fact]
    public async Task ListAppSettings_ValidResourceId_ReturnsSettingsList()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var settings = new List<AppSettingResult>
        {
            new(
                new AppSettingId(Guid.NewGuid()),
                AzureResourceId.Create(resourceGuid),
                "VAR_A",
                new Dictionary<string, string> { ["dev"] = "a" },
                SourceResourceId: null, SourceOutputName: null, IsOutputReference: false,
                KeyVaultResourceId: null, SecretName: null, IsKeyVaultReference: false,
                HasKeyVaultAccess: null, SecretValueAssignment: null,
                VariableGroupId: null, PipelineVariableName: null, VariableGroupName: null,
                IsViaVariableGroup: false),
            new(
                new AppSettingId(Guid.NewGuid()),
                AzureResourceId.Create(resourceGuid),
                "VAR_B",
                EnvironmentValues: null,
                SourceResourceId: AzureResourceId.Create(Guid.NewGuid()),
                SourceOutputName: "Key", IsOutputReference: true,
                KeyVaultResourceId: null, SecretName: null, IsKeyVaultReference: false,
                HasKeyVaultAccess: null, SecretValueAssignment: null,
                VariableGroupId: null, PipelineVariableName: null, VariableGroupName: null,
                IsViaVariableGroup: false),
        };

        _mediator
            .Send(Arg.Any<ListAppSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<IReadOnlyList<AppSettingResult>>>(settings));

        // Act
        var json = await AppSettingsTools.ListAppSettings(_mediator, resourceGuid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("count").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("appSettings").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ListAppSettings_MediatRError_ReturnsQueryFailed()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<ListAppSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<IReadOnlyList<AppSettingResult>>>(
                Error.NotFound("NOT_FOUND", "Resource does not exist.")));

        // Act
        var json = await AppSettingsTools.ListAppSettings(_mediator, resourceGuid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("query_failed");
    }

    // ── RemoveAppSetting ───────────────────────────────────────────────

    [Fact]
    public async Task RemoveAppSetting_InvalidResourceId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.RemoveAppSetting(
            _mediator, "bad", Guid.NewGuid().ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_resource_id");
    }

    [Fact]
    public async Task RemoveAppSetting_InvalidAppSettingId_ReturnsError()
    {
        // Act
        var json = await AppSettingsTools.RemoveAppSetting(
            _mediator, Guid.NewGuid().ToString(), "bad-setting-id");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_app_setting_id");
    }

    [Fact]
    public async Task RemoveAppSetting_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var settingGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<RemoveAppSettingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(Result.Deleted));

        // Act
        var json = await AppSettingsTools.RemoveAppSetting(
            _mediator, resourceGuid.ToString(), settingGuid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
    }

    [Fact]
    public async Task RemoveAppSetting_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var resourceGuid = Guid.NewGuid();
        var settingGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<RemoveAppSettingCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(
                Error.NotFound("NOT_FOUND", "App setting not found.")));

        // Act
        var json = await AppSettingsTools.RemoveAppSetting(
            _mediator, resourceGuid.ToString(), settingGuid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("App setting not found.");
    }
}
