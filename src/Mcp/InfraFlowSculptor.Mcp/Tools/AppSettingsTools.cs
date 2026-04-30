using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;
using InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for managing app settings (environment variables) on compute resources
/// (ContainerApp, WebApp, FunctionApp).
/// </summary>
[McpServerToolType]
public sealed class AppSettingsTools
{
    private AppSettingsTools() { }

    /// <summary>
    /// Adds a static app setting (environment variable) to a compute resource with per-environment values.
    /// </summary>
    [McpServerTool(Name = "add_app_setting")]
    [Description(
        "Adds an app setting (environment variable) to a compute resource (ContainerApp, WebApp, FunctionApp). " +
        "Provide per-environment values as a JSON object: {\"dev\": \"value1\", \"stg\": \"value2\", \"prod\": \"value3\"}. " +
        "Use environment short names as keys.")]
    public static async Task<string> AddAppSetting(
        ISender mediator,
        [Description("The resource ID (GUID) of the compute resource.")] string resourceId,
        [Description("The app setting name (e.g. 'ASPNETCORE_ENVIRONMENT', 'DATABASE_URL').")] string name,
        [Description("JSON object mapping environment short names to values, e.g. {\"dev\": \"Development\", \"prod\": \"Production\"}.")] string environmentValues)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error("invalid_resource_id", "The resourceId must be a valid GUID.");
        }

        var values = DeserializeEnvValues(environmentValues);
        if (values is null)
        {
            return McpJsonDefaults.Error("invalid_environment_values", "Failed to parse environmentValues as a JSON object of string key-value pairs.");
        }

        var command = new AddAppSettingCommand(
            ResourceId: AzureResourceId.Create(id),
            Name: name,
            EnvironmentValues: values,
            SourceResourceId: null,
            SourceOutputName: null,
            KeyVaultResourceId: null,
            SecretName: null);

        var result = await mediator.Send(command);

        return result.Match(
            appSetting => JsonSerializer.Serialize(new
            {
                status = "success",
                appSettingId = appSetting.Id.Value.ToString(),
                name = appSetting.Name,
                environmentCount = values.Count,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Adds an output-reference app setting that wires a value from another resource's output.
    /// </summary>
    [McpServerTool(Name = "add_output_reference_app_setting")]
    [Description(
        "Adds an app setting that references the output of another Azure resource. " +
        "Use this to wire connection strings, endpoints, or keys from one resource to another.")]
    public static async Task<string> AddOutputReferenceAppSetting(
        ISender mediator,
        [Description("The resource ID (GUID) of the compute resource receiving the setting.")] string resourceId,
        [Description("The app setting name (e.g. 'REDIS_CONNECTION_STRING').")] string name,
        [Description("The source resource ID (GUID) providing the output.")] string sourceResourceId,
        [Description("The output name from the source resource (e.g. 'PrimaryConnectionString', 'PrimaryKey').")] string sourceOutputName)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error("invalid_resource_id", "The resourceId must be a valid GUID.");
        }

        if (!Guid.TryParse(sourceResourceId, out var sourceId))
        {
            return McpJsonDefaults.Error("invalid_source_resource_id", "The sourceResourceId must be a valid GUID.");
        }

        var command = new AddAppSettingCommand(
            ResourceId: AzureResourceId.Create(id),
            Name: name,
            EnvironmentValues: null,
            SourceResourceId: AzureResourceId.Create(sourceId),
            SourceOutputName: sourceOutputName,
            KeyVaultResourceId: null,
            SecretName: null);

        var result = await mediator.Send(command);

        return result.Match(
            appSetting => JsonSerializer.Serialize(new
            {
                status = "success",
                appSettingId = appSetting.Id.Value.ToString(),
                name = appSetting.Name,
                sourceResourceId,
                sourceOutputName,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Lists all app settings configured on a resource.
    /// </summary>
    [McpServerTool(Name = "list_app_settings")]
    [Description("Lists all app settings (environment variables) configured on a compute resource.")]
    public static async Task<string> ListAppSettings(
        ISender mediator,
        [Description("The resource ID (GUID) of the compute resource.")] string resourceId)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error("invalid_resource_id", "The resourceId must be a valid GUID.");
        }

        var query = new ListAppSettingsQuery(AzureResourceId.Create(id));
        var result = await mediator.Send(query);

        return result.Match(
            settings => JsonSerializer.Serialize(new
            {
                resourceId,
                count = settings.Count,
                appSettings = settings.Select(s => new
                {
                    id = s.Id.Value.ToString(),
                    name = s.Name,
                    isOutputReference = s.IsOutputReference,
                    isKeyVaultReference = s.IsKeyVaultReference,
                    environmentValues = s.EnvironmentValues,
                }),
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("query_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Removes an app setting from a resource.
    /// </summary>
    [McpServerTool(Name = "remove_app_setting")]
    [Description("Removes an app setting by its ID from a compute resource.")]
    public static async Task<string> RemoveAppSetting(
        ISender mediator,
        [Description("The resource ID (GUID) of the compute resource.")] string resourceId,
        [Description("The app setting ID (GUID) to remove.")] string appSettingId)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error("invalid_resource_id", "The resourceId must be a valid GUID.");
        }

        if (!Guid.TryParse(appSettingId, out var settingId))
        {
            return McpJsonDefaults.Error("invalid_app_setting_id", "The appSettingId must be a valid GUID.");
        }

        var command = new RemoveAppSettingCommand(
            AzureResourceId.Create(id),
            AppSettingId.Create(settingId));

        var result = await mediator.Send(command);

        return result.Match(
            _ => JsonSerializer.Serialize(new { status = "success", message = "App setting removed." }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static IReadOnlyDictionary<string, string>? DeserializeEnvValues(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
