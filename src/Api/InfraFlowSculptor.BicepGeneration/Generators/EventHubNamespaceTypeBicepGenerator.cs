using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Generators.Helpers;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Event Hub Namespace (<c>Microsoft.EventHub/namespaces@2024-01-01</c>).
/// </summary>
public sealed class EventHubNamespaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "eventHubNamespace";
    private const string ModuleFolderName = "EventHubNamespace";
    private const string ModuleFileName = "eventHubNamespace";
    private const string SkuParameterName = "sku";
    private const string CapacityParameterName = "capacity";
    private const string ZoneRedundantParameterName = "zoneRedundant";
    private const string DisableLocalAuthParameterName = "disableLocalAuth";
    private const string MinimumTlsVersionParameterName = "minimumTlsVersion";
    private const string AutoInflateEnabledParameterName = "autoInflateEnabled";
    private const string MaxThroughputUnitsParameterName = "maxThroughputUnits";
    private const string ResourceSymbol = "eventHubNamespace";
    private const string EventHubNamespaceArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.EventHubNamespaceArmType;
    private const string TierPropertyName = "tier";
    private const string DisableLocalAuthenticationPropertyName = "disableLocalAuthentication";
    private const string IsAutoInflateEnabledPropertyName = "isAutoInflateEnabled";
    private const string MaximumThroughputUnitsPropertyName = "maximumThroughputUnits";
    private const string NameOutputName = "nameOutput";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.EventHubNamespaceType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.EventHubNamespace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .AddMessagingNamespaceTypeImport()
            .Param(LocationParameterName, BicepType.String, "Azure region for the Event Hub Namespace")
            .Param(NameParameterName, BicepType.String, "Name of the Event Hub Namespace")
            .Param(SkuParameterName, BicepType.Custom(MessagingNamespaceBicepGeneratorHelper.SkuNameTypeName), "SKU name for the Event Hub Namespace",
                defaultValue: new BicepStringLiteral(MessagingNamespaceBicepGeneratorHelper.DefaultSkuName))
            .Param(CapacityParameterName, BicepType.Int, "Throughput or processing units capacity",
                defaultValue: new BicepIntLiteral(1))
            .Param(ZoneRedundantParameterName, BicepType.Bool, "Whether zone redundancy is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(DisableLocalAuthParameterName, BicepType.Bool, "Whether local (SAS key) authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(MinimumTlsVersionParameterName, BicepType.Custom(MessagingNamespaceBicepGeneratorHelper.TlsVersionTypeName), "Minimum TLS version",
                defaultValue: new BicepStringLiteral(MessagingNamespaceBicepGeneratorHelper.DefaultMinimumTlsVersion))
            .Param(AutoInflateEnabledParameterName, BicepType.Bool, "Whether auto-inflate is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(MaxThroughputUnitsParameterName, BicepType.Int, "Maximum throughput units when auto-inflate is enabled (0-40)",
                defaultValue: new BicepIntLiteral(0))
            .Resource(ResourceSymbol, EventHubNamespaceArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName))
                .Property(TierPropertyName, new BicepReference(SkuParameterName))
                .Property(CapacityParameterName, new BicepReference(CapacityParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(ZoneRedundantParameterName, new BicepReference(ZoneRedundantParameterName))
                .Property(DisableLocalAuthenticationPropertyName, new BicepReference(DisableLocalAuthParameterName))
                .Property(MinimumTlsVersionParameterName, new BicepReference(MinimumTlsVersionParameterName))
                .Property(IsAutoInflateEnabledPropertyName, new BicepReference(AutoInflateEnabledParameterName))
                .Property(MaximumThroughputUnitsPropertyName, new BicepConditionalExpression(
                    new BicepReference(AutoInflateEnabledParameterName),
                    new BicepReference(MaxThroughputUnitsParameterName),
                    new BicepIntLiteral(0))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Event Hub Namespace")
            .Output(NameOutputName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Event Hub Namespace")
            .AddMessagingNamespaceExportedTypes("Event Hub Namespace")
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
            ModuleBicepContent = EventHubModuleTemplate,
            ModuleTypesBicepContent = EventHubTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string EventHubTypesTemplate = MessagingNamespaceBicepGeneratorHelper.BuildTypesTemplate("Event Hub Namespace");

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

        resource eventHubNamespace '{{EventHubNamespaceArmType}}' = {
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
