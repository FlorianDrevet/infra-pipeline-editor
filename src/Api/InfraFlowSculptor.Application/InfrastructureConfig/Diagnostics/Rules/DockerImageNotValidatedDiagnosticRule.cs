using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;

/// <summary>
/// Detects compute resources that have a Docker image configured but not yet validated
/// by the user. When the image is not validated, the Bicep generator uses the default
/// placeholder image instead.
/// </summary>
public sealed class DockerImageNotValidatedDiagnosticRule : IDiagnosticRule
{
    /// <summary>Stable diagnostic code emitted when a Docker image is set but not validated.</summary>
    private const string RuleCode = "DOCKER_IMAGE_NOT_VALIDATED";

    /// <summary>The property key that holds the Docker image name.</summary>
    private const string DockerImageNameProperty = "dockerImageName";

    /// <summary>The property key that holds the Docker image validation status.</summary>
    private const string DockerImageValidatedProperty = "dockerImageValidated";

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

            // Only fire when the image IS set but NOT validated
            if (!resource.Properties.TryGetValue(DockerImageNameProperty, out var dockerImage)
                || string.IsNullOrWhiteSpace(dockerImage))
                continue;

            if (resource.Properties.TryGetValue(DockerImageValidatedProperty, out var validated)
                && string.Equals(validated, "true", StringComparison.OrdinalIgnoreCase))
                continue;

            diagnostics.Add(new ResourceDiagnosticItem(
                resource.Id,
                resource.Name,
                resource.ResourceType,
                DiagnosticSeverity.Warning,
                RuleCode,
                resource.Name));
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
