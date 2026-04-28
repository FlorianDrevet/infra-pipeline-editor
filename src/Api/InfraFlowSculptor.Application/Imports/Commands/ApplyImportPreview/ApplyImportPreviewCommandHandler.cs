using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using MediatR;

namespace InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;

/// <summary>
/// Handles <see cref="ApplyImportPreviewCommand"/> requests.
/// </summary>
public sealed class ApplyImportPreviewCommandHandler(ISender mediator)
    : ICommandHandler<ApplyImportPreviewCommand, ApplyImportPreviewResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ApplyImportPreviewResult>> Handle(
        ApplyImportPreviewCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var environments = ResolveEnvironments(command.Environments);
        var primaryLocation = environments[0].Location;

        var createProjectCommand = new CreateProjectWithSetupCommand(
            Name: command.ProjectName,
            Description: $"Imported from ARM template ({command.Preview.Resources.Count} resources detected)",
            LayoutPreset: command.LayoutPreset,
            Environments: environments,
            Repositories: BuildRepositoriesForLayout(command.LayoutPreset));

        var projectResult = await mediator.Send(createProjectCommand, cancellationToken).ConfigureAwait(false);
        if (projectResult.IsError)
            return projectResult.Errors;

        var resourceInputs = BuildResourceInputs(command.Preview, command.ResourceFilter, primaryLocation);
        if (resourceInputs.Count == 0)
        {
            return new ApplyImportPreviewResult
            {
                ProjectId = projectResult.Value.Id.Value.ToString(),
                ProjectName = projectResult.Value.Name.Value,
                CreatedResources = [],
                SkippedResources = [],
                NextSuggestedActions =
                [
                    "No mapped resources found in the import preview.",
                    "Add resources manually via the API or frontend.",
                    "Generate Bicep with 'generate_project_bicep' once resources are added.",
                ],
            };
        }

        var infrastructureResult = await ImportResourceCreationDispatcher.CreateInfrastructureAsync(
                mediator,
                projectResult.Value.Id.Value,
                projectResult.Value.Name.Value,
            primaryLocation,
                cancellationToken)
            .ConfigureAwait(false);

        if (infrastructureResult.IsError)
        {
            return new ApplyImportPreviewResult
            {
                ProjectId = projectResult.Value.Id.Value.ToString(),
                ProjectName = projectResult.Value.Name.Value,
                InfrastructureError = string.Join("; ", infrastructureResult.Errors.Select(error => error.Description)),
                CreatedResources = [],
                SkippedResources = [],
                NextSuggestedActions =
                [
                    "Create an infrastructure configuration manually, then add the imported resources.",
                ],
            };
        }

        var (createdResources, skippedResources) = await ImportResourceCreationDispatcher.CreateResourcesAsync(
                mediator,
                infrastructureResult.Value.ResourceGroupId,
                resourceInputs,
                cancellationToken)
            .ConfigureAwait(false);

        return new ApplyImportPreviewResult
        {
            ProjectId = projectResult.Value.Id.Value.ToString(),
            ProjectName = projectResult.Value.Name.Value,
            InfrastructureConfigId = infrastructureResult.Value.ConfigId.Value.ToString(),
            ResourceGroupId = infrastructureResult.Value.ResourceGroupId.Value.ToString(),
            CreatedResources = createdResources,
            SkippedResources = skippedResources,
            NextSuggestedActions = BuildNextSuggestedActions(createdResources.Count, skippedResources.Count),
        };
    }

    private static IReadOnlyList<ImportResourceInput> BuildResourceInputs(
        ImportPreviewAnalysisResult preview,
        IReadOnlyList<string>? resourceFilter,
        string defaultLocation)
    {
        var filter = resourceFilter is { Count: > 0 }
            ? new HashSet<string>(resourceFilter, StringComparer.OrdinalIgnoreCase)
            : null;

        var mappedNamesBySource = preview.Resources
            .Where(resource => !string.IsNullOrWhiteSpace(resource.MappedResourceType))
            .ToDictionary(
                resource => resource.SourceName,
                resource => resource.MappedName ?? resource.SourceName,
                StringComparer.OrdinalIgnoreCase);

        return preview.Resources
            .Where(resource => !string.IsNullOrWhiteSpace(resource.MappedResourceType))
            .Where(resource => filter is null || filter.Contains(resource.SourceName))
            .Select(resource => new ImportResourceInput
            {
                ResourceType = resource.MappedResourceType!,
                Name = resource.MappedName ?? resource.SourceName,
                Location = defaultLocation,
                DependencyResourceNames = preview.Dependencies
                    .Where(dependency => string.Equals(dependency.FromResourceName, resource.SourceName, StringComparison.OrdinalIgnoreCase))
                    .Select(dependency => mappedNamesBySource.TryGetValue(dependency.ToResourceName, out var mappedName)
                        ? mappedName
                        : dependency.ToResourceName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                ExtractedProperties = resource.ExtractedProperties,
            })
            .ToList();
    }

    private static IReadOnlyList<EnvironmentSetupItem> ResolveEnvironments(IReadOnlyList<EnvironmentSetupItem>? environments)
    {
        if (environments is { Count: > 0 })
            return environments;

        return
        [
            new EnvironmentSetupItem(
                "Development",
                "dev",
                string.Empty,
                string.Empty,
                "westeurope",
                Guid.Empty,
                0,
                false),
        ];
    }

    private static IReadOnlyList<RepositorySetupItem> BuildRepositoriesForLayout(string layoutPreset)
    {
        return layoutPreset switch
        {
            "AllInOne" =>
            [
                new RepositorySetupItem("main", ["Infrastructure", "ApplicationCode"], null, null, null),
            ],
            "SplitInfraCode" =>
            [
                new RepositorySetupItem("infra", ["Infrastructure"], null, null, null),
                new RepositorySetupItem("app", ["ApplicationCode"], null, null, null),
            ],
            "MultiRepo" => [],
            _ =>
            [
                new RepositorySetupItem("main", ["Infrastructure", "ApplicationCode"], null, null, null),
            ],
        };
    }

    private static IReadOnlyList<string> BuildNextSuggestedActions(int createdCount, int skippedCount)
    {
        var actions = new List<string>();
        if (skippedCount > 0)
        {
            actions.Add($"{skippedCount} resource(s) could not be auto-created. Add them manually via the API or frontend.");
        }

        actions.Add("Review imported resource configurations.");
        actions.Add("Configure environment-specific settings.");

        if (createdCount > 0)
        {
            actions.Add("Generate Bicep with 'generate_project_bicep'.");
        }

        return actions;
    }
}