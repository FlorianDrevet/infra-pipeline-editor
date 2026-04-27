using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Pure naming, resolution, and string-escaping helpers for application pipelines.
/// </summary>
internal static class AppNamingHelper
{
    private const string DefaultImageTagPattern = "{buildNumber}-{shortSha}";

    /// <summary>
    /// Returns the first environment from the request, used as the build-source environment.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>The first <see cref="EnvironmentDefinition"/>, or <c>null</c> if none exist.</returns>
    internal static EnvironmentDefinition? GetBuildSourceEnvironment(AppPipelineGenerationRequest request)
    {
        return request.Environments.FirstOrDefault();
    }

    /// <summary>
    /// Builds the CI pipeline definition name from the configuration and resource names.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>A human-readable CI pipeline definition name.</returns>
    internal static string BuildCiPipelineDefinitionName(AppPipelineGenerationRequest request)
    {
        return $"{request.ConfigName} - {request.ResourceName} - CI";
    }

    /// <summary>
    /// Resolves the container image repository name.
    /// Uses the explicit <see cref="AppPipelineGenerationRequest.DockerImageName"/> when set,
    /// otherwise falls back to the lower-cased resource name.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>The resolved image repository name.</returns>
    internal static string ResolveImageRepository(AppPipelineGenerationRequest request)
    {
        return request.DockerImageName ?? request.ResourceName.ToLowerInvariant();
    }

    /// <summary>
    /// Resolves the image tag pattern.
    /// Uses the explicit <see cref="AppPipelineGenerationRequest.ImageTagPattern"/> when set,
    /// otherwise falls back to the default <c>{buildNumber}-{shortSha}</c>.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>The resolved image tag pattern.</returns>
    internal static string ResolveImageTagPattern(AppPipelineGenerationRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ImageTagPattern)
            ? DefaultImageTagPattern
            : request.ImageTagPattern;
    }

    /// <summary>
    /// Returns the YAML variable name used to reference the deployment resource in a given resource type.
    /// </summary>
    /// <param name="resourceType">The Azure resource type identifier.</param>
    /// <returns>The deployment resource name variable (e.g. <c>containerAppName</c>).</returns>
    /// <exception cref="InvalidOperationException">When the resource type is unsupported.</exception>
    internal static string GetDeploymentResourceNameVariable(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.ContainerApp => "containerAppName",
            AzureResourceTypes.WebApp => "webAppName",
            AzureResourceTypes.FunctionApp => "functionAppName",
            _ => throw new InvalidOperationException($"Unsupported app deployment resource type '{resourceType}'."),
        };
    }

    /// <summary>
    /// Returns the Azure DevOps task name used to deploy an App Service resource.
    /// </summary>
    /// <param name="resourceType">The Azure resource type identifier.</param>
    /// <returns>The task name (e.g. <c>AzureWebApp@1</c>).</returns>
    /// <exception cref="InvalidOperationException">When the resource type is unsupported.</exception>
    internal static string GetAppServiceTaskName(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.WebApp => "AzureWebApp@1",
            AzureResourceTypes.FunctionApp => "AzureFunctionApp@2",
            _ => throw new InvalidOperationException($"Unsupported App Service resource type '{resourceType}'."),
        };
    }

    /// <summary>
    /// Returns the Azure DevOps <c>appType</c> value for container-based App Service deployments.
    /// </summary>
    /// <param name="resourceType">The Azure resource type identifier.</param>
    /// <returns>The container app type (e.g. <c>webAppContainer</c>).</returns>
    /// <exception cref="InvalidOperationException">When the resource type is unsupported.</exception>
    internal static string GetAppServiceContainerType(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.WebApp => "webAppContainer",
            AzureResourceTypes.FunctionApp => "functionAppContainer",
            _ => throw new InvalidOperationException($"Unsupported container App Service resource type '{resourceType}'."),
        };
    }

    /// <summary>
    /// Returns the Azure DevOps <c>appType</c> value for code-based App Service deployments.
    /// </summary>
    /// <param name="resourceType">The Azure resource type identifier.</param>
    /// <returns>The code app type (e.g. <c>webApp</c>).</returns>
    /// <exception cref="InvalidOperationException">When the resource type is unsupported.</exception>
    internal static string GetAppServiceCodeType(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.WebApp => "webApp",
            AzureResourceTypes.FunctionApp => "functionApp",
            _ => throw new InvalidOperationException($"Unsupported code App Service resource type '{resourceType}'."),
        };
    }

    /// <summary>
    /// Escapes a value for use inside a YAML single-quoted string by doubling single quotes.
    /// </summary>
    /// <param name="value">The raw string value.</param>
    /// <returns>The escaped string safe for single-quoted YAML contexts.</returns>
    internal static string EscapeForSingleQuotedYaml(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    /// <summary>
    /// Escapes a value for use inside a Bash double-quoted string by escaping backslashes and double quotes.
    /// </summary>
    /// <param name="value">The raw string value.</param>
    /// <returns>The escaped string safe for double-quoted Bash contexts.</returns>
    internal static string EscapeForDoubleQuotedBash(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
