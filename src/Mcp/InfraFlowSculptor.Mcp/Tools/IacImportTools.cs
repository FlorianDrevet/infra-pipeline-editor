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
    /// Creates the project, infrastructure config, resource group, and all mapped resources.
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

        var projectId = result.Value.Id.Value;
        var createdProjectName = result.Value.Name.Value;

        // Build resource inputs from the import preview (only mapped resources).
        var filterSet = ParseResourceFilter(resourceFilter);
        var resourceInputs = preview.ProjectDefinition.Resources
            .Where(r => r.MappedResourceType is not null)
            .Where(r => filterSet is null || filterSet.Contains(r.SourceName))
            .Select(r => new ProjectSetupOrchestrator.ResourceInput
            {
                ResourceType = r.MappedResourceType!,
                Name = r.MappedName ?? r.SourceName,
                ExtractedProperties = r.ExtractedProperties,
            })
            .ToList();

        if (resourceInputs.Count > 0)
        {
            var infraResult = await ProjectSetupOrchestrator.CreateInfrastructureAsync(
                mediator, projectId, createdProjectName);

            if (infraResult.IsError)
            {
                previewService.RemovePreview(previewId);
                return JsonSerializer.Serialize(new
                {
                    status = "applied",
                    previewId,
                    projectId = projectId.ToString(),
                    projectName = createdProjectName,
                    infrastructureError = string.Join("; ", infraResult.Errors.Select(e => e.Description)),
                    createdResources = Array.Empty<object>(),
                    skippedResources = Array.Empty<object>(),
                    nextSuggestedActions = new[]
                    {
                        "Create an infrastructure configuration manually, then add the imported resources.",
                    },
                }, JsonOptions);
            }

            var (_, rgId) = infraResult.Value;
            var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
                mediator, rgId, resourceInputs);

            previewService.RemovePreview(previewId);

            return JsonSerializer.Serialize(new
            {
                status = "applied",
                previewId,
                projectId = projectId.ToString(),
                projectName = createdProjectName,
                createdResources = created.Select(r => new
                {
                    r.ResourceType,
                    r.ResourceId,
                    r.Name,
                }),
                skippedResources = skipped.Select(r => new
                {
                    r.ResourceType,
                    r.Name,
                    r.Reason,
                }),
                nextSuggestedActions = BuildImportNextActions(created.Count, skipped.Count),
            }, JsonOptions);
        }

        previewService.RemovePreview(previewId);

        return JsonSerializer.Serialize(new
        {
            status = "applied",
            previewId,
            projectId = projectId.ToString(),
            projectName = createdProjectName,
            createdResources = Array.Empty<object>(),
            skippedResources = Array.Empty<object>(),
            nextSuggestedActions = new[]
            {
                "No mapped resources found in the import preview.",
                "Add resources manually via the API or frontend.",
                "Generate Bicep with 'generate_project_bicep' once resources are added.",
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

    private static HashSet<string>? ParseResourceFilter(string? resourceFilter)
    {
        if (string.IsNullOrWhiteSpace(resourceFilter))
            return null;

        try
        {
            var names = JsonSerializer.Deserialize<string[]>(resourceFilter, JsonOptions);
            return names is not null ? new HashSet<string>(names, StringComparer.OrdinalIgnoreCase) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string[] BuildImportNextActions(int createdCount, int skippedCount)
    {
        var actions = new List<string>();
        if (skippedCount > 0)
            actions.Add($"{skippedCount} resource(s) could not be auto-created. Add them manually via the API or frontend.");
        actions.Add("Review imported resource configurations.");
        actions.Add("Configure environment-specific settings.");
        if (createdCount > 0)
            actions.Add("Generate Bicep with 'generate_project_bicep'.");
        return actions.ToArray();
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
