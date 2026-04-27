using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class ContainerRegistryTypeBicepGeneratorTests
{
    private readonly ContainerRegistryTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-acr",
        Type = AzureResourceTypes.ArmTypes.ContainerRegistry,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "cr",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.ContainerRegistry);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerRegistry);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("containerRegistry");
        spec.ModuleFolderName.Should().Be("ContainerRegistry");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerRegistry);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSixParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(6);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.Description == "Azure region for the Container Registry" &&
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
                p.Description == "Name of the Container Registry" &&
                !p.IsSecure &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var skuParam = spec.Parameters.Should().Contain(p => p.Name == "sku").Subject;
        skuParam.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        skuParam.Description.Should().Be("SKU of the Container Registry");
        skuParam.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Basic");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAdminUserEnabledParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "adminUserEnabled").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.Description.Should().Be("Whether the admin user is enabled");
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
        param.Description.Should().Be("Public network access setting");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Enabled");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasZoneRedundancyParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "zoneRedundancy").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.Description.Should().Be("Whether zone redundancy is enabled (Premium only)");
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("containerRegistry");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.ContainerRegistry/registries@2023-07-01");
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
    public void Given_Resource_When_GenerateSpec_Then_SkuObjectHasName()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsThreeProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(3);
        properties.Properties[0].Key.Should().Be("adminUserEnabled");
        properties.Properties[1].Key.Should().Be("publicNetworkAccess");
        properties.Properties[2].Key.Should().Be("zoneRedundancy");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ZoneRedundancyUsesConditionalExpression()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var zrProp = properties.Properties[2];
        zrProp.Value.Should().BeOfType<BicepConditionalExpression>();
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
        output.Description.Should().Be("The resource ID of the Container Registry");
        output.IsSecure.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputLoginServerIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs[1];
        output.Name.Should().Be("loginServer");
        output.Type.Should().Be(BicepType.String);
        output.Description.Should().Be("The login server of the Container Registry");
        output.IsSecure.Should().BeFalse();
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
        type.Description.Should().Be("SKU for the Container Registry");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypePublicNetworkAccessIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "PublicNetworkAccess").Subject;
        type.IsExported.Should().BeTrue();
        type.Description.Should().Be("Public network access setting for the Container Registry");
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

        module.ModuleName.Should().Be("containerRegistry");
        module.ModuleFolderName.Should().Be("ContainerRegistry");
        module.ModuleBicepContent.Should().Contain("resource containerRegistry");
        module.ModuleTypesBicepContent.Should().Contain("type SkuName");
        module.ModuleTypesBicepContent.Should().Contain("type PublicNetworkAccess");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        // Import
        bicep.Should().Contain("import { SkuName, PublicNetworkAccess } from './types.bicep'");

        // Params
        bicep.Should().Contain("@description('Azure region for the Container Registry')");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'Basic'");
        bicep.Should().Contain("param adminUserEnabled bool = false");
        bicep.Should().Contain("param publicNetworkAccess PublicNetworkAccess = 'Enabled'");
        bicep.Should().Contain("param zoneRedundancy bool = false");

        // Resource
        bicep.Should().Contain("resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {");
        bicep.Should().Contain("  name: name");
        bicep.Should().Contain("  location: location");
        bicep.Should().Contain("  sku: {");
        bicep.Should().Contain("    name: sku");
        bicep.Should().Contain("  properties: {");
        bicep.Should().Contain("    adminUserEnabled: adminUserEnabled");
        bicep.Should().Contain("    publicNetworkAccess: publicNetworkAccess");
        bicep.Should().Contain("zoneRedundancy ?");

        // Outputs
        bicep.Should().Contain("output id string = containerRegistry.id");
        bicep.Should().Contain("output loginServer string = containerRegistry.properties.loginServer");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('SKU for the Container Registry')");
        types.Should().Contain("type SkuName =");
        types.Should().Contain("@description('Public network access setting for the Container Registry')");
        types.Should().Contain("type PublicNetworkAccess =");
    }
}
