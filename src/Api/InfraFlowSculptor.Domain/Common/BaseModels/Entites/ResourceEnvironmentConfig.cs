using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Stores environment-specific configuration values for an <see cref="AzureResource"/>.
/// Each resource can have one entry per environment, holding the configuration properties
/// (e.g., SKU, capacity, TLS version) that vary across deployment environments.
/// </summary>
public sealed class ResourceEnvironmentConfig : Entity<ResourceEnvironmentConfigId>
{
    /// <summary>Gets the parent resource identifier.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    private readonly Dictionary<string, string> _properties = new();

    /// <summary>Gets the configuration key-value pairs for this environment (e.g., sku=Standard, capacity=1).</summary>
    public IReadOnlyDictionary<string, string> Properties => _properties;

    private ResourceEnvironmentConfig() { }

    internal ResourceEnvironmentConfig(
        AzureResourceId resourceId,
        string environmentName,
        IReadOnlyDictionary<string, string> properties)
        : base(ResourceEnvironmentConfigId.CreateUnique())
    {
        ResourceId = resourceId;
        EnvironmentName = environmentName;
        _properties.Clear();
        foreach (var kvp in properties)
            _properties[kvp.Key] = kvp.Value;
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEnvironmentConfig"/> for the specified resource and environment.
    /// </summary>
    public static ResourceEnvironmentConfig Create(
        AzureResourceId resourceId,
        string environmentName,
        IReadOnlyDictionary<string, string> properties)
    {
        return new ResourceEnvironmentConfig(resourceId, environmentName, properties);
    }

    /// <summary>Updates the configuration properties for this environment.</summary>
    public void UpdateProperties(IReadOnlyDictionary<string, string> properties)
    {
        _properties.Clear();
        foreach (var kvp in properties)
            _properties[kvp.Key] = kvp.Value;
    }
}
