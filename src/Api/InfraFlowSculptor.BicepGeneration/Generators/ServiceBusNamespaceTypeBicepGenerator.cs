using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Service Bus Namespace (<c>Microsoft.ServiceBus/namespaces@2022-10-01-preview</c>).
/// </summary>
public sealed class ServiceBusNamespaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ServiceBusNamespace;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ServiceBusNamespace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("serviceBusNamespace", "ServiceBusNamespace", ResourceTypeName)
            .Import("./types.bicep", "SkuName", "TlsVersion")
            .Param("location", BicepType.String, "Azure region for the Service Bus Namespace")
            .Param("name", BicepType.String, "Name of the Service Bus Namespace")
            .Param("sku", BicepType.Custom("SkuName"), "SKU name for the Service Bus Namespace",
                defaultValue: new BicepStringLiteral("Standard"))
            .Param("capacity", BicepType.Int, "Messaging units capacity (Premium tier only, 1-16)",
                defaultValue: new BicepIntLiteral(1))
            .Param("zoneRedundant", BicepType.Bool, "Whether zone redundancy is enabled (Premium tier only)",
                defaultValue: new BicepBoolLiteral(false))
            .Param("disableLocalAuth", BicepType.Bool, "Whether local (SAS key) authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("minimumTlsVersion", BicepType.Custom("TlsVersion"), "Minimum TLS version",
                defaultValue: new BicepStringLiteral("1.2"))
            .Resource("serviceBusNamespace", "Microsoft.ServiceBus/namespaces@2022-10-01-preview")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("sku", sku => sku
                .Property("name", new BicepReference("sku"))
                .Property("tier", new BicepReference("sku"))
                .Property("capacity", new BicepConditionalExpression(
                    new BicepRawExpression("sku == 'Premium'"),
                    new BicepReference("capacity"),
                    new BicepIntLiteral(0))))
            .Property("properties", props => props
                .Property("zoneRedundant", new BicepReference("zoneRedundant"))
                .Property("disableLocalAuth", new BicepReference("disableLocalAuth"))
                .Property("minimumTlsVersion", new BicepReference("minimumTlsVersion")))
            .Output("id", BicepType.String, new BicepRawExpression("serviceBusNamespace.id"),
                description: "The resource ID of the Service Bus Namespace")
            .Output("defaultConnectionString", BicepType.String,
                new BicepRawExpression("listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString"),
                description: "The default primary connection string")
            .ExportedType("SkuName",
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium'"),
                description: "SKU name for the Service Bus Namespace")
            .ExportedType("TlsVersion",
                new BicepRawExpression("'1.0' | '1.1' | '1.2'"),
                description: "Minimum TLS version for the Service Bus Namespace")
            .Build();
    }

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
