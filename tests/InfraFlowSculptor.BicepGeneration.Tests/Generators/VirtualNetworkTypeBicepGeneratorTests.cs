using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class VirtualNetworkTypeBicepGeneratorTests
{
    private readonly VirtualNetworkTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-vnet",
        Type = AzureResourceTypes.ArmTypes.VirtualNetworkType,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "vnet",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.VirtualNetworkType);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.VirtualNetwork);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("virtualNetwork");
        spec.ModuleFolderName.Should().Be("VirtualNetwork");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.VirtualNetwork);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSubnetConfigFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SubnetConfig"));
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
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAddressPrefixesParamWithDefaultArray()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "addressPrefixes").Subject;
        param.Type.Should().Be(BicepType.Array);
        param.DefaultValue.Should().BeOfType<BicepArrayExpression>();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnableDdosProtectionParamWithFalseDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "enableDdosProtection").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSubnetsParamWithCustomTypeAndEmptyDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "subnets").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SubnetConfig[]");
        param.DefaultValue.Should().BeOfType<BicepArrayExpression>();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTagsParamWithEmptyObjectDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "tags").Subject;
        param.Type.Should().Be(BicepType.Object);
        param.DefaultValue.Should().BeOfType<BicepObjectExpression>();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("virtualNetwork");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/virtualNetworks@2023-11-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasExpectedProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(4);
        spec.Resource.Body.Select(p => p.Key).Should().ContainInConsecutiveOrder(
            "name", "location", "tags", "properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsAddressSpaceAndSubnets()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.First(p => p.Key == "properties")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(3);
        properties.Properties.Select(p => p.Key).Should()
            .Contain("addressSpace")
            .And.Contain("enableDdosProtection")
            .And.Contain("subnets");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_AddressSpaceContainsAddressPrefixes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.First(p => p.Key == "properties")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var addressSpace = properties.Properties.First(p => p.Key == "addressSpace")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        addressSpace.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("addressPrefixes");
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
            .Which.RawBicep.Should().Be("virtualNetwork.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "nameOutput").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("virtualNetwork.name");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeSubnetConfigIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SubnetConfig").Subject;
        var raw = type.Body.Should().BeOfType<BicepRawExpression>().Subject;
        raw.RawBicep.Should().Contain("name: string");
        raw.RawBicep.Should().Contain("addressPrefix: string");
        raw.RawBicep.Should().Contain("delegation: string?");
        raw.RawBicep.Should().Contain("serviceEndpoints: string[]?");
        raw.RawBicep.Should().Contain("nsgId: string?");
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

        legacy.ModuleName.Should().Be("virtualNetwork");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SubnetConfig } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param addressPrefixes array =");
        bicep.Should().Contain("'10.0.0.0/16'");
        bicep.Should().Contain("param enableDdosProtection bool = false");
        bicep.Should().Contain("param subnets SubnetConfig[] = []");
        bicep.Should().Contain("param tags object = {}");
        bicep.Should().Contain("resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-11-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("tags: tags");
        bicep.Should().Contain("addressPrefixes: addressPrefixes");
        bicep.Should().Contain("enableDdosProtection: enableDdosProtection");
        bicep.Should().Contain("subnets: subnets");
        bicep.Should().Contain("output id string = virtualNetwork.id");
        bicep.Should().Contain("output nameOutput string = virtualNetwork.name");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsSubnetConfigType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SubnetConfig =");
        types.Should().Contain("name: string");
        types.Should().Contain("addressPrefix: string");
        types.Should().Contain("nsgId: string?");
    }
}
