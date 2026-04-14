namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Carries a single CORS rule for the blob service companion Bicep module.
/// </summary>
public sealed record BlobCorsRuleData(
    IReadOnlyList<string> AllowedOrigins,
    IReadOnlyList<string> AllowedMethods,
    IReadOnlyList<string> AllowedHeaders,
    IReadOnlyList<string> ExposedHeaders,
    int MaxAgeInSeconds);
