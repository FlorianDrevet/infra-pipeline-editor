using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

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
