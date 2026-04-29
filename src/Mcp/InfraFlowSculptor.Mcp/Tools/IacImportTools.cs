using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.Imports;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for importing Infrastructure-as-Code templates into InfraFlowSculptor projects.
/// </summary>
[McpServerToolType]
public sealed class IacImportTools
{
    private IacImportTools() { }

    /// <summary>
    /// Analyzes an IaC source (ARM JSON for V1) and produces a preview showing
    /// mapped resources, gaps, and unsupported elements.
    /// </summary>
    [McpServerTool(Name = "preview_iac_import")]
    [Description("Analyzes an IaC source (ARM JSON for V1) and produces a preview showing mapped resources, gaps, and unsupported elements.")]
    public static string PreviewIacImport(
        IImportPreviewService previewService,
        [Description("Source format. V1 supports: 'arm-json'.")] string sourceFormat,
        [Description("The raw content of the IaC source file.")] string sourceContent)
    {
        if (!string.Equals(sourceFormat, IacSourceFormat.ArmJson, StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                previewId = (string?)null,
                sourceFormat,
                parsedResourceCount = 0,
                mappedResources = Array.Empty<object>(),
                gaps = Array.Empty<object>(),
                unsupportedResources = Array.Empty<object>(),
                suggestedProjectStructure = (object?)null,
                summary = $"Source format '{sourceFormat}' is not supported in V1. Supported formats: {IacSourceFormat.ArmJson}.",
            }, McpJsonDefaults.SerializerOptions);
        }

        try
        {
            var preview = previewService.CreatePreviewFromArm(sourceContent);
            return SerializePreview(preview);
        }
        catch (JsonException ex)
        {
            return JsonError("invalid_json", $"Failed to parse source content: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies a validated import preview to create a new project with the imported resources.
    /// Creates the project, infrastructure config, resource group, and all mapped resources.
    /// </summary>
    [McpServerTool(Name = "apply_import_preview")]
    [Description("Applies a validated import preview to create a new project with the imported resources.")]
    public static async Task<string> ApplyImportPreview(
        IImportPreviewService previewService,
        ISender mediator,
        [Description("The preview ID from preview_iac_import.")] string previewId,
        [Description("Project name for the new project.")] string projectName,
        [Description("Layout preset (AllInOne, SplitInfraCode, MultiRepo).")] string layoutPreset,
        [Description("Optional JSON array of environment definitions.")] string? environments = null,
        [Description("Optional list of source resource names to include. If null, all mapped resources are imported.")] string? resourceFilter = null)
    {
        var preview = previewService.GetPreview(previewId);
        if (preview is null)
        {
            return JsonError("preview_not_found", $"Import preview '{previewId}' not found.");
        }

        var envItems = ParseEnvironments(environments);
        var filter = ParseResourceFilter(resourceFilter)?.ToList();
        var command = new ApplyImportPreviewCommand(
            projectName,
            layoutPreset,
            preview.Analysis,
            envItems,
            filter);

        var result = await mediator.Send(command);

        if (result.IsError)
        {
            return JsonError("creation_failed", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        previewService.RemovePreview(previewId);

        return JsonSerializer.Serialize(new
        {
            status = result.Value.Status,
            previewId,
            projectId = result.Value.ProjectId,
            projectName = result.Value.ProjectName,
            infrastructureConfigId = result.Value.InfrastructureConfigId,
            resourceGroupId = result.Value.ResourceGroupId,
            infrastructureError = result.Value.InfrastructureError,
            createdResources = result.Value.CreatedResources.Select(resource => new
            {
                resource.ResourceType,
                resource.ResourceId,
                resource.Name,
            }),
            skippedResources = result.Value.SkippedResources.Select(resource => new
            {
                resource.ResourceType,
                resource.Name,
                resource.Reason,
            }),
            nextSuggestedActions = result.Value.NextSuggestedActions,
        }, McpJsonDefaults.SerializerOptions);
    }

    private static string SerializePreview(Imports.Models.ImportPreview preview)
    {
        var mapped = preview.Analysis.Resources
            .Where(r => r.MappedResourceType is not null)
            .Select(r => new
            {
                r.SourceType,
                r.SourceName,
                r.MappedResourceType,
                r.MappedName,
                confidence = r.Confidence.ToString().ToLowerInvariant(),
                extractedProperties = r.ExtractedProperties,
                unmappedProperties = r.UnmappedProperties,
            })
            .ToList();

        var gapsList = preview.Analysis.Gaps.Select(g => new
        {
            severity = g.Severity.ToString().ToLowerInvariant(),
            g.Category,
            g.Message,
            g.SourceResourceName,
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            preview.PreviewId,
            sourceFormat = preview.Analysis.SourceFormat,
            parsedResourceCount = preview.Analysis.Resources.Count,
            mappedResources = mapped,
            gaps = gapsList,
            unsupportedResources = preview.Analysis.UnsupportedResources,
            dependencies = preview.Analysis.Dependencies.Select(d => new
            {
                d.FromResourceName,
                d.ToResourceName,
                d.DependencyType,
            }),
            metadata = preview.Analysis.Metadata,
            summary = $"Parsed {preview.Analysis.Resources.Count} resource(s): " +
                      $"{mapped.Count} mapped, {preview.Analysis.UnsupportedResources.Count} unsupported.",
        }, McpJsonDefaults.SerializerOptions);
    }

    private const string DefaultLocation = "westeurope";

    private static readonly IReadOnlyList<EnvironmentSetupItem> DefaultEnvironments =
    [
        new EnvironmentSetupItem("Development", "dev", string.Empty, string.Empty, DefaultLocation, Guid.Empty, 0, false),
    ];

    private static IReadOnlyList<EnvironmentSetupItem> ParseEnvironments(string? environments)
    {
        if (string.IsNullOrWhiteSpace(environments))
            return DefaultEnvironments;

        try
        {
            using var doc = JsonDocument.Parse(environments);
            var items = new List<EnvironmentSetupItem>();
            var order = 0;

            foreach (var env in doc.RootElement.EnumerateArray())
            {
                items.Add(ParseEnvironmentItem(env, ref order));
            }

            return items.Count > 0 ? items : DefaultEnvironments;
        }
        catch (JsonException)
        {
            return DefaultEnvironments;
        }
    }

    private static EnvironmentSetupItem ParseEnvironmentItem(JsonElement env, ref int order)
    {
        return new EnvironmentSetupItem(
            Name: env.TryGetProperty("name", out var n) ? n.GetString() ?? "Environment" : "Environment",
            ShortName: env.TryGetProperty("shortName", out var sn) ? sn.GetString() ?? "env" : "env",
            Prefix: env.TryGetProperty("prefix", out var p) ? p.GetString() ?? string.Empty : string.Empty,
            Suffix: env.TryGetProperty("suffix", out var s) ? s.GetString() ?? string.Empty : string.Empty,
            Location: env.TryGetProperty("location", out var l) ? l.GetString() ?? DefaultLocation : DefaultLocation,
            SubscriptionId: env.TryGetProperty("subscriptionId", out var sub)
                && Guid.TryParse(sub.GetString(), out var subGuid)
                    ? subGuid
                    : Guid.Empty,
            Order: order++,
            RequiresApproval: env.TryGetProperty("requiresApproval", out var ra) && ra.GetBoolean());
    }

    private static HashSet<string>? ParseResourceFilter(string? resourceFilter)
    {
        if (string.IsNullOrWhiteSpace(resourceFilter))
            return null;

        try
        {
            var names = JsonSerializer.Deserialize<string[]>(resourceFilter, McpJsonDefaults.SerializerOptions);
            return names is not null ? new HashSet<string>(names, StringComparer.OrdinalIgnoreCase) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string JsonError(string code, string message) =>
        McpJsonDefaults.Error(code, message);
}
