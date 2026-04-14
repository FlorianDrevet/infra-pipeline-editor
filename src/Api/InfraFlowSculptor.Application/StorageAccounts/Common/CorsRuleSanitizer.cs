namespace InfraFlowSculptor.Application.StorageAccounts.Common;

internal static class CorsRuleSanitizer
{
    internal static IReadOnlyList<CorsRuleResult>? Sanitize(IReadOnlyList<CorsRuleResult>? rules)
    {
        if (rules is null)
        {
            return null;
        }

        return rules.Select(rule => new CorsRuleResult(
            NormalizeOrigins(rule.AllowedOrigins),
            NormalizeMethods(rule.AllowedMethods),
            NormalizeHeaders(rule.AllowedHeaders),
            NormalizeHeaders(rule.ExposedHeaders),
            NormalizeMaxAge(rule.MaxAgeInSeconds))).ToList();
    }

    private static IReadOnlyList<string> NormalizeOrigins(IReadOnlyList<string> origins)
        => origins.Select(NormalizeOrigin).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static IReadOnlyList<string> NormalizeMethods(IReadOnlyList<string> methods)
        => methods.Select(method => method.Trim().ToUpperInvariant()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static IReadOnlyList<string> NormalizeHeaders(IReadOnlyList<string> headers)
        => headers.Select(header => header.Trim().ToLowerInvariant()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static string NormalizeOrigin(string origin)
    {
        var trimmed = origin.Trim();
        if (trimmed == "*")
        {
            return trimmed;
        }

        var normalized = trimmed.TrimEnd('/');
        if (normalized.Contains("*.", StringComparison.Ordinal))
        {
            return normalized.ToLowerInvariant();
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            return normalized;
        }

        var authority = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
        return $"{uri.Scheme.ToLowerInvariant()}://{authority.ToLowerInvariant()}";
    }

    private static int NormalizeMaxAge(int maxAgeInSeconds)
        => Math.Max(0, maxAgeInSeconds);
}