using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;

/// <summary>
/// Detects compute resources in container mode that have no Docker image configured.
/// Emits an informational diagnostic indicating that the default placeholder image
/// (<c>pause:3.6</c>) will be used until the application pipeline pushes a real image.
/// </summary>
public sealed class DockerImageNotSetDiagnosticRule : IDiagnosticRule
{
    /// <summary>Stable diagnostic code emitted when no Docker image is configured.</summary>
    private const string RuleCode = "DOCKER_IMAGE_NOT_SET";

    /// <summary>The property key that holds the Docker image name.</summary>
    private const string DockerImageNameProperty = "dockerImageName";

    /// <summary>The property key that holds the deployment mode (Code vs Container).</summary>
    private const string DeploymentModeProperty = "deploymentMode";

    /// <summary>The deployment mode value indicating a container-based deployment.</summary>
    private const string ContainerDeploymentMode = "Container";

    /// <summary>ARM resource types that always run in container mode.</summary>
    private static readonly HashSet<string> AlwaysContainerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AzureResourceTypes.ArmTypes.ContainerAppType,
    };

    /// <summary>ARM resource types that may run in container mode depending on deployment mode.</summary>
    private static readonly HashSet<string> ConditionalContainerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        AzureResourceTypes.ArmTypes.WebAppType,
        AzureResourceTypes.ArmTypes.FunctionAppType,
    };

    /// <inheritdoc />
    public Task<IReadOnlyList<ResourceDiagnosticItem>> EvaluateAsync(
        InfrastructureConfigReadModel config,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<ResourceDiagnosticItem>();

        var allResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .ToList();

        foreach (var resource in allResources)
        {
            if (resource.IsExisting)
                continue;

            if (!IsContainerResource(resource))
                continue;

            if (resource.Properties.TryGetValue(DockerImageNameProperty, out var dockerImage)
                && !string.IsNullOrWhiteSpace(dockerImage))
                continue;

            diagnostics.Add(new ResourceDiagnosticItem(
                resource.Id,
                resource.Name,
                resource.ResourceType,
                DiagnosticSeverity.Info,
                RuleCode,
                string.Empty));
        }

        return Task.FromResult<IReadOnlyList<ResourceDiagnosticItem>>(diagnostics);
    }

    private static bool IsContainerResource(AzureResourceReadModel resource)
    {
        if (AlwaysContainerTypes.Contains(resource.ResourceType))
            return true;

        if (!ConditionalContainerTypes.Contains(resource.ResourceType))
            return false;

        return resource.Properties.TryGetValue(DeploymentModeProperty, out var mode)
               && string.Equals(mode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
    }
}
