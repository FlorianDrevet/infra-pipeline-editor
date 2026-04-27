using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Event Hub Namespace (<c>Microsoft.EventHub/namespaces@2024-01-01</c>).
/// </summary>
public sealed class EventHubNamespaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.EventHubNamespace;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.EventHubNamespace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("eventHubNamespace", "EventHubNamespace", ResourceTypeName)
            .Import("./types.bicep", "SkuName", "TlsVersion")
            .Param("location", BicepType.String, "Azure region for the Event Hub Namespace")
            .Param("name", BicepType.String, "Name of the Event Hub Namespace")
            .Param("sku", BicepType.Custom("SkuName"), "SKU name for the Event Hub Namespace",
                defaultValue: new BicepStringLiteral("Standard"))
            .Param("capacity", BicepType.Int, "Throughput or processing units capacity",
                defaultValue: new BicepIntLiteral(1))
            .Param("zoneRedundant", BicepType.Bool, "Whether zone redundancy is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("disableLocalAuth", BicepType.Bool, "Whether local (SAS key) authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("minimumTlsVersion", BicepType.Custom("TlsVersion"), "Minimum TLS version",
                defaultValue: new BicepStringLiteral("1.2"))
            .Param("autoInflateEnabled", BicepType.Bool, "Whether auto-inflate is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("maxThroughputUnits", BicepType.Int, "Maximum throughput units when auto-inflate is enabled (0-40)",
                defaultValue: new BicepIntLiteral(0))
            .Resource("eventHubNamespace", "Microsoft.EventHub/namespaces@2024-01-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("sku", sku => sku
                .Property("name", new BicepReference("sku"))
                .Property("tier", new BicepReference("sku"))
                .Property("capacity", new BicepReference("capacity")))
            .Property("properties", props => props
                .Property("zoneRedundant", new BicepReference("zoneRedundant"))
                .Property("disableLocalAuthentication", new BicepReference("disableLocalAuth"))
                .Property("minimumTlsVersion", new BicepReference("minimumTlsVersion"))
                .Property("isAutoInflateEnabled", new BicepReference("autoInflateEnabled"))
                .Property("maximumThroughputUnits", new BicepConditionalExpression(
                    new BicepReference("autoInflateEnabled"),
                    new BicepReference("maxThroughputUnits"),
                    new BicepIntLiteral(0))))
            .Output("id", BicepType.String, new BicepRawExpression("eventHubNamespace.id"),
                description: "The resource ID of the Event Hub Namespace")
            .Output("nameOutput", BicepType.String, new BicepRawExpression("eventHubNamespace.name"),
                description: "The name of the Event Hub Namespace")
            .ExportedType("SkuName",
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium'"),
                description: "SKU name for the Event Hub Namespace")
            .ExportedType("TlsVersion",
                new BicepRawExpression("'1.0' | '1.1' | '1.2'"),
                description: "Minimum TLS version for the Event Hub Namespace")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "eventHubNamespace",
            ModuleFileName = "eventHubNamespace",
            ModuleFolderName = "EventHubNamespace",
            ModuleBicepContent = EventHubModuleTemplate,
            ModuleTypesBicepContent = EventHubTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string EventHubTypesTemplate = """
        @export()
        @description('SKU name for the Event Hub Namespace')
        type SkuName = 'Basic' | 'Standard' | 'Premium'

        @export()
        @description('Minimum TLS version for the Event Hub Namespace')
        type TlsVersion = '1.0' | '1.1' | '1.2'
        """;

    private const string EventHubModuleTemplate = """
        import { SkuName, TlsVersion } from './types.bicep'

        @description('Azure region for the Event Hub Namespace')
        param location string

        @description('Name of the Event Hub Namespace')
        param name string

        @description('SKU name for the Event Hub Namespace')
        param sku SkuName = 'Standard'

        @description('Throughput or processing units capacity')
        param capacity int = 1

        @description('Whether zone redundancy is enabled')
        param zoneRedundant bool = false

        @description('Whether local (SAS key) authentication is disabled')
        param disableLocalAuth bool = false

        @description('Minimum TLS version')
        param minimumTlsVersion TlsVersion = '1.2'

        @description('Whether auto-inflate is enabled')
        param autoInflateEnabled bool = false

        @description('Maximum throughput units when auto-inflate is enabled (0-40)')
        param maxThroughputUnits int = 0

        resource eventHubNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
          name: name
          location: location
          sku: {
            name: sku
            tier: sku
            capacity: capacity
          }
          properties: {
            zoneRedundant: zoneRedundant
            disableLocalAuthentication: disableLocalAuth
            minimumTlsVersion: minimumTlsVersion
            isAutoInflateEnabled: autoInflateEnabled
            maximumThroughputUnits: autoInflateEnabled ? maxThroughputUnits : 0
          }
        }

        @description('The resource ID of the Event Hub Namespace')
        output id string = eventHubNamespace.id

        @description('The name of the Event Hub Namespace')
        output nameOutput string = eventHubNamespace.name
        """;
}
