using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Document Intelligence
/// (<c>Microsoft.CognitiveServices/accounts@2024-10-01</c> with kind FormRecognizer).
/// </summary>
public sealed class DocumentIntelligenceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "documentIntelligence";
    private const string ModuleFolderName = "DocumentIntelligence";
    private const string ModuleFileName = "documentIntelligence";
    private const string SkuParameterName = "sku";
    private const string CustomSubDomainNameParameterName = "customSubDomainName";
    private const string PublicNetworkAccessParameterName = "publicNetworkAccess";
    private const string DisableLocalAuthParameterName = "disableLocalAuth";
    private const string ResourceSymbol = "docIntel";
    private const string DocumentIntelligenceArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.DocumentIntelligenceArmType;
    private const string KindPropertyName = "kind";
    private const string FormRecognizerKind = "FormRecognizer";
    private const string PublicNetworkAccessPropertyName = "publicNetworkAccess";
    private const string DisableLocalAuthPropertyName = "disableLocalAuth";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string EndpointExpression = ResourceSymbol + ".properties.endpoint";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.DocumentIntelligenceType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.DocumentIntelligence;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Document Intelligence resource")
            .Param(NameParameterName, BicepType.String, "Name of the Document Intelligence resource")
            .Param(SkuParameterName, BicepType.String, "Pricing SKU (F0 or S0)",
                defaultValue: new BicepStringLiteral("S0"))
            .Param(CustomSubDomainNameParameterName, BicepType.String, "Custom sub-domain name for the Cognitive Services account",
                defaultValue: new BicepStringLiteral(""))
            .Param(PublicNetworkAccessParameterName, BicepType.String, "Whether public network access is enabled or disabled",
                defaultValue: new BicepStringLiteral("Enabled"))
            .Param(DisableLocalAuthParameterName, BicepType.Bool, "Whether to disable local (key-based) authentication",
                defaultValue: new BicepBoolLiteral(false))
            .Resource(ResourceSymbol, DocumentIntelligenceArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(KindPropertyName, new BicepStringLiteral(FormRecognizerKind))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(CustomSubDomainNameParameterName, new BicepConditionalExpression(
                    new BicepRawExpression($"!empty({CustomSubDomainNameParameterName})"),
                    new BicepReference(CustomSubDomainNameParameterName),
                    new BicepRawExpression("null")))
                .Property(PublicNetworkAccessPropertyName, new BicepReference(PublicNetworkAccessParameterName))
                .Property(DisableLocalAuthPropertyName, new BicepReference(DisableLocalAuthParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Document Intelligence account")
            .Output("endpoint", BicepType.String, new BicepRawExpression(EndpointExpression),
                description: "The endpoint URL of the Document Intelligence account")
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
            ModuleBicepContent = DocumentIntelligenceModuleTemplate,
            ModuleTypesBicepContent = string.Empty,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string DocumentIntelligenceModuleTemplate = $$"""
        @description('Azure region for the Document Intelligence resource')
        param location string

        @description('Name of the Document Intelligence resource')
        param name string

        @description('Pricing SKU (F0 or S0)')
        param sku string = 'S0'

        @description('Custom sub-domain name for the Cognitive Services account')
        param customSubDomainName string = ''

        @description('Whether public network access is enabled or disabled')
        param publicNetworkAccess string = 'Enabled'

        @description('Whether to disable local (key-based) authentication')
        param disableLocalAuth bool = false

        resource docIntel '{{DocumentIntelligenceArmType}}' = {
          name: name
          location: location
          kind: 'FormRecognizer'
          sku: {
            name: sku
          }
          properties: {
            customSubDomainName: !empty(customSubDomainName) ? customSubDomainName : null
            publicNetworkAccess: publicNetworkAccess
            disableLocalAuth: disableLocalAuth
          }
        }

        @description('The resource ID of the Document Intelligence account')
        output id string = docIntel.id

        @description('The endpoint URL of the Document Intelligence account')
        output endpoint string = docIntel.properties.endpoint
        """;
}
