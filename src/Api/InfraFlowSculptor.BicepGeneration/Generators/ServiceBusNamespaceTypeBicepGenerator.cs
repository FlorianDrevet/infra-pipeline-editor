using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Service Bus Namespace (<c>Microsoft.ServiceBus/namespaces@2022-10-01-preview</c>).
/// </summary>
public sealed class ServiceBusNamespaceTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.ServiceBus/namespaces";

    /// <inheritdoc />
    public string ResourceTypeName => "ServiceBusNamespace";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "serviceBusNamespace",
            ModuleFileName = "serviceBusNamespace",
            ModuleFolderName = "ServiceBusNamespace",
            ModuleBicepContent = ServiceBusModuleTemplate,
            ModuleTypesBicepContent = ServiceBusTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ServiceBusTypesTemplate = """
        @export()
        @description('SKU name for the Service Bus Namespace')
        type SkuName = 'Basic' | 'Standard' | 'Premium'

        @export()
        @description('Minimum TLS version for the Service Bus Namespace')
        type TlsVersion = '1.0' | '1.1' | '1.2'
        """;

    private const string ServiceBusModuleTemplate = """
        import { SkuName, TlsVersion } from './types.bicep'

        @description('Azure region for the Service Bus Namespace')
        param location string

        @description('Name of the Service Bus Namespace')
        param name string

        @description('SKU name for the Service Bus Namespace')
        param sku SkuName = 'Standard'

        @description('Messaging units capacity (Premium tier only, 1-16)')
        param capacity int = 1

        @description('Whether zone redundancy is enabled (Premium tier only)')
        param zoneRedundant bool = false

        @description('Whether local (SAS key) authentication is disabled')
        param disableLocalAuth bool = false

        @description('Minimum TLS version')
        param minimumTlsVersion TlsVersion = '1.2'

        resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
          name: name
          location: location
          sku: {
            name: sku
            tier: sku
            capacity: sku == 'Premium' ? capacity : 0
          }
          properties: {
            zoneRedundant: zoneRedundant
            disableLocalAuth: disableLocalAuth
            minimumTlsVersion: minimumTlsVersion
          }
        }

        @description('The resource ID of the Service Bus Namespace')
        output id string = serviceBusNamespace.id

        @description('The default primary connection string')
        output defaultConnectionString string = listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
        """;
}
