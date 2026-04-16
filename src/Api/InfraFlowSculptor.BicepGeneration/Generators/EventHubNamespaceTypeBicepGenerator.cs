using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Event Hub Namespace (<c>Microsoft.EventHub/namespaces@2024-01-01</c>).
/// </summary>
public sealed class EventHubNamespaceTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.EventHubNamespace;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.EventHubNamespace;

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

    private static readonly string EventHubModuleTemplate = $$"""
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

        resource eventHubNamespace 'Microsoft.EventHub/namespaces@{{AzureResourceTypes.ApiVersions.EventHubNamespace}}' = {
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
