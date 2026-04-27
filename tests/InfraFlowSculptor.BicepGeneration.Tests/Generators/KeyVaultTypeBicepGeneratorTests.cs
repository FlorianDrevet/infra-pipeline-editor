using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class KeyVaultTypeBicepGeneratorTests
{
    private readonly KeyVaultTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-kv",
        Type = AzureResourceTypes.ArmTypes.KeyVault,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "kv",
        Sku = "Standard",
        Properties = properties ?? new Dictionary<string, string>(),
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.KeyVault);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.KeyVault);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("keyVault");
        spec.ModuleFolderName.Should().Be("KeyVault");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.KeyVault);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSkuNameFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SkuName"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(3);
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

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("kv");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.KeyVault/vaults@2023-07-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasThreeTopLevelProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(3);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasEightProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(8);
        properties.Properties[0].Key.Should().Be("sku");
        properties.Properties[1].Key.Should().Be("tenantId");
        properties.Properties[2].Key.Should().Be("enableRbacAuthorization");
        properties.Properties[3].Key.Should().Be("enabledForDeployment");
        properties.Properties[4].Key.Should().Be("enabledForDiskEncryption");
        properties.Properties[5].Key.Should().Be("enabledForTemplateDeployment");
        properties.Properties[6].Key.Should().Be("enablePurgeProtection");
        properties.Properties[7].Key.Should().Be("enableSoftDelete");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuNestedObjectHasFamilyAndName()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var sku = properties.Properties[0].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().HaveCount(2);
        sku.Properties[0].Key.Should().Be("family");
        sku.Properties[0].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("A");
        sku.Properties[1].Key.Should().Be("name");
        sku.Properties[1].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("sku");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_TenantIdIsSubscriptionExpression()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[1].Value.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("subscription().tenantId");
    }

    // ── Dynamic boolean properties (defaults) ──

    [Fact]
    public void Given_ResourceWithNoProperties_When_GenerateSpec_Then_BooleanDefaultsAreApplied()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;

        // enableRbacAuthorization defaults to true
        properties.Properties[2].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
        // enabledForDeployment defaults to false
        properties.Properties[3].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
        // enabledForDiskEncryption defaults to false
        properties.Properties[4].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
        // enabledForTemplateDeployment defaults to false
        properties.Properties[5].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
        // enablePurgeProtection defaults to true
        properties.Properties[6].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
        // enableSoftDelete defaults to true
        properties.Properties[7].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
    }

    [Fact]
    public void Given_ResourceWithCustomProperties_When_GenerateSpec_Then_OverridesAreApplied()
    {
        var resource = CreateResource(new Dictionary<string, string>
        {
            ["enableRbacAuthorization"] = "false",
            ["enabledForDeployment"] = "true",
            ["enabledForDiskEncryption"] = "true",
            ["enabledForTemplateDeployment"] = "true",
            ["enablePurgeProtection"] = "false",
            ["enableSoftDelete"] = "false",
        });

        var spec = _sut.GenerateSpec(resource);

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;

        properties.Properties[2].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
        properties.Properties[3].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
        properties.Properties[4].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
        properties.Properties[5].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
        properties.Properties[6].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
        properties.Properties[7].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputIdIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("kv.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "name").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("kv.name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputVaultUriIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "vaultUri").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("kv.properties.vaultUri");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeSkuNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SkuName").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'premium' | 'standard'");
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

        legacy.ModuleName.Should().Be("keyVault");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
        legacy.ModuleTypesBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_ParametersDictContainsSku()
    {
        var resource = CreateResource();
        resource.Sku = "Standard";
        var legacy = _sut.Generate(resource);

        legacy.Parameters.Should().ContainKey("sku")
            .WhoseValue.Should().Be("standard");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_ResourceWithDefaults_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SkuName } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'standard'");
        bicep.Should().Contain("resource kv 'Microsoft.KeyVault/vaults@2023-07-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("family: 'A'");
        bicep.Should().Contain("name: sku");
        bicep.Should().Contain("tenantId: subscription().tenantId");
        bicep.Should().Contain("enableRbacAuthorization: true");
        bicep.Should().Contain("enabledForDeployment: false");
        bicep.Should().Contain("enabledForDiskEncryption: false");
        bicep.Should().Contain("enabledForTemplateDeployment: false");
        bicep.Should().Contain("enablePurgeProtection: true");
        bicep.Should().Contain("enableSoftDelete: true");
        bicep.Should().Contain("output id string = kv.id");
        bicep.Should().Contain("output name string = kv.name");
        bicep.Should().Contain("output vaultUri string = kv.properties.vaultUri");
    }

    [Fact]
    public void Given_ResourceWithOverrides_When_EmitModule_Then_ReflectsOverriddenValues()
    {
        var resource = CreateResource(new Dictionary<string, string>
        {
            ["enableRbacAuthorization"] = "false",
            ["enabledForDeployment"] = "true",
        });

        var spec = _sut.GenerateSpec(resource);
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("enableRbacAuthorization: false");
        bicep.Should().Contain("enabledForDeployment: true");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SkuName = 'premium' | 'standard'");
    }
}
