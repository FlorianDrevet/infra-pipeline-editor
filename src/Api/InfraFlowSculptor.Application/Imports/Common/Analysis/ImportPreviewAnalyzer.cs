using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.Imports.Common.Arm;
using InfraFlowSculptor.Application.Imports.Common.Constants;
using InfraFlowSculptor.Application.Imports.Common.Properties;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Imports.Common.Analysis;

/// <summary>
/// Analyzes ARM templates into a normalized import preview result.
/// </summary>
public sealed class ImportPreviewAnalyzer : IImportPreviewAnalyzer
{
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
            SourceFormat = IacSourceFormat.ArmJson,
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

        var (extractedProperties, unmappedProperties) = ExtractProperties(armResource, friendlyName);

        if (!isMapped)
        {
            unsupported.Add(sourceName);
            gaps.Add(new ImportPreviewGapResult
            {
                Severity = ImportPreviewGapSeverity.Warning,
                Category = ImportPreviewGapCategory.UnsupportedResource,
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
        IReadOnlyList<string> unmappedProperties,
        List<ImportPreviewGapResult> gaps)
    {
        foreach (var unmappedProperty in unmappedProperties)
        {
            gaps.Add(new ImportPreviewGapResult
            {
                Severity = ImportPreviewGapSeverity.Info,
                Category = ImportPreviewGapCategory.UnmappedProperty,
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
                ImportDependencyType.DependsOn));
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

    private static (IReadOnlyDictionary<string, object?> Extracted, IReadOnlyList<string> Unmapped) ExtractProperties(
        ArmResource armResource,
        string? friendlyName)
    {
        if (armResource.Properties is not { ValueKind: JsonValueKind.Object } propertiesElement)
        {
            return (new Dictionary<string, object?>(), []);
        }

        IExtractedResourceProperties? typed = friendlyName switch
        {
            AzureResourceTypes.KeyVault => KeyVaultExtractedProperties.FromArm(propertiesElement),
            AzureResourceTypes.StorageAccount => StorageAccountExtractedProperties.FromArm(armResource, propertiesElement),
            _ => null,
        };

        if (typed is not null)
        {
            return (typed.ToDictionary(), typed.UnmappedProperties);
        }

        // Fallback: all properties are unmapped for unknown types.
        var unmapped = propertiesElement.EnumerateObject()
            .Select(p => p.Name)
            .ToList();
        return (new Dictionary<string, object?>(), unmapped);
    }
}