using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class AppConfigurationTypeBicepGeneratorTests
{
    private readonly AppConfigurationTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-appconfig",
        Type = AzureResourceTypes.ArmTypes.AppConfiguration,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "appcs",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.AppConfiguration);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.AppConfiguration);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("appConfiguration");
        spec.ModuleFolderName.Should().Be("AppConfiguration");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.AppConfiguration);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSkuNameAndPublicNetworkAccessFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SkuName") &&
                i.Symbols.Contains("PublicNetworkAccess"));
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
            .Which.Value.Should().Be("standard");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSoftDeleteRetentionParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "softDeleteRetentionInDays").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(7);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnablePurgeProtectionParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "enablePurgeProtection").Subject;
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
    public void Given_Resource_When_GenerateSpec_Then_HasPublicNetworkAccessParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "publicNetworkAccess").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("PublicNetworkAccess");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Enabled");
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("appConfig");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.AppConfiguration/configurationStores@2023-03-01");
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
    public void Given_Resource_When_GenerateSpec_Then_SkuObjectHasNameOnly()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsFourProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(4);
        properties.Properties.Select(p => p.Key).Should()
            .Contain("softDeleteRetentionInDays")
            .And.Contain("enablePurgeProtection")
            .And.Contain("disableLocalAuth")
            .And.Contain("publicNetworkAccess");
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
            .Which.RawBicep.Should().Be("appConfig.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputEndpointIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "endpoint").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("appConfig.properties.endpoint");
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
            .Which.RawBicep.Should().Be("'free' | 'standard'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypePublicNetworkAccessIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "PublicNetworkAccess").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Enabled' | 'Disabled'");
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

        legacy.ModuleName.Should().Be("appConfiguration");
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

        bicep.Should().Contain("import { SkuName, PublicNetworkAccess } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'standard'");
        bicep.Should().Contain("param softDeleteRetentionInDays int = 7");
        bicep.Should().Contain("param enablePurgeProtection bool = false");
        bicep.Should().Contain("param disableLocalAuth bool = false");
        bicep.Should().Contain("param publicNetworkAccess PublicNetworkAccess = 'Enabled'");
        bicep.Should().Contain("resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("name: sku");
        bicep.Should().Contain("softDeleteRetentionInDays: softDeleteRetentionInDays");
        bicep.Should().Contain("enablePurgeProtection: enablePurgeProtection");
        bicep.Should().Contain("disableLocalAuth: disableLocalAuth");
        bicep.Should().Contain("publicNetworkAccess: publicNetworkAccess");
        bicep.Should().Contain("output id string = appConfig.id");
        bicep.Should().Contain("output endpoint string = appConfig.properties.endpoint");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SkuName = 'free' | 'standard'");
        types.Should().Contain("type PublicNetworkAccess = 'Enabled' | 'Disabled'");
    }
}
