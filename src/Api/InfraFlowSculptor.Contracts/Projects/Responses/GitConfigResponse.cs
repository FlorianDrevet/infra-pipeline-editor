namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing the Git repository configuration of a project.</summary>
public record GitConfigResponse(
    string Id,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? BasePath,
    string KeyVaultUrl,
    string SecretName,
    string Owner,
    string RepositoryName);
