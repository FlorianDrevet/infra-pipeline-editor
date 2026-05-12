using ErrorOr;

namespace InfraFlowSculptor.GenerationCore.Errors;

/// <summary>
/// Provides typed <see cref="Error"/> values for expected generation failures surfaced at engine boundaries.
/// </summary>
public static class GenerationErrors
{
    private const string InvalidDeploymentModeCode = "Generation.InvalidDeploymentMode";
    private const string MissingAppPipelineGeneratorCode = "Generation.MissingAppPipelineGenerator";
    private const string InvalidPipelineVariableGroupNameCode = "Generation.InvalidPipelineVariableGroupName";
    private const string InvalidAppPipelineConfigurationCode = "Generation.InvalidAppPipelineConfiguration";
    private const string InvalidInfrastructurePipelineConfigurationCode = "Generation.InvalidInfrastructurePipelineConfiguration";
    private const string UnsupportedBicepResourceTypeCode = "Generation.UnsupportedBicepResourceType";
    private const string UnsupportedBicepFeatureCode = "Generation.UnsupportedBicepFeature";
    private const string InvalidBicepConfigurationCode = "Generation.InvalidBicepConfiguration";

    /// <summary>
    /// Returns a validation error when an application pipeline request uses an unsupported deployment mode.
    /// </summary>
    /// <param name="deploymentMode">The unsupported deployment mode.</param>
    /// <param name="validModes">The supported deployment modes.</param>
    /// <returns>A validation error describing the unsupported deployment mode.</returns>
    public static Error InvalidDeploymentMode(string deploymentMode, IReadOnlyCollection<string> validModes) =>
        Error.Validation(
            code: InvalidDeploymentModeCode,
            description: $"Invalid deployment mode '{deploymentMode}'. Valid values are: {string.Join(", ", validModes)}.");

    /// <summary>
    /// Returns a validation error when no application pipeline generator matches the requested resource type and deployment mode.
    /// </summary>
    /// <param name="resourceType">The target resource type.</param>
    /// <param name="deploymentMode">The requested deployment mode.</param>
    /// <returns>A validation error describing the missing application pipeline generator.</returns>
    public static Error MissingAppPipelineGenerator(string resourceType, string deploymentMode) =>
        Error.Validation(
            code: MissingAppPipelineGeneratorCode,
            description: $"No application pipeline generator registered for resource type '{resourceType}' with deployment mode '{deploymentMode}'.");

    /// <summary>
    /// Returns a validation error when a pipeline variable group name is not safe to emit into generated YAML.
    /// </summary>
    /// <param name="description">The validation failure description.</param>
    /// <returns>A validation error describing the invalid pipeline variable group name.</returns>
    public static Error InvalidPipelineVariableGroupName(string description) =>
        Error.Validation(code: InvalidPipelineVariableGroupNameCode, description: description);

    /// <summary>
    /// Returns a validation error when app pipeline generation fails because the request configuration is not supported.
    /// </summary>
    /// <param name="description">The failure description.</param>
    /// <returns>A validation error describing the invalid app pipeline configuration.</returns>
    public static Error InvalidAppPipelineConfiguration(string description) =>
        Error.Validation(code: InvalidAppPipelineConfigurationCode, description: description);

    /// <summary>
    /// Returns a validation error when infrastructure pipeline generation fails because the request configuration is not supported.
    /// </summary>
    /// <param name="description">The failure description.</param>
    /// <returns>A validation error describing the invalid infrastructure pipeline configuration.</returns>
    public static Error InvalidInfrastructurePipelineConfiguration(string description) =>
        Error.Validation(code: InvalidInfrastructurePipelineConfigurationCode, description: description);

    /// <summary>
    /// Returns a validation error when no Bicep generator exists for a requested resource type.
    /// </summary>
    /// <param name="description">The unsupported resource type description.</param>
    /// <returns>A validation error describing the unsupported Bicep resource type.</returns>
    public static Error UnsupportedBicepResourceType(string description) =>
        Error.Validation(code: UnsupportedBicepResourceTypeCode, description: description);

    /// <summary>
    /// Returns a validation error when a Bicep generation feature is not supported by the current generator set.
    /// </summary>
    /// <param name="description">The unsupported feature description.</param>
    /// <returns>A validation error describing the unsupported Bicep feature.</returns>
    public static Error UnsupportedBicepFeature(string description) =>
        Error.Validation(code: UnsupportedBicepFeatureCode, description: description);

    /// <summary>
    /// Returns a validation error when Bicep generation fails because the input configuration is invalid.
    /// </summary>
    /// <param name="description">The validation failure description.</param>
    /// <returns>A validation error describing the invalid Bicep configuration.</returns>
    public static Error InvalidBicepConfiguration(string description) =>
        Error.Validation(code: InvalidBicepConfigurationCode, description: description);
}