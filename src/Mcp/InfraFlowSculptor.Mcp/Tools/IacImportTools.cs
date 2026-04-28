using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Models;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for importing Infrastructure-as-Code templates into InfraFlowSculptor projects.
/// </summary>
[McpServerToolType]
public sealed class IacImportTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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
        if (!string.Equals(sourceFormat, "arm-json", StringComparison.OrdinalIgnoreCase))
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
                summary = $"Source format '{sourceFormat}' is not supported in V1. Supported formats: arm-json.",
            }, JsonOptions);
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
    /// For V1, creates the project skeleton; resources must be added separately via API or frontend.
    /// </summary>
    [McpServerTool(Name = "apply_import_preview")]
    [Description("Applies a validated import preview to create a new project with the imported resources.")]
    public static async Task<string> ApplyImportPreview(
        IImportPreviewService previewService,
        IProjectDraftService draftService,
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
        var repositories = BuildRepositoriesForLayout(layoutPreset);

        var command = new CreateProjectWithSetupCommand(
            Name: projectName,
            Description: $"Imported from ARM template ({preview.ProjectDefinition.Resources.Count} resources detected)",
            LayoutPreset: layoutPreset,
            Environments: envItems,
            Repositories: repositories);

        var result = await mediator.Send(command);

        if (result.IsError)
        {
            return JsonError("creation_failed", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        previewService.RemovePreview(previewId);

        var mappedResources = preview.ProjectDefinition.Resources
            .Where(r => r.MappedResourceType is not null)
            .Select(r => new { r.SourceName, r.MappedResourceType, r.MappedName })
            .ToList();

        return JsonSerializer.Serialize(new
        {
            status = "created",
            projectId = result.Value.Id.Value.ToString(),
            projectName = result.Value.Name.Value,
            importedResourceCount = mappedResources.Count,
            mappedResources,
            nextSuggestedActions = new[]
            {
                "Add the detected resources to the project via the API or frontend.",
                "Configure environment-specific settings for each resource.",
                "Generate Bicep files using 'generate_project_bicep' once resources are added.",
            },
        }, JsonOptions);
    }

    private static string SerializePreview(ImportPreview preview)
    {
        var mapped = preview.ProjectDefinition.Resources
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

        var gapsList = preview.Gaps.Select(g => new
        {
            severity = g.Severity.ToString().ToLowerInvariant(),
            g.Category,
            g.Message,
            g.SourceResourceName,
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            preview.PreviewId,
            sourceFormat = preview.ProjectDefinition.SourceFormat,
            parsedResourceCount = preview.ProjectDefinition.Resources.Count,
            mappedResources = mapped,
            gaps = gapsList,
            unsupportedResources = preview.UnsupportedResources,
            dependencies = preview.ProjectDefinition.Dependencies.Select(d => new
            {
                d.FromResourceName,
                d.ToResourceName,
                d.DependencyType,
            }),
            metadata = preview.ProjectDefinition.Metadata,
            summary = $"Parsed {preview.ProjectDefinition.Resources.Count} resource(s): " +
                      $"{mapped.Count} mapped, {preview.UnsupportedResources.Count} unsupported.",
        }, JsonOptions);
    }

    private static IReadOnlyList<EnvironmentSetupItem> ParseEnvironments(string? environments)
    {
        if (string.IsNullOrWhiteSpace(environments))
        {
            return
            [
                new EnvironmentSetupItem(
                    "Development", "dev", string.Empty, string.Empty,
                    "westeurope", Guid.Empty, 0, false),
            ];
        }

        try
        {
            using var doc = JsonDocument.Parse(environments);
            var items = new List<EnvironmentSetupItem>();
            var order = 0;

            foreach (var env in doc.RootElement.EnumerateArray())
            {
                items.Add(new EnvironmentSetupItem(
                    Name: env.TryGetProperty("name", out var n) ? n.GetString() ?? "Environment" : "Environment",
                    ShortName: env.TryGetProperty("shortName", out var sn) ? sn.GetString() ?? "env" : "env",
                    Prefix: env.TryGetProperty("prefix", out var p) ? p.GetString() ?? string.Empty : string.Empty,
                    Suffix: env.TryGetProperty("suffix", out var s) ? s.GetString() ?? string.Empty : string.Empty,
                    Location: env.TryGetProperty("location", out var l) ? l.GetString() ?? "westeurope" : "westeurope",
                    SubscriptionId: env.TryGetProperty("subscriptionId", out var sub)
                        && Guid.TryParse(sub.GetString(), out var subGuid)
                            ? subGuid
                            : Guid.Empty,
                    Order: order++,
                    RequiresApproval: env.TryGetProperty("requiresApproval", out var ra) && ra.GetBoolean()));
            }

            return items.Count > 0
                ? items
                :
                [
                    new EnvironmentSetupItem(
                        "Development", "dev", string.Empty, string.Empty,
                        "westeurope", Guid.Empty, 0, false),
                ];
        }
        catch (JsonException)
        {
            return
            [
                new EnvironmentSetupItem(
                    "Development", "dev", string.Empty, string.Empty,
                    "westeurope", Guid.Empty, 0, false),
            ];
        }
    }

    private static IReadOnlyList<RepositorySetupItem> BuildRepositoriesForLayout(string layoutPreset)
    {
        return layoutPreset switch
        {
            "AllInOne" =>
            [
                new RepositorySetupItem("main", ["Infrastructure", "Application"], null, null, null),
            ],
            "SplitInfraCode" =>
            [
                new RepositorySetupItem("infra", ["Infrastructure"], null, null, null),
                new RepositorySetupItem("app", ["Application"], null, null, null),
            ],
            "MultiRepo" => [],
            _ =>
            [
                new RepositorySetupItem("main", ["Infrastructure", "Application"], null, null, null),
            ],
        };
    }

    private static string JsonError(string code, string message) =>
        JsonSerializer.Serialize(new { error = code, message }, JsonOptions);
}
