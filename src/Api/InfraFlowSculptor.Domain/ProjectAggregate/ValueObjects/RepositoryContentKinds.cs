using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Bit flags describing what kinds of content a repository hosts.</summary>
[Flags]
public enum RepositoryContentKindsEnum
{
    /// <summary>No content kind selected. Invalid as a persisted value.</summary>
    None = 0,

    /// <summary>Infrastructure-as-Code content (Bicep modules and parameter files).</summary>
    Infrastructure = 1,

    /// <summary>Application source code.</summary>
    ApplicationCode = 2,

    /// <summary>CI/CD pipeline definitions (YAML).</summary>
    Pipelines = 4,
}

/// <summary>
/// Value object wrapping a combination of <see cref="RepositoryContentKindsEnum"/> flags.
/// Stored internally as an <see cref="int"/> to allow stable EF Core persistence.
/// </summary>
public sealed class RepositoryContentKinds : ValueObject
{
    private const char Separator = ',';

    /// <summary>Gets the flags value as an integer (suitable for storage).</summary>
    public int Value { get; private set; }

    /// <summary>Gets the flags value as the strongly-typed enum.</summary>
    public RepositoryContentKindsEnum Flags => (RepositoryContentKindsEnum)Value;

    /// <summary>EF Core constructor.</summary>
    private RepositoryContentKinds() { }

    private RepositoryContentKinds(RepositoryContentKindsEnum flags)
    {
        Value = (int)flags;
    }

    /// <summary>
    /// Creates a new <see cref="RepositoryContentKinds"/> from the given flags.
    /// At least one flag must be set; <see cref="RepositoryContentKindsEnum.None"/> is rejected.
    /// </summary>
    /// <param name="flags">The combination of flags to wrap.</param>
    /// <returns>The created value object or a validation error.</returns>
    public static ErrorOr<RepositoryContentKinds> Create(RepositoryContentKindsEnum flags)
    {
        if (flags == RepositoryContentKindsEnum.None)
            return Errors.ProjectRepository.NoContentKind();

        return new RepositoryContentKinds(flags);
    }

    /// <summary>Determines whether the specified flag is set.</summary>
    /// <param name="kind">The flag to test.</param>
    /// <returns><c>true</c> if the flag is set; otherwise <c>false</c>.</returns>
    public bool Has(RepositoryContentKindsEnum kind) => (Flags & kind) == kind && kind != RepositoryContentKindsEnum.None;

    /// <summary>
    /// Parses a comma-separated CSV string of flag names (case-insensitive) into a value object.
    /// Used by the EF Core value converter.
    /// </summary>
    /// <param name="raw">The CSV string (e.g. <c>"Infrastructure,Pipelines"</c>).</param>
    /// <returns>The parsed value object or a validation error.</returns>
    public static ErrorOr<RepositoryContentKinds> Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Errors.ProjectRepository.NoContentKind();

        var flags = RepositoryContentKindsEnum.None;
        foreach (var token in raw.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<RepositoryContentKindsEnum>(token, ignoreCase: true, out var parsed))
                return Errors.ProjectRepository.NoContentKind();

            flags |= parsed;
        }

        return Create(flags);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var parts = new List<string>(3);
        if (Has(RepositoryContentKindsEnum.Infrastructure))
            parts.Add(nameof(RepositoryContentKindsEnum.Infrastructure));
        if (Has(RepositoryContentKindsEnum.ApplicationCode))
            parts.Add(nameof(RepositoryContentKindsEnum.ApplicationCode));
        if (Has(RepositoryContentKindsEnum.Pipelines))
            parts.Add(nameof(RepositoryContentKindsEnum.Pipelines));

        return string.Join(Separator, parts);
    }

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
