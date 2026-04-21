using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;

/// <summary>
/// Handles <see cref="CheckResourceNameAvailabilityQuery"/>: applies templates per environment,
/// validates the generated names against Azure naming rules, and (when supported) calls Azure
/// to check global availability.
/// </summary>
public sealed class CheckResourceNameAvailabilityQueryHandler(
    IProjectAccessService projectAccessService,
    IResourceNameResolver resolver,
    IAzureNameAvailabilityChecker checker)
    : IRequestHandler<CheckResourceNameAvailabilityQuery, ErrorOr<CheckResourceNameAvailabilityResult>>
{
    private const string StatusAvailable = "available";
    private const string StatusUnavailable = "unavailable";
    private const string StatusUnknown = "unknown";
    private const string StatusInvalid = "invalid";
    private const string StatusCurrent = "current";

    /// <inheritdoc />
    public async Task<ErrorOr<CheckResourceNameAvailabilityResult>> Handle(
        CheckResourceNameAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var accessResult = await projectAccessService.VerifyReadAccessAsync(request.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var resolveResult = await resolver.ResolveAsync(
            request.ProjectId,
            request.ConfigId,
            request.ResourceType,
            request.Name,
            cancellationToken);

        if (resolveResult.IsError)
            return resolveResult.Errors;

        // When the submitted name equals the persisted one, every environment is "current" by definition.
        var nameUnchanged = string.Equals(request.Name, request.CurrentPersistedName, StringComparison.OrdinalIgnoreCase);

        // Resolve the persisted name (post-template) only when names differ but a persisted name was provided.
        Dictionary<string, string>? persistedGeneratedNames = null;
        if (!nameUnchanged && !string.IsNullOrWhiteSpace(request.CurrentPersistedName))
        {
            var persistedResolve = await resolver.ResolveAsync(
                request.ProjectId,
                request.ConfigId,
                request.ResourceType,
                request.CurrentPersistedName,
                cancellationToken);

            if (!persistedResolve.IsError)
            {
                persistedGeneratedNames = persistedResolve.Value
                    .ToDictionary(r => r.EnvironmentName, r => r.GeneratedName, StringComparer.OrdinalIgnoreCase);
            }
        }

        var supported = checker.Supports(request.ResourceType);
        var constraint = AzureNamingConstraints.GetConstraint(request.ResourceType);

        var environments = new List<EnvironmentNameAvailabilityResult>(resolveResult.Value.Count);
        foreach (var resolved in resolveResult.Value)
        {
            // Short-circuit: the generated name matches the currently persisted name → "current".
            if (nameUnchanged || IsCurrentName(resolved, persistedGeneratedNames))
            {
                environments.Add(BuildItem(resolved, StatusCurrent, reason: null, message: null));
                continue;
            }

            var validationError = ValidateGeneratedName(resolved.GeneratedName, constraint);
            if (validationError is not null)
            {
                environments.Add(BuildItem(resolved, StatusInvalid, reason: "Invalid", message: validationError));
                continue;
            }

            if (!supported)
            {
                environments.Add(BuildItem(
                    resolved,
                    StatusUnknown,
                    reason: null,
                    message: "Availability check not supported for this resource type."));
                continue;
            }

            var availability = await checker.CheckAsync(
                request.ResourceType,
                resolved.SubscriptionId,
                resolved.GeneratedName,
                cancellationToken);

            var status = availability.Status switch
            {
                AzureNameAvailabilityStatus.Available => StatusAvailable,
                AzureNameAvailabilityStatus.Unavailable => StatusUnavailable,
                _ => StatusUnknown
            };

            environments.Add(BuildItem(resolved, status, availability.Reason, availability.Message));
        }

        return new CheckResourceNameAvailabilityResult(
            request.ResourceType,
            request.Name,
            supported,
            environments);
    }

    private static EnvironmentNameAvailabilityResult BuildItem(
        ResolvedResourceName resolved,
        string status,
        string? reason,
        string? message) =>
        new(
            resolved.EnvironmentName,
            resolved.EnvironmentShortName,
            resolved.SubscriptionId,
            resolved.GeneratedName,
            resolved.AppliedTemplate,
            status,
            reason,
            message);

    private static string? ValidateGeneratedName(string generatedName, AzureNamingConstraint? constraint)
    {
        if (string.IsNullOrEmpty(generatedName))
            return "Generated name is empty.";

        if (constraint is null)
            return null;

        if (generatedName.Length < constraint.MinLength)
            return $"Generated name '{generatedName}' is too short ({generatedName.Length} chars). Minimum allowed: {constraint.MinLength}.";

        if (generatedName.Length > constraint.MaxLength)
            return $"Generated name '{generatedName}' is too long ({generatedName.Length} chars). Maximum allowed: {constraint.MaxLength}.";

        if (constraint.InvalidStaticCharsRegex.IsMatch(generatedName))
            return $"Generated name '{generatedName}' contains invalid characters. Allowed: {constraint.AllowedCharsDescription}.";

        return null;
    }

    /// <summary>
    /// Returns <c>true</c> when the generated name for this environment matches the
    /// previously-persisted generated name (case-insensitive, as Azure DNS is case-insensitive).
    /// </summary>
    private static bool IsCurrentName(
        ResolvedResourceName resolved,
        Dictionary<string, string>? persistedGeneratedNames)
    {
        if (persistedGeneratedNames is null)
            return false;

        return persistedGeneratedNames.TryGetValue(resolved.EnvironmentName, out var persistedName)
               && string.Equals(resolved.GeneratedName, persistedName, StringComparison.OrdinalIgnoreCase);
    }
}
