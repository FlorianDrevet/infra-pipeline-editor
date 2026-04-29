using System.Text.Json;
using InfraFlowSculptor.Application.Imports.Common.Arm;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Imports.Common.Properties;

/// <summary>
/// Base interface for typed resource property extraction results.
/// Implementations parse ARM resource properties into strongly-typed records
/// and provide dictionary conversion for the public contract boundary.
/// </summary>
public interface IExtractedResourceProperties
{
    /// <summary>
    /// Converts the typed properties to a dictionary for serialization and public API contracts.
    /// </summary>
    IReadOnlyDictionary<string, object?> ToDictionary();

    /// <summary>
    /// Returns the list of source property names that were not mapped.
    /// </summary>
    IReadOnlyList<string> UnmappedProperties { get; }
}

/// <summary>
/// Resolves an <see cref="IExtractedResourceProperties"/> from an untyped dictionary and resource type.
/// Used when properties round-trip through HTTP serialization.
/// </summary>
public static class ExtractedPropertiesResolver
{
    /// <summary>
    /// Reconstructs a typed property record from a dictionary and resource type identifier.
    /// Returns <c>null</c> if the resource type has no typed properties or the dictionary is empty.
    /// </summary>
    public static IExtractedResourceProperties? FromDictionary(
        string resourceType,
        IReadOnlyDictionary<string, object?>? properties)
    {
        if (properties is null or { Count: 0 })
            return null;

        return resourceType switch
        {
            AzureResourceTypes.KeyVault => KeyVaultExtractedProperties.FromDictionary(properties),
            AzureResourceTypes.StorageAccount => StorageAccountExtractedProperties.FromDictionary(properties),
            AzureResourceTypes.AppServicePlan => AppServicePlanExtractedProperties.FromDictionary(properties),
            AzureResourceTypes.WebApp => WebAppExtractedProperties.FromDictionary(properties),
            AzureResourceTypes.FunctionApp => FunctionAppExtractedProperties.FromDictionary(properties),
            _ => null,
        };
    }
}

/// <summary>Extracted properties for a KeyVault ARM resource.</summary>
public sealed record KeyVaultExtractedProperties(
    string? SkuName,
    IReadOnlyList<string> UnmappedProperties) : IExtractedResourceProperties
{
    /// <summary>Parses KeyVault properties from an ARM resource.</summary>
    public static KeyVaultExtractedProperties FromArm(JsonElement properties)
    {
        string? skuName = null;

        if (properties.TryGetProperty("sku", out var sku)
            && sku.TryGetProperty("name", out var skuNameElement))
        {
            skuName = skuNameElement.GetString();
        }

        var unmapped = properties.EnumerateObject()
            .Where(p => p.Name is not "sku")
            .Select(p => p.Name)
            .ToList();

        return new KeyVaultExtractedProperties(skuName, unmapped);
    }

    /// <summary>Reconstructs from a serialized dictionary.</summary>
    public static KeyVaultExtractedProperties FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        var skuName = dict.TryGetValue("skuName", out var v) && v is string s ? s : null;
        return new KeyVaultExtractedProperties(skuName, []);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
        var dict = new Dictionary<string, object?>();
        if (SkuName is not null)
            dict["skuName"] = SkuName;
        return dict;
    }
}

/// <summary>Extracted properties for a StorageAccount ARM resource.</summary>
public sealed record StorageAccountExtractedProperties(
    string? SkuName,
    string? Kind,
    IReadOnlyList<string> UnmappedProperties) : IExtractedResourceProperties
{
    /// <summary>The storage account kind. Falls back to <c>StorageV2</c>.</summary>
    public string KindOrDefault => Kind ?? "StorageV2";

    /// <summary>Parses StorageAccount properties from an ARM resource.</summary>
    public static StorageAccountExtractedProperties FromArm(ArmResource armResource, JsonElement properties)
    {
        var skuName = armResource.Sku?.Name;
        var kind = armResource.Kind;

        var unmapped = properties.EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        return new StorageAccountExtractedProperties(skuName, kind, unmapped);
    }

    /// <summary>Reconstructs from a serialized dictionary.</summary>
    public static StorageAccountExtractedProperties FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        var skuName = dict.TryGetValue("skuName", out var v1) && v1 is string s1 ? s1 : null;
        var kind = dict.TryGetValue("kind", out var v2) && v2 is string s2 ? s2 : null;
        return new StorageAccountExtractedProperties(skuName, kind, []);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
        var dict = new Dictionary<string, object?>();
        if (SkuName is not null)
            dict["skuName"] = SkuName;
        if (Kind is not null)
            dict["kind"] = Kind;
        return dict;
    }
}

/// <summary>Extracted properties for an AppServicePlan ARM resource.</summary>
public sealed record AppServicePlanExtractedProperties(
    string OsType) : IExtractedResourceProperties
{
    /// <summary>Default OS type when not specified.</summary>
    public const string DefaultOsType = "Linux";

    /// <summary>Reconstructs from a serialized dictionary.</summary>
    public static AppServicePlanExtractedProperties FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        var osType = dict.TryGetValue("osType", out var v) && v is string s ? s : DefaultOsType;
        return new AppServicePlanExtractedProperties(osType);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> UnmappedProperties => [];

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToDictionary()
        => new Dictionary<string, object?> { ["osType"] = OsType };
}

/// <summary>Extracted properties for a WebApp ARM resource.</summary>
public sealed record WebAppExtractedProperties(
    string RuntimeStack,
    string RuntimeVersion) : IExtractedResourceProperties
{
    /// <summary>Default runtime stack.</summary>
    public const string DefaultRuntimeStack = "DOTNETCORE";

    /// <summary>Default runtime version.</summary>
    public const string DefaultRuntimeVersion = "8.0";

    /// <summary>Reconstructs from a serialized dictionary.</summary>
    public static WebAppExtractedProperties FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        var runtimeStack = dict.TryGetValue("runtimeStack", out var v1) && v1 is string s1 ? s1 : DefaultRuntimeStack;
        var runtimeVersion = dict.TryGetValue("runtimeVersion", out var v2) && v2 is string s2 ? s2 : DefaultRuntimeVersion;
        return new WebAppExtractedProperties(runtimeStack, runtimeVersion);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> UnmappedProperties => [];

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToDictionary()
        => new Dictionary<string, object?>
        {
            ["runtimeStack"] = RuntimeStack,
            ["runtimeVersion"] = RuntimeVersion,
        };
}

/// <summary>Extracted properties for a FunctionApp ARM resource.</summary>
public sealed record FunctionAppExtractedProperties(
    string RuntimeStack,
    string RuntimeVersion) : IExtractedResourceProperties
{
    /// <summary>Default runtime stack.</summary>
    public const string DefaultRuntimeStack = "DOTNET-ISOLATED";

    /// <summary>Default runtime version.</summary>
    public const string DefaultRuntimeVersion = "8.0";

    /// <summary>Reconstructs from a serialized dictionary.</summary>
    public static FunctionAppExtractedProperties FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        var runtimeStack = dict.TryGetValue("runtimeStack", out var v1) && v1 is string s1 ? s1 : DefaultRuntimeStack;
        var runtimeVersion = dict.TryGetValue("runtimeVersion", out var v2) && v2 is string s2 ? s2 : DefaultRuntimeVersion;
        return new FunctionAppExtractedProperties(runtimeStack, runtimeVersion);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> UnmappedProperties => [];

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToDictionary()
        => new Dictionary<string, object?>
        {
            ["runtimeStack"] = RuntimeStack,
            ["runtimeVersion"] = RuntimeVersion,
        };
}
