using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using System.Collections.ObjectModel;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>
/// Represents a CORS (Cross-Origin Resource Sharing) rule on a Storage Account
/// for either the Blob or Table service.
/// </summary>
public class CorsRule : Entity<CorsRuleId>
{
    private readonly List<string> _allowedOrigins = [];
    private readonly List<string> _allowedMethods = [];
    private readonly List<string> _allowedHeaders = [];
    private readonly List<string> _exposedHeaders = [];
    private readonly ReadOnlyCollection<string> _allowedOriginsView;
    private readonly ReadOnlyCollection<string> _allowedMethodsView;
    private readonly ReadOnlyCollection<string> _allowedHeadersView;
    private readonly ReadOnlyCollection<string> _exposedHeadersView;

    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the storage service type this rule applies to (Blob or Table).</summary>
    public CorsServiceType ServiceType { get; private set; } = null!;

    /// <summary>Gets the allowed origin domains.</summary>
    public IReadOnlyList<string> AllowedOrigins => _allowedOriginsView;

    /// <summary>Gets the allowed HTTP methods.</summary>
    public IReadOnlyList<string> AllowedMethods => _allowedMethodsView;

    /// <summary>Gets the allowed request headers.</summary>
    public IReadOnlyList<string> AllowedHeaders => _allowedHeadersView;

    /// <summary>Gets the response headers exposed to the client.</summary>
    public IReadOnlyList<string> ExposedHeaders => _exposedHeadersView;

    /// <summary>Gets the maximum age in seconds that a preflight response can be cached.</summary>
    public int MaxAgeInSeconds { get; private set; }

    private CorsRule(CorsRuleId id) : base(id)
    {
        _allowedOriginsView = _allowedOrigins.AsReadOnly();
        _allowedMethodsView = _allowedMethods.AsReadOnly();
        _allowedHeadersView = _allowedHeaders.AsReadOnly();
        _exposedHeadersView = _exposedHeaders.AsReadOnly();
    }

    internal CorsRule()
    {
        _allowedOriginsView = _allowedOrigins.AsReadOnly();
        _allowedMethodsView = _allowedMethods.AsReadOnly();
        _allowedHeadersView = _allowedHeaders.AsReadOnly();
        _exposedHeadersView = _exposedHeaders.AsReadOnly();
    }

    /// <summary>Updates all properties of this CORS rule.</summary>
    public void Update(
        CorsServiceType serviceType,
        IReadOnlyList<string> allowedOrigins,
        IReadOnlyList<string> allowedMethods,
        IReadOnlyList<string> allowedHeaders,
        IReadOnlyList<string> exposedHeaders,
        int maxAgeInSeconds)
    {
        ServiceType = serviceType;
        ReplaceContents(_allowedOrigins, allowedOrigins);
        ReplaceContents(_allowedMethods, allowedMethods);
        ReplaceContents(_allowedHeaders, allowedHeaders);
        ReplaceContents(_exposedHeaders, exposedHeaders);
        MaxAgeInSeconds = maxAgeInSeconds;
    }

    /// <summary>Creates a new <see cref="CorsRule"/> with a generated identifier.</summary>
    public static CorsRule Create(
        AzureResourceId storageAccountId,
        CorsServiceType serviceType,
        IReadOnlyList<string> allowedOrigins,
        IReadOnlyList<string> allowedMethods,
        IReadOnlyList<string> allowedHeaders,
        IReadOnlyList<string> exposedHeaders,
        int maxAgeInSeconds)
    {
        var corsRule = new CorsRule(CorsRuleId.CreateUnique())
        {
            StorageAccountId = storageAccountId
        };

        corsRule.Update(serviceType, allowedOrigins, allowedMethods, allowedHeaders, exposedHeaders, maxAgeInSeconds);
        return corsRule;
    }

    private static void ReplaceContents(List<string> target, IReadOnlyList<string> source)
    {
        target.Clear();

        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}