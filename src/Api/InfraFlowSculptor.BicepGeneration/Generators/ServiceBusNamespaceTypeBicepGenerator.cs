using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Service Bus Namespace (<c>Microsoft.ServiceBus/namespaces@2022-10-01-preview</c>).
/// </summary>
public sealed class ServiceBusNamespaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "serviceBusNamespace";
    private const string ModuleFolderName = "ServiceBusNamespace";
    private const string ModuleFileName = "serviceBusNamespace";
    private const string SkuNameTypeName = "SkuName";
    private const string TlsVersionTypeName = "TlsVersion";
    private const string SkuParameterName = "sku";
    private const string CapacityParameterName = "capacity";
    private const string ZoneRedundantParameterName = "zoneRedundant";
    private const string DisableLocalAuthParameterName = "disableLocalAuth";
    private const string MinimumTlsVersionParameterName = "minimumTlsVersion";
    private const string ResourceSymbol = "serviceBusNamespace";
    private const string ServiceBusNamespaceArmType = "Microsoft.ServiceBus/namespaces@2022-10-01-preview";
    private const string DefaultSkuName = "Standard";
    private const string DefaultMinimumTlsVersion = "1.2";
    private const string PremiumSkuValue = "Premium";
    private const string TierPropertyName = "tier";
    private const string DefaultConnectionStringOutputName = "defaultConnectionString";
    private const string PremiumCapacityConditionExpression = SkuParameterName + " == '" + PremiumSkuValue + "'";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string DefaultConnectionStringExpression = "listKeys('${" + ResourceSymbol + ".id}/AuthorizationRules/RootManageSharedAccessKey', " + ResourceSymbol + ".apiVersion).primaryConnectionString";
    private const string SkuNameUnion = "'Basic' | 'Standard' | 'Premium'";
    private const string TlsVersionUnion = "'1.0' | '1.1' | '1.2'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ServiceBusNamespaceType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ServiceBusNamespace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName, TlsVersionTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Service Bus Namespace")
            .Param(NameParameterName, BicepType.String, "Name of the Service Bus Namespace")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU name for the Service Bus Namespace",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(CapacityParameterName, BicepType.Int, "Messaging units capacity (Premium tier only, 1-16)",
                defaultValue: new BicepIntLiteral(1))
            .Param(ZoneRedundantParameterName, BicepType.Bool, "Whether zone redundancy is enabled (Premium tier only)",
                defaultValue: new BicepBoolLiteral(false))
            .Param(DisableLocalAuthParameterName, BicepType.Bool, "Whether local (SAS key) authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(MinimumTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version",
                defaultValue: new BicepStringLiteral(DefaultMinimumTlsVersion))
            .Resource(ResourceSymbol, ServiceBusNamespaceArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName))
                .Property(TierPropertyName, new BicepReference(SkuParameterName))
                .Property(CapacityParameterName, new BicepConditionalExpression(
                    new BicepRawExpression(PremiumCapacityConditionExpression),
                    new BicepReference(CapacityParameterName),
                    new BicepIntLiteral(0))))
            .Property(PropertiesPropertyName, props => props
                .Property(ZoneRedundantParameterName, new BicepReference(ZoneRedundantParameterName))
                .Property(DisableLocalAuthParameterName, new BicepReference(DisableLocalAuthParameterName))
                .Property(MinimumTlsVersionParameterName, new BicepReference(MinimumTlsVersionParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Service Bus Namespace")
            .Output(DefaultConnectionStringOutputName, BicepType.String,
                new BicepRawExpression(DefaultConnectionStringExpression),
                description: "The default primary connection string")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the Service Bus Namespace")
            .ExportedType(TlsVersionTypeName,
                new BicepRawExpression(TlsVersionUnion),
                description: "Minimum TLS version for the Service Bus Namespace")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleFileName,
            ModuleFolderName = ModuleFolderName,
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
