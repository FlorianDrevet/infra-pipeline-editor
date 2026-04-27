using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class EventHubNamespaceTypeBicepGeneratorTests
{
    private readonly EventHubNamespaceTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-evhns",
        Type = AzureResourceTypes.ArmTypes.EventHubNamespace,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "evhns",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.EventHubNamespace);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.EventHubNamespace);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("eventHubNamespace");
        spec.ModuleFolderName.Should().Be("EventHubNamespace");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.EventHubNamespace);
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
    public void Given_Resource_When_GenerateSpec_Then_HasNineParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(9);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "sku").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Standard");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCapacityParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "capacity").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(1);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasZoneRedundantParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "zoneRedundant").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDisableLocalAuthParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "disableLocalAuth").Subject;
        param.Type.Should().Be(BicepType.Bool);
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
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("1.2");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAutoInflateEnabledParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "autoInflateEnabled").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMaxThroughputUnitsParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "maxThroughputUnits").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(0);
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("eventHubNamespace");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.EventHub/namespaces@2024-01-01");
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
    public void Given_Resource_When_GenerateSpec_Then_SkuNameAndTierReuseSkuParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("sku");
        sku.Properties[1].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("sku");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsFiveProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(5);
        properties.Properties[0].Key.Should().Be("zoneRedundant");
        properties.Properties[1].Key.Should().Be("disableLocalAuthentication");
        properties.Properties[2].Key.Should().Be("minimumTlsVersion");
        properties.Properties[3].Key.Should().Be("isAutoInflateEnabled");
        properties.Properties[4].Key.Should().Be("maximumThroughputUnits");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_MaxThroughputUnitsUsesConditionalExpression()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[4].Value.Should().BeOfType<BicepConditionalExpression>();
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

        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("eventHubNamespace.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "nameOutput").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("eventHubNamespace.name");
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
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Basic' | 'Standard' | 'Premium'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeTlsVersionIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "TlsVersion").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'1.0' | '1.1' | '1.2'");
    }

    // ── No companions / no variables ──

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
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.ModuleName.Should().Be("eventHubNamespace");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
        legacy.ModuleTypesBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SkuName, TlsVersion } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'Standard'");
        bicep.Should().Contain("param capacity int = 1");
        bicep.Should().Contain("param zoneRedundant bool = false");
        bicep.Should().Contain("param disableLocalAuth bool = false");
        bicep.Should().Contain("param minimumTlsVersion TlsVersion = '1.2'");
        bicep.Should().Contain("param autoInflateEnabled bool = false");
        bicep.Should().Contain("param maxThroughputUnits int = 0");
        bicep.Should().Contain("resource eventHubNamespace 'Microsoft.EventHub/namespaces@2024-01-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("name: sku");
        bicep.Should().Contain("tier: sku");
        bicep.Should().Contain("capacity: capacity");
        bicep.Should().Contain("zoneRedundant: zoneRedundant");
        bicep.Should().Contain("disableLocalAuthentication: disableLocalAuth");
        bicep.Should().Contain("minimumTlsVersion: minimumTlsVersion");
        bicep.Should().Contain("isAutoInflateEnabled: autoInflateEnabled");
        bicep.Should().Contain("autoInflateEnabled ? maxThroughputUnits : 0");
        bicep.Should().Contain("output id string = eventHubNamespace.id");
        bicep.Should().Contain("output nameOutput string = eventHubNamespace.name");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SkuName = 'Basic' | 'Standard' | 'Premium'");
        types.Should().Contain("type TlsVersion = '1.0' | '1.1' | '1.2'");
    }
}
