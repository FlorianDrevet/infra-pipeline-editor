using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Projects.Requests;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

/// <summary>
/// Request body for applying a previously previewed IaC import to a new project.
/// </summary>
public sealed record ApplyImportPreviewRequest : IValidatableObject
{
    /// <summary>
    /// Gets the project name to create.
    /// </summary>
    [Required, StringLength(80, MinimumLength = 3)]
    public required string ProjectName { get; init; }

    /// <summary>
    /// Gets the layout preset for the new project.
    /// </summary>
    [Required]
    public required string LayoutPreset { get; init; }

    /// <summary>
    /// Gets the optional environment definitions for the new project.
    /// </summary>
    public IReadOnlyList<EnvironmentSetupRequest>? Environments { get; init; }

    /// <summary>
    /// Gets the optional source resource names to include.
    /// </summary>
    public IReadOnlyList<string>? ResourceFilter { get; init; }

    /// <summary>
    /// Gets the preview payload to apply.
    /// </summary>
    [Required]
    public required ImportPreviewPayloadRequest Preview { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateNestedObject(Preview, nameof(Preview)))
            yield return result;

        foreach (var result in ValidateNestedCollection(Environments, nameof(Environments)))
            yield return result;
    }

    private static IEnumerable<ValidationResult> ValidateNestedObject(object? value, string memberName)
    {
        if (value is null)
            yield break;

        var nestedResults = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), nestedResults, true);
        foreach (var result in nestedResults)
        {
            var memberNames = result.MemberNames.Any()
                ? result.MemberNames.Select(member => $"{memberName}.{member}")
                : [memberName];
            yield return new ValidationResult(result.ErrorMessage, memberNames);
        }
    }

    private static IEnumerable<ValidationResult> ValidateNestedCollection<T>(IReadOnlyList<T>? items, string memberName)
    {
        if (items is null)
            yield break;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (item is null)
            {
                yield return new ValidationResult($"{memberName} entries cannot be null.", [$"{memberName}[{index}]"]);
                continue;
            }

            var nestedResults = new List<ValidationResult>();
            Validator.TryValidateObject(item, new ValidationContext(item), nestedResults, true);
            foreach (var result in nestedResults)
            {
                var memberNames = result.MemberNames.Any()
                    ? result.MemberNames.Select(member => $"{memberName}[{index}].{member}")
                    : [$"{memberName}[{index}]"];
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }
        }
    }
}

/// <summary>
/// Represents the stateless preview payload sent to the apply endpoint.
/// </summary>
public sealed record ImportPreviewPayloadRequest : IValidatableObject
{
    /// <summary>
    /// Gets the source format identifier.
    /// </summary>
    [Required]
    public required string SourceFormat { get; init; }

    /// <summary>
    /// Gets the parsed resources from the preview.
    /// </summary>
    [Required]
    public required IReadOnlyList<ImportPreviewResourceRequest> Resources { get; init; }

    /// <summary>
    /// Gets the parsed dependencies from the preview.
    /// </summary>
    public IReadOnlyList<ImportPreviewDependencyRequest> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets the preview metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the identified gaps.
    /// </summary>
    public IReadOnlyList<ImportPreviewGapRequest> Gaps { get; init; } = [];

    /// <summary>
    /// Gets the unsupported source resource names.
    /// </summary>
    public IReadOnlyList<string> UnsupportedResources { get; init; } = [];

    /// <summary>
    /// Gets the human-readable preview summary.
    /// </summary>
    [Required]
    public required string Summary { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateNestedCollection(Resources, nameof(Resources)))
            yield return result;

        foreach (var result in ValidateNestedCollection(Dependencies, nameof(Dependencies)))
            yield return result;

        foreach (var result in ValidateNestedCollection(Gaps, nameof(Gaps)))
            yield return result;
    }

    private static IEnumerable<ValidationResult> ValidateNestedCollection<T>(IReadOnlyList<T>? items, string memberName)
    {
        if (items is null)
            yield break;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (item is null)
            {
                yield return new ValidationResult($"{memberName} entries cannot be null.", [$"{memberName}[{index}]"]);
                continue;
            }

            var nestedResults = new List<ValidationResult>();
            Validator.TryValidateObject(item, new ValidationContext(item), nestedResults, true);
            foreach (var result in nestedResults)
            {
                var memberNames = result.MemberNames.Any()
                    ? result.MemberNames.Select(member => $"{memberName}[{index}].{member}")
                    : [$"{memberName}[{index}]"];
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }
        }
    }
}

/// <summary>
/// Represents one parsed preview resource.
/// </summary>
public sealed record ImportPreviewResourceRequest
{
    /// <summary>
    /// Gets the original source resource type.
    /// </summary>
    [Required]
    public required string SourceType { get; init; }

    /// <summary>
    /// Gets the original source resource name.
    /// </summary>
    [Required]
    public required string SourceName { get; init; }

    /// <summary>
    /// Gets the mapped InfraFlowSculptor resource type when one is available.
    /// </summary>
    public string? MappedResourceType { get; init; }

    /// <summary>
    /// Gets the mapped resource name when one is available.
    /// </summary>
    public string? MappedName { get; init; }

    /// <summary>
    /// Gets the mapping confidence string.
    /// </summary>
    [Required]
    public required string Confidence { get; init; }

    /// <summary>
    /// Gets the extracted source properties.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the source properties that could not be mapped.
    /// </summary>
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}

/// <summary>
/// Represents one parsed preview dependency.
/// </summary>
public sealed record ImportPreviewDependencyRequest
{
    /// <summary>
    /// Gets the source resource name.
    /// </summary>
    [Required]
    public required string FromResourceName { get; init; }

    /// <summary>
    /// Gets the target resource name.
    /// </summary>
    [Required]
    public required string ToResourceName { get; init; }

    /// <summary>
    /// Gets the dependency type.
    /// </summary>
    [Required]
    public required string DependencyType { get; init; }
}

/// <summary>
/// Represents one preview gap.
/// </summary>
public sealed record ImportPreviewGapRequest
{
    /// <summary>
    /// Gets the gap severity string.
    /// </summary>
    [Required]
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the gap category.
    /// </summary>
    [Required]
    public required string Category { get; init; }

    /// <summary>
    /// Gets the gap message.
    /// </summary>
    [Required]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the related source resource name when one is available.
    /// </summary>
    public string? SourceResourceName { get; init; }
}