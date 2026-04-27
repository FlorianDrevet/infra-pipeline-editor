using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class ServiceBusNamespaceTypeBicepGeneratorTests
{
    private readonly ServiceBusNamespaceTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-sbns",
        Type = AzureResourceTypes.ArmTypes.ServiceBusNamespace,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "sbns",
    };

    // ── Interface contracts ──

    [Fact]
    public void Given_Generator_Then_ImplementsIResourceTypeBicepSpecGenerator()
    {
        _sut.Should().BeAssignableTo<IResourceTypeBicepSpecGenerator>();
    }

    [Fact]
    public void Given_Generator_Then_ResourceTypeIsCorrectArmType()
    {
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.ServiceBusNamespace);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.ServiceBusNamespace);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("serviceBusNamespace");
        spec.ModuleFolderName.Should().Be("ServiceBusNamespace");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.ServiceBusNamespace);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSkuNameAndTlsVersionFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SkuName") &&
                i.Symbols.Contains("TlsVersion"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSevenParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(7);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.Description == "Azure region for the Service Bus Namespace" &&
                !p.IsSecure &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.Description == "Name of the Service Bus Namespace" &&
                !p.IsSecure &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "sku").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        param.Description.Should().Be("SKU name for the Service Bus Namespace");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Standard");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCapacityParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "capacity").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.Description.Should().Be("Messaging units capacity (Premium tier only, 1-16)");
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(1);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasZoneRedundantParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "zoneRedundant").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.Description.Should().Be("Whether zone redundancy is enabled (Premium tier only)");
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDisableLocalAuthParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "disableLocalAuth").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.Description.Should().Be("Whether local (SAS key) authentication is disabled");
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMinimumTlsVersionParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "minimumTlsVersion").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("TlsVersion");
        param.Description.Should().Be("Minimum TLS version");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("1.2");
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("serviceBusNamespace");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.ServiceBus/namespaces@2022-10-01-preview");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameLocationSkuProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(4);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("sku");
        spec.Resource.Body[3].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuObjectHasNameTierCapacity()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().HaveCount(3);
        sku.Properties[0].Key.Should().Be("name");
        sku.Properties[1].Key.Should().Be("tier");
        sku.Properties[2].Key.Should().Be("capacity");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuCapacityUsesConditionalExpression()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties[2].Value.Should().BeOfType<BicepConditionalExpression>();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsThreeProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(3);
        properties.Properties[0].Key.Should().Be("zoneRedundant");
        properties.Properties[1].Key.Should().Be("disableLocalAuth");
        properties.Properties[2].Key.Should().Be("minimumTlsVersion");
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTwoOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputIdIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs[0];
        output.Name.Should().Be("id");
        output.Type.Should().Be(BicepType.String);
        output.Description.Should().Be("The resource ID of the Service Bus Namespace");
        output.IsSecure.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputConnectionStringIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs[1];
        output.Name.Should().Be("defaultConnectionString");
        output.Type.Should().Be(BicepType.String);
        output.Description.Should().Be("The default primary connection string");
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Contain("listKeys(");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTwoExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeSkuNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SkuName").Subject;
        type.IsExported.Should().BeTrue();
        type.Description.Should().Be("SKU name for the Service Bus Namespace");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeTlsVersionIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "TlsVersion").Subject;
        type.IsExported.Should().BeTrue();
        type.Description.Should().Be("Minimum TLS version for the Service Bus Namespace");
    }

    // ── No companions, no variables, no secure params ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoCompanions()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Companions.Should().BeEmpty();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoVariables()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Variables.Should().BeEmpty();
    }

    // ── Legacy backward compatibility ──

    [Fact]
    public void Given_Resource_When_Generate_Then_ReturnsLegacyModule()
    {
        var module = _sut.Generate(CreateResource());

        module.ModuleName.Should().Be("serviceBusNamespace");
        module.ModuleFolderName.Should().Be("ServiceBusNamespace");
        module.ModuleBicepContent.Should().Contain("resource serviceBusNamespace");
        module.ModuleTypesBicepContent.Should().Contain("type SkuName");
        module.ModuleTypesBicepContent.Should().Contain("type TlsVersion");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        // Import
        bicep.Should().Contain("import { SkuName, TlsVersion } from './types.bicep'");

        // Params
        bicep.Should().Contain("@description('Azure region for the Service Bus Namespace')");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'Standard'");
        bicep.Should().Contain("param capacity int = 1");
        bicep.Should().Contain("param zoneRedundant bool = false");
        bicep.Should().Contain("param disableLocalAuth bool = false");
        bicep.Should().Contain("param minimumTlsVersion TlsVersion = '1.2'");

        // Resource
        bicep.Should().Contain("resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {");
        bicep.Should().Contain("  name: name");
        bicep.Should().Contain("  location: location");
        bicep.Should().Contain("  sku: {");
        bicep.Should().Contain("    name: sku");
        bicep.Should().Contain("    tier: sku");
        bicep.Should().Contain("sku == 'Premium' ? capacity : 0");
        bicep.Should().Contain("  properties: {");
        bicep.Should().Contain("    zoneRedundant: zoneRedundant");
        bicep.Should().Contain("    disableLocalAuth: disableLocalAuth");
        bicep.Should().Contain("    minimumTlsVersion: minimumTlsVersion");

        // Outputs
        bicep.Should().Contain("output id string = serviceBusNamespace.id");
        bicep.Should().Contain("output defaultConnectionString string =");
        bicep.Should().Contain("listKeys(");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('SKU name for the Service Bus Namespace')");
        types.Should().Contain("type SkuName =");
        types.Should().Contain("@description('Minimum TLS version for the Service Bus Namespace')");
        types.Should().Contain("type TlsVersion =");
    }
}
