using System.Collections.Concurrent;
using System.Text.Json;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Imports.Models;

namespace InfraFlowSculptor.Mcp.Imports;

/// <summary>In-memory implementation of <see cref="IImportPreviewService"/> for the MCP session.</summary>
public sealed class ImportPreviewService : IImportPreviewService
{
    private readonly ConcurrentDictionary<string, ImportPreview> _previews = new();

    /// <inheritdoc />
    public ImportPreview CreatePreviewFromArm(string sourceContent)
    {
        using var doc = JsonDocument.Parse(sourceContent);
        var root = doc.RootElement;

        var resources = new List<ImportedResourceDefinition>();
        var dependencies = new List<ImportedDependency>();
        var gaps = new List<ImportGap>();
        var unsupported = new List<string>();
        var metadata = new Dictionary<string, string>();

        if (root.TryGetProperty("$schema", out var schema))
        {
            metadata["schema"] = schema.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("contentVersion", out var contentVersion))
        {
            metadata["contentVersion"] = contentVersion.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("resources", out var resourcesArray)
            || resourcesArray.ValueKind != JsonValueKind.Array)
        {
            var previewId = GeneratePreviewId();
            var emptyPreview = new ImportPreview
            {
                PreviewId = previewId,
                ProjectDefinition = new ImportedProjectDefinition
                {
                    SourceFormat = "arm-json",
                    Resources = [],
                    Dependencies = [],
                    Metadata = metadata,
                },
                Gaps = [],
                UnsupportedResources = [],
            };
            _previews[previewId] = emptyPreview;
            return emptyPreview;
        }

        foreach (var resourceElement in resourcesArray.EnumerateArray())
        {
            var sourceType = resourceElement.TryGetProperty("type", out var typeProp)
                ? typeProp.GetString() ?? string.Empty
                : string.Empty;

            var sourceName = resourceElement.TryGetProperty("name", out var nameProp)
                ? StripArmExpression(nameProp.GetString() ?? string.Empty)
                : string.Empty;

            var isMapped = AzureResourceTypes.ArmTypeToFriendlyName.TryGetValue(sourceType, out var friendlyName);

            var extractedProperties = new Dictionary<string, object?>();
            var unmappedProperties = new List<string>();

            ExtractProperties(resourceElement, sourceType, friendlyName, extractedProperties, unmappedProperties);

            if (!isMapped)
            {
                unsupported.Add(sourceName);
                gaps.Add(new ImportGap
                {
                    Severity = ImportGapSeverity.Warning,
                    Category = "unsupported_resource",
                    Message = $"Resource type '{sourceType}' is not supported by InfraFlowSculptor.",
                    SourceResourceName = sourceName,
                });
            }

            foreach (var unmapped in unmappedProperties)
            {
                gaps.Add(new ImportGap
                {
                    Severity = ImportGapSeverity.Info,
                    Category = "unmapped_property",
                    Message = $"Property '{unmapped}' on '{sourceName}' is auto-managed by InfraFlowSculptor.",
                    SourceResourceName = sourceName,
                });
            }

            resources.Add(new ImportedResourceDefinition
            {
                SourceType = sourceType,
                SourceName = sourceName,
                MappedResourceType = isMapped ? friendlyName : null,
                MappedName = isMapped ? sourceName : null,
                Confidence = isMapped ? MappingConfidence.High : MappingConfidence.Low,
                ExtractedProperties = extractedProperties,
                UnmappedProperties = unmappedProperties,
            });

            if (resourceElement.TryGetProperty("dependsOn", out var dependsOn)
                && dependsOn.ValueKind == JsonValueKind.Array)
            {
                foreach (var dep in dependsOn.EnumerateArray())
                {
                    var depValue = dep.GetString() ?? string.Empty;
                    var targetName = ExtractResourceNameFromDependsOn(depValue);

                    dependencies.Add(new ImportedDependency(
                        sourceName,
                        targetName,
                        "dependsOn"));
                }
            }
        }

        var id = GeneratePreviewId();
        var preview = new ImportPreview
        {
            PreviewId = id,
            ProjectDefinition = new ImportedProjectDefinition
            {
                SourceFormat = "arm-json",
                Resources = resources,
                Dependencies = dependencies,
                Metadata = metadata,
            },
            Gaps = gaps,
            UnsupportedResources = unsupported,
        };

        _previews[id] = preview;
        return preview;
    }

    /// <inheritdoc />
    public ImportPreview? GetPreview(string previewId)
    {
        return _previews.TryGetValue(previewId, out var preview) ? preview : null;
    }

    /// <inheritdoc />
    public bool RemovePreview(string previewId)
    {
        return _previews.TryRemove(previewId, out _);
    }

    private static string GeneratePreviewId() =>
        "preview_" + Guid.NewGuid().ToString("N")[..8];

    private static string StripArmExpression(string name)
    {
        if (name.StartsWith('[') && name.EndsWith(']'))
        {
            var inner = name[1..^1];
            var parenIndex = inner.IndexOf('(');
            if (parenIndex >= 0)
            {
                var paramContent = inner[(parenIndex + 1)..].TrimEnd(')');
                paramContent = paramContent.Trim('\'', '"');
                return paramContent;
            }
        }

        return name;
    }

    private static string ExtractResourceNameFromDependsOn(string dependsOnValue)
    {
        if (dependsOnValue.StartsWith('[') && dependsOnValue.EndsWith(']'))
        {
            var inner = dependsOnValue[1..^1];
            var lastComma = inner.LastIndexOf(',');
            if (lastComma >= 0)
            {
                var namePart = inner[(lastComma + 1)..].Trim().TrimEnd(')').Trim('\'', '"', ' ');
                return namePart;
            }
        }

        return dependsOnValue;
    }

    private static void ExtractProperties(
        JsonElement resourceElement,
        string sourceType,
        string? friendlyName,
        Dictionary<string, object?> extractedProperties,
        List<string> unmappedProperties)
    {
        if (!resourceElement.TryGetProperty("properties", out var propertiesElement)
            || propertiesElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        switch (friendlyName)
        {
            case AzureResourceTypes.KeyVault:
                ExtractKeyVaultProperties(propertiesElement, extractedProperties, unmappedProperties);
                break;

            case AzureResourceTypes.StorageAccount:
                ExtractStorageAccountProperties(resourceElement, propertiesElement, extractedProperties, unmappedProperties);
                break;

            default:
                foreach (var prop in propertiesElement.EnumerateObject())
                {
                    unmappedProperties.Add(prop.Name);
                }

                break;
        }
    }

    private static void ExtractKeyVaultProperties(
        JsonElement properties,
        Dictionary<string, object?> extracted,
        List<string> unmapped)
    {
        if (properties.TryGetProperty("sku", out var sku)
            && sku.TryGetProperty("name", out var skuName))
        {
            extracted["skuName"] = skuName.GetString();
        }

        foreach (var prop in properties.EnumerateObject())
        {
            if (prop.Name is not "sku")
            {
                unmapped.Add(prop.Name);
            }
        }
    }

    private static void ExtractStorageAccountProperties(
        JsonElement resourceElement,
        JsonElement properties,
        Dictionary<string, object?> extracted,
        List<string> unmapped)
    {
        if (resourceElement.TryGetProperty("sku", out var sku)
            && sku.TryGetProperty("name", out var skuName))
        {
            extracted["skuName"] = skuName.GetString();
        }

        if (resourceElement.TryGetProperty("kind", out var kind))
        {
            extracted["kind"] = kind.GetString();
        }

        foreach (var prop in properties.EnumerateObject())
        {
            unmapped.Add(prop.Name);
        }
    }
}
