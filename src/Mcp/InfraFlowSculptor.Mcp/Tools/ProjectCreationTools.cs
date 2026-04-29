using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Drafts.Models;
using InfraFlowSculptor.Mcp.Tools.Models;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides the MCP tool that creates a project from a completed draft
/// by dispatching a <see cref="CreateProjectWithSetupCommand"/> via MediatR.
/// </summary>
[McpServerToolType]
public sealed class ProjectCreationTools
{
    private ProjectCreationTools() { }

    /// <summary>
    /// Creates a project in Infra Flow Sculptor from a completed and validated draft.
    /// The draft must have status <see cref="DraftStatus.ReadyToCreate"/>.
    /// Also creates an infrastructure config, a default resource group, and all resources
    /// declared in the draft intent.
    /// </summary>
    [McpServerTool(Name = "create_project_from_draft")]
    [Description("Creates a project in Infra Flow Sculptor from a completed and validated draft. The draft must have status 'ReadyToCreate'.")]
    public static async Task<string> CreateProjectFromDraft(
        IProjectDraftService draftService,
        ISender mediator,
        [Description("The draft ID of the completed draft.")] string draftId)
    {
        var draft = draftService.GetDraft(draftId);
        if (draft is null)
        {
            return JsonError("draft_not_found", $"No draft with id '{draftId}' was found.");
        }

        if (draft.Status != DraftStatus.ReadyToCreate)
        {
            return JsonError("draft_not_ready", "The draft is not ready for creation. Call validate_project_draft first.");
        }

        var command = BuildCommand(draft.Intent);
        var result = await mediator.Send(command);

        if (result.IsError)
        {
            return JsonError("creation_failed", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var projectId = result.Value.Id.Value;
        var projectName = result.Value.Name.Value;
        var primaryLocation = draft.Intent.Environments?.FirstOrDefault()?.Location ?? ProjectSetupDefaults.DefaultLocation;

        // Create infrastructure config + resource group + resources if the draft has resources.
        var resourceInputs = (draft.Intent.Resources ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.ResourceType))
            .Select(r => new ResourceInput
            {
                ResourceType = r.ResourceType,
                Name = r.Name ?? $"{projectName}-{r.ResourceType.ToLowerInvariant()}",
                Location = primaryLocation,
            })
            .ToList();

        if (resourceInputs.Count > 0)
        {
            var infraResult = await ProjectSetupOrchestrator.CreateInfrastructureAsync(
                mediator, projectId, projectName, primaryLocation);

            if (infraResult.IsError)
            {
                // Project was created but infrastructure setup failed — return partial success.
                return JsonSerializer.Serialize(new
                {
                    status = "created",
                    projectId = projectId.ToString(),
                    projectName,
                    infrastructureError = string.Join("; ", infraResult.Errors.Select(e => e.Description)),
                    createdResources = Array.Empty<object>(),
                    skippedResources = Array.Empty<object>(),
                    nextSuggestedActions = new[]
                    {
                        "Create an infrastructure configuration manually via the API or frontend.",
                        "Add resources to the project once the infrastructure configuration is set up.",
                    },
                }, McpJsonDefaults.SerializerOptions);
            }

            var (configId, rgId) = infraResult.Value;
            var (created, skipped) = await ProjectSetupOrchestrator.CreateResourcesAsync(
                mediator, rgId, resourceInputs);

            draftService.RemoveDraft(draftId);
            return JsonSerializer.Serialize(new
            {
                status = "created",
                projectId = projectId.ToString(),
                projectName,
                layoutPreset = draft.Intent.LayoutPreset?.ToString(),
                environmentCount = draft.Intent.Environments?.Count ?? 0,
                repositoryCount = draft.Intent.Repositories?.Count ?? 0,
                infrastructureConfigId = configId.Value.ToString(),
                resourceGroupId = rgId.Value.ToString(),
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
                nextSuggestedActions = BuildNextActions(created.Count, skipped.Count),
            }, McpJsonDefaults.SerializerOptions);
        }

        draftService.RemoveDraft(draftId);
        return JsonSerializer.Serialize(new
        {
            status = "created",
            projectId = projectId.ToString(),
            projectName,
            layoutPreset = draft.Intent.LayoutPreset?.ToString(),
            environmentCount = draft.Intent.Environments?.Count ?? 0,
            repositoryCount = draft.Intent.Repositories?.Count ?? 0,
            createdResources = Array.Empty<object>(),
            skippedResources = Array.Empty<object>(),
            nextSuggestedActions = new[]
            {
                "Add resources to the project via the API or frontend.",
                "Configure environment-specific settings for each resource.",
                "Generate Bicep files using 'generate_project_bicep' once resources are added.",
            },
        }, McpJsonDefaults.SerializerOptions);
    }

    private static CreateProjectWithSetupCommand BuildCommand(DraftProjectIntent intent)
    {
        var environments = intent.Environments?.Select(e => new EnvironmentSetupItem(
            e.Name,
            e.ShortName,
            e.Prefix,
            e.Suffix,
            e.Location,
            e.SubscriptionId,
            e.Order,
            e.RequiresApproval
        )).ToList() ?? [];

        var repositories = intent.Repositories?.Select(r => new RepositorySetupItem(
            r.Alias,
            NormalizeContentKinds(r.ContentKinds),
            r.ProviderType,
            r.RepositoryUrl,
            r.DefaultBranch
        )).ToList() ?? [];

        return new CreateProjectWithSetupCommand(
            Name: intent.ProjectName!,
            Description: intent.Description,
            LayoutPreset: intent.LayoutPreset?.ToString() ?? nameof(Domain.ProjectAggregate.ValueObjects.LayoutPresetEnum.MultiRepo),
            Environments: environments,
            Repositories: repositories
        );
    }

    private const string ApplicationContentKindAlias = "Application";

    private static IReadOnlyList<string> NormalizeContentKinds(IReadOnlyList<string> contentKinds)
    {
        return contentKinds
            .Select(contentKind => string.Equals(contentKind, ApplicationContentKindAlias, StringComparison.OrdinalIgnoreCase)
                ? nameof(RepositoryContentKindsEnum.ApplicationCode)
                : contentKind)
            .ToList();
    }

    private static string[] BuildNextActions(int createdCount, int skippedCount)
    {
        var actions = new List<string>();
        if (skippedCount > 0)
            actions.Add($"{skippedCount} resource(s) could not be auto-created. Add them manually via the API or frontend.");
        actions.Add("Configure environment-specific settings for each resource.");
        if (createdCount > 0)
            actions.Add("Generate Bicep files using 'generate_project_bicep'.");
        else
            actions.Add("Add resources to the project, then generate Bicep files using 'generate_project_bicep'.");
        return actions.ToArray();
    }

    private static string JsonError(string error, string message) =>
        McpJsonDefaults.Error(error, message);
}
