using System.Text.RegularExpressions;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>
/// Slug-like value object identifying a <see cref="Entities.ProjectRepository"/> within a project.
/// Used as the logical reference key from <c>InfrastructureConfig.RepositoryBinding</c>.
/// </summary>
public sealed class RepositoryAlias : ValueObject
{
    private const int MaxLength = 50;
    private static readonly Regex AliasRegex = new("^[a-z0-9-]+$", RegexOptions.Compiled);

    /// <summary>Gets the underlying slug string.</summary>
    public string Value { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private RepositoryAlias() { }

    private RepositoryAlias(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="RepositoryAlias"/> after validating the input slug.
    /// Allowed characters: lowercase letters, digits and hyphens. Maximum length: 50.
    /// </summary>
    /// <param name="value">The candidate slug.</param>
    /// <returns>The created alias or a validation error.</returns>
    public static ErrorOr<RepositoryAlias> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Errors.ProjectRepository.InvalidAlias(value ?? string.Empty);

        if (value.Length > MaxLength)
            return Errors.ProjectRepository.InvalidAlias(value);

        if (!AliasRegex.IsMatch(value))
            return Errors.ProjectRepository.InvalidAlias(value);

        return new RepositoryAlias(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Implicitly converts a <see cref="RepositoryAlias"/> to its underlying string value.</summary>
    public static implicit operator string(RepositoryAlias alias) => alias.Value;

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
