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