using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class CorsRuleEntry : IValidatableObject
{
    private static readonly HashSet<string> SupportedMethods =
    [
        "DELETE",
        "GET",
        "HEAD",
        "MERGE",
        "OPTIONS",
        "PATCH",
        "POST",
        "PUT"
    ];

    private static readonly Regex HeaderPattern = new(
        @"^[A-Za-z0-9!#$%&'*+.^_`|~-]+(?:-[A-Za-z0-9!#$%&'*+.^_`|~-]+)*\*?$",
        RegexOptions.Compiled);

    [Required]
    public required List<string> AllowedOrigins { get; init; }

    [Required]
    public required List<string> AllowedMethods { get; init; }

    [Required]
    public required List<string> AllowedHeaders { get; init; }

    [Required]
    public required List<string> ExposedHeaders { get; init; }

    [Range(0, int.MaxValue)]
    public int MaxAgeInSeconds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateOriginList(AllowedOrigins, nameof(AllowedOrigins)))
            yield return result;

        foreach (var result in ValidateMethodList(AllowedMethods, nameof(AllowedMethods)))
            yield return result;

        foreach (var result in ValidateHeaderList(AllowedHeaders, nameof(AllowedHeaders)))
            yield return result;

        foreach (var result in ValidateHeaderList(ExposedHeaders, nameof(ExposedHeaders)))
            yield return result;

        if (MaxAgeInSeconds < 0)
        {
            yield return new ValidationResult(
                "MaxAgeInSeconds must be greater than or equal to 0.",
                [nameof(MaxAgeInSeconds)]);
        }
    }

    private static IEnumerable<ValidationResult> ValidateOriginList(IReadOnlyList<string>? origins, string memberName)
    {
        if (origins is null || origins.Count == 0)
        {
            yield return new ValidationResult("At least one allowed origin is required.", [memberName]);
            yield break;
        }

        for (var index = 0; index < origins.Count; index++)
        {
            var origin = origins[index]?.Trim();
            if (string.IsNullOrWhiteSpace(origin))
            {
                yield return new ValidationResult("Origins cannot contain empty values.", [$"{memberName}[{index}]"]);
                continue;
            }

            if (origin.Length > 256)
            {
                yield return new ValidationResult("Each origin must be 256 characters or fewer.", [$"{memberName}[{index}]"]);
                continue;
            }

            if (origin == "*")
                continue;

            if (!IsValidOrigin(origin))
            {
                yield return new ValidationResult(
                    "Origins must be '*' or a valid http/https origin without path, query, or fragment. Wildcard subdomains such as 'https://*.contoso.com' are supported.",
                    [$"{memberName}[{index}]"]);
            }
        }
    }

    private static IEnumerable<ValidationResult> ValidateMethodList(IReadOnlyList<string>? methods, string memberName)
    {
        if (methods is null || methods.Count == 0)
        {
            yield return new ValidationResult("At least one allowed method is required.", [memberName]);
            yield break;
        }

        for (var index = 0; index < methods.Count; index++)
        {
            var method = methods[index]?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(method) || !SupportedMethods.Contains(method))
            {
                yield return new ValidationResult(
                    $"Unsupported CORS method '{methods[index]}'. Allowed values: {string.Join(", ", SupportedMethods)}.",
                    [$"{memberName}[{index}]"]);
            }
        }
    }

    private static IEnumerable<ValidationResult> ValidateHeaderList(IReadOnlyList<string>? headers, string memberName)
    {
        if (headers is null)
        {
            yield break;
        }

        var prefixedCount = 0;
        var literalCount = 0;

        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]?.Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                yield return new ValidationResult("Headers cannot contain empty values.", [$"{memberName}[{index}]"]);
                continue;
            }

            if (header.Length > 256)
            {
                yield return new ValidationResult("Each header must be 256 characters or fewer.", [$"{memberName}[{index}]"]);
                continue;
            }

            if (!HeaderPattern.IsMatch(header))
            {
                yield return new ValidationResult(
                    "Headers must be valid HTTP header names. A wildcard is only allowed at the end for prefixed headers, for example 'x-ms-meta*'.",
                    [$"{memberName}[{index}]"]);
                continue;
            }

            if (header.EndsWith('*'))
                prefixedCount++;
            else
                literalCount++;
        }

        if (literalCount > 64)
        {
            yield return new ValidationResult("A CORS header list cannot contain more than 64 literal headers.", [memberName]);
        }

        if (prefixedCount > 2)
        {
            yield return new ValidationResult("A CORS header list cannot contain more than 2 prefixed headers ending with '*'.", [memberName]);
        }
    }

    private static bool IsValidOrigin(string value)
    {
        var normalized = value.Trim().TrimEnd('/');
        if (normalized.Contains("*.", StringComparison.Ordinal))
        {
            var wildcardOriginPattern = new Regex(
                @"^https?://\*\.[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*(?::\d{1,5})?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return wildcardOriginPattern.IsMatch(normalized);
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            return false;

        return uri.GetLeftPart(UriPartial.Authority).Equals(normalized, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>Common properties shared by create and update Storage Account requests.</summary>
public abstract class StorageAccountRequestBase : IValidatableObject
{
    /// <summary>Display name for the Storage Account resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Storage Account will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Storage account kind (e.g. StorageV2, BlobStorage).</summary>
    [Required, EnumValidation(typeof(StorageAccountKind.Kind))]
    public required string Kind { get; init; }

    /// <summary>Default access tier (Hot, Cool, Premium).</summary>
    [Required, EnumValidation(typeof(StorageAccessTier.Tier))]
    public required string AccessTier { get; init; }

    /// <summary>Whether public access to blobs is allowed.</summary>
    public bool AllowBlobPublicAccess { get; init; }

    /// <summary>Whether HTTPS-only traffic is enforced.</summary>
    public bool EnableHttpsTrafficOnly { get; init; } = true;

    /// <summary>Minimum TLS version for client connections.</summary>
    [Required, EnumValidation(typeof(StorageAccountTlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    /// <summary>Per-environment typed configuration overrides (SKU only).</summary>
    public List<StorageAccountEnvironmentConfigEntry>? EnvironmentSettings { get; init; }

    /// <summary>Blob service CORS rules applied after storage account deployment.</summary>
    public List<CorsRuleEntry>? CorsRules { get; init; }

    /// <summary>Table service CORS rules applied after storage account deployment.</summary>
    public List<CorsRuleEntry>? TableCorsRules { get; init; }

    /// <summary>Blob lifecycle management rules that auto-delete blobs after a TTL.</summary>
    public List<BlobLifecycleRuleEntry>? LifecycleRules { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateRuleCollection(CorsRules, nameof(CorsRules)))
            yield return result;

        foreach (var result in ValidateRuleCollection(TableCorsRules, nameof(TableCorsRules)))
            yield return result;

        foreach (var result in ValidateLifecycleRules(LifecycleRules, nameof(LifecycleRules)))
            yield return result;
    }

    private static IEnumerable<ValidationResult> ValidateRuleCollection(List<CorsRuleEntry>? rules, string memberName)
    {
        if (rules is null)
            yield break;

        if (rules.Count > 5)
        {
            yield return new ValidationResult("A storage service can define at most 5 CORS rules.", [memberName]);
        }

        for (var index = 0; index < rules.Count; index++)
        {
            var entry = rules[index];
            var context = new ValidationContext(entry);
            var nestedResults = new List<ValidationResult>();
            Validator.TryValidateObject(entry, context, nestedResults, true);
            foreach (var result in nestedResults)
            {
                var memberNames = result.MemberNames.Any()
                    ? result.MemberNames.Select(member => $"{memberName}[{index}].{member}")
                    : [$"{memberName}[{index}]"];
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }
        }
    }

    private static IEnumerable<ValidationResult> ValidateLifecycleRules(List<BlobLifecycleRuleEntry>? rules, string memberName)
    {
        if (rules is null)
            yield break;

        var ruleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < rules.Count; index++)
        {
            var entry = rules[index];
            var context = new ValidationContext(entry);
            var nestedResults = new List<ValidationResult>();
            Validator.TryValidateObject(entry, context, nestedResults, true);
            foreach (var result in nestedResults)
            {
                var memberNames = result.MemberNames.Any()
                    ? result.MemberNames.Select(member => $"{memberName}[{index}].{member}")
                    : [$"{memberName}[{index}]"];
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }

            if (!string.IsNullOrWhiteSpace(entry.RuleName) && !ruleNames.Add(entry.RuleName))
            {
                yield return new ValidationResult(
                    $"Duplicate lifecycle rule name '{entry.RuleName}'.",
                    [$"{memberName}[{index}].{nameof(BlobLifecycleRuleEntry.RuleName)}"]);
            }
        }
    }
}

/// <summary>Typed per-environment configuration entry for a Storage Account. Only SKU varies per environment.</summary>
public class StorageAccountEnvironmentConfigEntry
{
    /// <summary>Name of the target environment.</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional performance/replication tier override.</summary>
    [EnumValidation(typeof(StorageAccountSku.Sku))]
    public string? Sku { get; init; }
}

/// <summary>Response DTO for a typed per-environment Storage Account configuration.</summary>
public record StorageAccountEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku);

/// <summary>Describes a single blob lifecycle management rule.</summary>
public class BlobLifecycleRuleEntry : IValidatableObject
{
    /// <summary>Display name of the lifecycle rule.</summary>
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public required string RuleName { get; init; }

    /// <summary>Names of the blob containers this rule targets.</summary>
    [Required]
    public required List<string> ContainerNames { get; init; }

    /// <summary>Number of days after blob creation before automatic deletion.</summary>
    [Required]
    [Range(1, 36500)]
    public int TimeToLiveInDays { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ContainerNames is null || ContainerNames.Count == 0)
        {
            yield return new ValidationResult(
                "At least one container name is required.",
                [nameof(ContainerNames)]);
            yield break;
        }

        for (var index = 0; index < ContainerNames.Count; index++)
        {
            var name = ContainerNames[index]?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                yield return new ValidationResult(
                    "Container names cannot contain empty values.",
                    [$"{nameof(ContainerNames)}[{index}]"]);
            }
        }
    }
}
