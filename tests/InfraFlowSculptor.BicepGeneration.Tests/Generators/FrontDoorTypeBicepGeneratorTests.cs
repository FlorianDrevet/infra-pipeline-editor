using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class FrontDoorTypeBicepGeneratorTests
{
    private readonly FrontDoorTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-frontdoor",
        Type = AzureResourceTypes.ArmTypes.FrontDoorType,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "afd",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.FrontDoorType);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.FrontDoor);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("frontDoor");
        spec.ModuleFolderName.Should().Be("FrontDoor");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.FrontDoor);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSkuNameParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "skuName").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Standard_AzureFrontDoor");
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

        spec.Resource.Symbol.Should().Be("frontDoor");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Cdn/profiles@2024-02-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasExpectedProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(5);
        spec.Resource.Body.Select(p => p.Key).Should().ContainInConsecutiveOrder(
            "name", "location", "tags", "skuName", "properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuObjectHasNameProperty()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body.First(p => p.Key == "skuName")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsOriginResponseTimeout()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.First(p => p.Key == "properties")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("originResponseTimeoutSeconds");
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
            .Which.RawBicep.Should().Be("frontDoor.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "nameOutput").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("frontDoor.name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputFrontDoorIdIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "frontDoorId").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("frontDoor.properties.frontDoorId");
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
            .Which.RawBicep.Should().Be("'Standard_AzureFrontDoor' | 'Premium_AzureFrontDoor'");
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

        legacy.ModuleName.Should().Be("frontDoor");
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

        bicep.Should().Contain("import { SkuName } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param skuName SkuName = 'Standard_AzureFrontDoor'");
        bicep.Should().Contain("param tags object = {}");
        bicep.Should().Contain("resource frontDoor 'Microsoft.Cdn/profiles@2024-02-01'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("tags: tags");
        bicep.Should().Contain("name: skuName");
        bicep.Should().Contain("originResponseTimeoutSeconds: 60");
        bicep.Should().Contain("output id string = frontDoor.id");
        bicep.Should().Contain("output nameOutput string = frontDoor.name");
        bicep.Should().Contain("output frontDoorId string = frontDoor.properties.frontDoorId");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsSkuNameType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SkuName = 'Standard_AzureFrontDoor' | 'Premium_AzureFrontDoor'");
    }
}
