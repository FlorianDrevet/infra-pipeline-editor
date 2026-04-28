using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.Imports.Common.Arm;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Analyzes ARM templates into a normalized import preview result.
/// </summary>
public sealed class ImportPreviewAnalyzer : IImportPreviewAnalyzer
{
    private const string ArmJsonSourceFormat = "arm-json";

    private static readonly JsonSerializerOptions ArmSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc />
    public ImportPreviewAnalysisResult AnalyzeArmTemplate(string sourceContent)
    {
        ArgumentNullException.ThrowIfNull(sourceContent);

        var template = JsonSerializer.Deserialize<ArmTemplateDocument>(sourceContent, ArmSerializerOptions)
                       ?? throw new JsonException("ARM template deserialization returned null.");

        var resources = new List<ImportedResourceAnalysisResult>();
        var dependencies = new List<ImportedDependencyAnalysisResult>();
        var gaps = new List<ImportPreviewGapResult>();
        var unsupported = new List<string>();
        var metadata = ExtractMetadata(template);

        if (template.Resources is not null)
        {
            foreach (var armResource in template.Resources)
            {
                AnalyzeResource(armResource, resources, dependencies, gaps, unsupported);
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

    private static Dictionary<string, string> ExtractMetadata(ArmTemplateDocument template)
    {
        var metadata = new Dictionary<string, string>();

        if (template.Schema is not null)
        {
            metadata["schema"] = template.Schema;
        }

        if (template.ContentVersion is not null)
        {
            metadata["contentVersion"] = template.ContentVersion;
        }

        return metadata;
    }

    private static void AnalyzeResource(
        ArmResource armResource,
        List<ImportedResourceAnalysisResult> resources,
        List<ImportedDependencyAnalysisResult> dependencies,
        List<ImportPreviewGapResult> gaps,
        List<string> unsupported)
    {
        var sourceType = armResource.Type ?? string.Empty;
        var sourceName = StripArmExpression(armResource.Name ?? string.Empty);

        var isMapped = AzureResourceTypes.ArmTypeToFriendlyName.TryGetValue(sourceType, out var friendlyName);

        var extractedProperties = new Dictionary<string, object?>();
        var unmappedProperties = new List<string>();

        ExtractProperties(armResource, friendlyName, extractedProperties, unmappedProperties);

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

        AddUnmappedPropertyGaps(sourceName, unmappedProperties, gaps);

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

        ExtractDependencies(armResource, sourceName, dependencies);
    }

    private static void AddUnmappedPropertyGaps(
        string sourceName,
        List<string> unmappedProperties,
        List<ImportPreviewGapResult> gaps)
    {
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
    }

    private static void ExtractDependencies(
        ArmResource armResource,
        string sourceName,
        List<ImportedDependencyAnalysisResult> dependencies)
    {
        if (armResource.DependsOn is null)
        {
            return;
        }

        foreach (var dependencyValue in armResource.DependsOn)
        {
            var targetName = ExtractResourceNameFromDependsOn(dependencyValue);

            dependencies.Add(new ImportedDependencyAnalysisResult(
                sourceName,
                targetName,
                "dependsOn"));
        }
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
        ArmResource armResource,
        string? friendlyName,
        Dictionary<string, object?> extractedProperties,
        List<string> unmappedProperties)
    {
        if (armResource.Properties is not { ValueKind: JsonValueKind.Object } propertiesElement)
        {
            return;
        }

        switch (friendlyName)
        {
            case AzureResourceTypes.KeyVault:
                ExtractKeyVaultProperties(propertiesElement, extractedProperties, unmappedProperties);
                break;

            case AzureResourceTypes.StorageAccount:
                ExtractStorageAccountProperties(armResource, propertiesElement, extractedProperties, unmappedProperties);
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

        unmappedProperties.AddRange(
            properties.EnumerateObject()
                .Where(property => property.Name is not "sku")
                .Select(property => property.Name));
    }

    private static void ExtractStorageAccountProperties(
        ArmResource armResource,
        JsonElement properties,
        Dictionary<string, object?> extractedProperties,
        List<string> unmappedProperties)
    {
        if (armResource.Sku?.Name is not null)
        {
            extractedProperties["skuName"] = armResource.Sku.Name;
        }

        if (armResource.Kind is not null)
        {
            extractedProperties["kind"] = armResource.Kind;
        }

        foreach (var property in properties.EnumerateObject())
        {
            unmappedProperties.Add(property.Name);
        }
    }
}