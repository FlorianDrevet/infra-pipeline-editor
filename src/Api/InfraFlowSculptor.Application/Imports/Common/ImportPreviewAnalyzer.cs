using System.Text.Json;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Analyzes ARM templates into a normalized import preview result.
/// </summary>
public sealed class ImportPreviewAnalyzer : IImportPreviewAnalyzer
{
    private const string ArmJsonSourceFormat = "arm-json";

    /// <inheritdoc />
    public ImportPreviewAnalysisResult AnalyzeArmTemplate(string sourceContent)
    {
        ArgumentNullException.ThrowIfNull(sourceContent);

        using var doc = JsonDocument.Parse(sourceContent);
        var root = doc.RootElement;

        var resources = new List<ImportedResourceAnalysisResult>();
        var dependencies = new List<ImportedDependencyAnalysisResult>();
        var gaps = new List<ImportPreviewGapResult>();
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

        if (root.TryGetProperty("resources", out var resourcesArray)
            && resourcesArray.ValueKind == JsonValueKind.Array)
        {
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

                ExtractProperties(resourceElement, friendlyName, extractedProperties, unmappedProperties);

                if (!isMapped)
                {
                    unsupported.Add(sourceName);
                    gaps.Add(new ImportPreviewGapResult
                    {
                        Severity = ImportPreviewGapSeverity.Warning,
                        Category = "unsupported_resource",
                        Message = $"Resource type '{sourceType}' is not supported by InfraFlowSculptor.",
                        SourceResourceName = sourceName,
                    });
                }

                foreach (var unmappedProperty in unmappedProperties)
                {
                    gaps.Add(new ImportPreviewGapResult
                    {
                        Severity = ImportPreviewGapSeverity.Info,
                        Category = "unmapped_property",
                        Message = $"Property '{unmappedProperty}' on '{sourceName}' is auto-managed by InfraFlowSculptor.",
                        SourceResourceName = sourceName,
                    });
                }

                resources.Add(new ImportedResourceAnalysisResult
                {
                    SourceType = sourceType,
                    SourceName = sourceName,
                    MappedResourceType = isMapped ? friendlyName : null,
                    MappedName = isMapped ? sourceName : null,
                    Confidence = isMapped ? ImportPreviewMappingConfidence.High : ImportPreviewMappingConfidence.Low,
                    ExtractedProperties = extractedProperties,
                    UnmappedProperties = unmappedProperties,
                });

                if (resourceElement.TryGetProperty("dependsOn", out var dependsOn)
                    && dependsOn.ValueKind == JsonValueKind.Array)
                {
                    foreach (var dependencyElement in dependsOn.EnumerateArray())
                    {
                        var dependencyValue = dependencyElement.GetString() ?? string.Empty;
                        var targetName = ExtractResourceNameFromDependsOn(dependencyValue);

                        dependencies.Add(new ImportedDependencyAnalysisResult(
                            sourceName,
                            targetName,
                            "dependsOn"));
                    }
                }
            }
        }

        var mappedCount = resources.Count(resource => resource.MappedResourceType is not null);

        return new ImportPreviewAnalysisResult
        {
            SourceFormat = ArmJsonSourceFormat,
            Resources = resources,
            Dependencies = dependencies,
            Metadata = metadata,
            Gaps = gaps,
            UnsupportedResources = unsupported,
            Summary = BuildSummary(resources.Count, mappedCount, unsupported.Count),
        };
    }

    private static string BuildSummary(int parsedResourceCount, int mappedResourceCount, int unsupportedCount)
    {
        return $"Parsed {parsedResourceCount} resource(s): {mappedResourceCount} mapped, {unsupportedCount} unsupported.";
    }

    private static string StripArmExpression(string name)
    {
        if (name.StartsWith('[') && name.EndsWith(']'))
        {
            var inner = name[1..^1];
            var parenthesisIndex = inner.IndexOf('(');
            if (parenthesisIndex >= 0)
            {
                var parameterContent = inner[(parenthesisIndex + 1)..].TrimEnd(')');
                parameterContent = parameterContent.Trim('\'', '"');
                return parameterContent;
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
                return inner[(lastComma + 1)..].Trim().TrimEnd(')').Trim('\'', '"', ' ');
            }
        }

        return dependsOnValue;
    }

    private static void ExtractProperties(
        JsonElement resourceElement,
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
                foreach (var property in propertiesElement.EnumerateObject())
                {
                    unmappedProperties.Add(property.Name);
                }

                break;
        }
    }

    private static void ExtractKeyVaultProperties(
        JsonElement properties,
        Dictionary<string, object?> extractedProperties,
        List<string> unmappedProperties)
    {
        if (properties.TryGetProperty("sku", out var sku)
            && sku.TryGetProperty("name", out var skuName))
        {
            extractedProperties["skuName"] = skuName.GetString();
        }

        foreach (var property in properties.EnumerateObject())
        {
            if (property.Name is not "sku")
            {
                unmappedProperties.Add(property.Name);
            }
        }
    }

    private static void ExtractStorageAccountProperties(
        JsonElement resourceElement,
        JsonElement properties,
        Dictionary<string, object?> extractedProperties,
        List<string> unmappedProperties)
    {
        if (resourceElement.TryGetProperty("sku", out var sku)
            && sku.TryGetProperty("name", out var skuName))
        {
            extractedProperties["skuName"] = skuName.GetString();
        }

        if (resourceElement.TryGetProperty("kind", out var kind))
        {
            extractedProperties["kind"] = kind.GetString();
        }

        foreach (var property in properties.EnumerateObject())
        {
            unmappedProperties.Add(property.Name);
        }
    }
}