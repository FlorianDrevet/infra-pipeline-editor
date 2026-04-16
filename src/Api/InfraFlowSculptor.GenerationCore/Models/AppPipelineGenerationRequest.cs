namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Request model for application pipeline generation,
/// containing resource-specific CI/CD configuration data.
/// </summary>
public class AppPipelineGenerationRequest
{
    /// <summary>Display name of the target Azure resource.</summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>User-friendly application name for pipeline display. Falls back to <see cref="ResourceName"/> if null.</summary>
    public string? ApplicationName { get; set; }

    /// <summary>Azure resource type identifier (e.g., AzureResourceTypes.WebApp).</summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>Deployment mode: "Code" or "Container".</summary>
    public string DeploymentMode { get; set; } = string.Empty;

    /// <summary>Relative path to the Dockerfile in the repository (container mode).</summary>
    public string? DockerfilePath { get; set; }

    /// <summary>Relative path to the source code folder (code mode).</summary>
    public string? SourceCodePath { get; set; }

    /// <summary>Optional custom build command (e.g., "dotnet publish -c Release").</summary>
    public string? BuildCommand { get; set; }

    /// <summary>Base Docker image name without tag (e.g., "myapp/api").</summary>
    public string? DockerImageName { get; set; }

    /// <summary>Name of the ACR resource (e.g., "myregistry").</summary>
    public string? ContainerRegistryName { get; set; }

    /// <summary>Runtime stack identifier (e.g., "DOTNETCORE", "NODE").</summary>
    public string? RuntimeStack { get; set; }

    /// <summary>Runtime version (e.g., "8.0", "20").</summary>
    public string? RuntimeVersion { get; set; }

    /// <summary>Environment definitions with service connections and subscription IDs.</summary>
    public IReadOnlyList<EnvironmentDefinition> Environments { get; set; } = [];

    /// <summary>Pipeline variable groups to reference in the generated pipeline.</summary>
    public IReadOnlyCollection<PipelineVariableGroupDefinition> PipelineVariableGroups { get; set; } = [];

    /// <summary>Name of the infrastructure configuration this resource belongs to.</summary>
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>Whether the project uses mono-repo pipeline structure.</summary>
    public bool IsMonoRepo { get; set; }

    /// <summary>
    /// Self-hosted agent pool name. When set, pipelines use <c>pool: name: 'value'</c>.
    /// When <c>null</c>, pipelines use the Microsoft-hosted pool (<c>vmImage: ubuntu-latest</c>).
    /// </summary>
    public string? AgentPoolName { get; set; }
}
