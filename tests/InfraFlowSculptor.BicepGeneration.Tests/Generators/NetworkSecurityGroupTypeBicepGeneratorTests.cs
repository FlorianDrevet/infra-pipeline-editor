using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class NetworkSecurityGroupTypeBicepGeneratorTests
{
    private readonly NetworkSecurityGroupTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-nsg",
        Type = AzureResourceTypes.ArmTypes.NetworkSecurityGroupType,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "nsg",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.NetworkSecurityGroupType);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.NetworkSecurityGroup);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("networkSecurityGroup");
        spec.ModuleFolderName.Should().Be("NetworkSecurityGroup");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.NetworkSecurityGroup);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSecurityRuleConfigFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SecurityRuleConfig"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasFourParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(4);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSecurityRulesParamWithCustomTypeAndEmptyDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "securityRules").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SecurityRuleConfig[]");
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

        spec.Resource.Symbol.Should().Be("nsg");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/networkSecurityGroups@2023-11-01");
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
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsSecurityRules()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.First(p => p.Key == "properties")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("securityRules");
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
            .Which.RawBicep.Should().Be("nsg.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "nameOutput").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("nsg.name");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeSecurityRuleConfigIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SecurityRuleConfig").Subject;
        var raw = type.Body.Should().BeOfType<BicepRawExpression>().Subject;
        raw.RawBicep.Should().Contain("name: string");
        raw.RawBicep.Should().Contain("priority: int");
        raw.RawBicep.Should().Contain("direction: string");
        raw.RawBicep.Should().Contain("access: string");
        raw.RawBicep.Should().Contain("protocol: string");
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

        legacy.ModuleName.Should().Be("networkSecurityGroup");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SecurityRuleConfig } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param securityRules SecurityRuleConfig[] = []");
        bicep.Should().Contain("param tags object = {}");
        bicep.Should().Contain("resource nsg 'Microsoft.Network/networkSecurityGroups@2023-11-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("tags: tags");
        bicep.Should().Contain("securityRules: securityRules");
        bicep.Should().Contain("output id string = nsg.id");
        bicep.Should().Contain("output nameOutput string = nsg.name");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsSecurityRuleConfigType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SecurityRuleConfig =");
        types.Should().Contain("name: string");
        types.Should().Contain("priority: int");
    }
}
