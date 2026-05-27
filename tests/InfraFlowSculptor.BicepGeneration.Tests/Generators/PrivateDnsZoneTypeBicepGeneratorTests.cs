using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class PrivateDnsZoneTypeBicepGeneratorTests
{
    private readonly PrivateDnsZoneTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "privatelink.vaultcore.azure.net",
        Type = AzureResourceTypes.ArmTypes.PrivateDnsZoneType,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "pdnsz",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.PrivateDnsZoneType);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.PrivateDnsZone);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("privateDnsZone");
        spec.ModuleFolderName.Should().Be("PrivateDnsZone");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.PrivateDnsZone);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsVirtualNetworkLinkConfigFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("VirtualNetworkLinkConfig"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasVirtualNetworkLinksParamWithCustomTypeAndEmptyDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "virtualNetworkLinks").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("VirtualNetworkLinkConfig[]");
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

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().NotContain(p => p.Name == "location",
            "Private DNS Zones always use 'global' location, hardcoded in the resource body");
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("privateDnsZone");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/privateDnsZones@2024-06-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasExpectedProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(3);
        spec.Resource.Body.Select(p => p.Key).Should().ContainInConsecutiveOrder(
            "name", "location", "tags");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_LocationIsGlobal()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.First(p => p.Key == "location")
            .Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("global");
    }

    // ── Additional resources (vnet links) ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneAdditionalResource()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.AdditionalResources.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_AdditionalResourceIsVnetLink()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var link = spec.AdditionalResources.Should().ContainSingle().Subject;
        link.Symbol.Should().Be("vnetLink");
        link.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01");
        link.ParentSymbol.Should().Be("privateDnsZone");
        link.ForLoop.Should().NotBeNull();
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
            .Which.RawBicep.Should().Be("privateDnsZone.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "nameOutput").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("privateDnsZone.name");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeVirtualNetworkLinkConfigIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "VirtualNetworkLinkConfig").Subject;
        var raw = type.Body.Should().BeOfType<BicepRawExpression>().Subject;
        raw.RawBicep.Should().Contain("name: string");
        raw.RawBicep.Should().Contain("vnetId: string");
        raw.RawBicep.Should().Contain("enableAutoRegistration: bool");
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

        legacy.ModuleName.Should().Be("privateDnsZone");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { VirtualNetworkLinkConfig } from './types.bicep'");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param virtualNetworkLinks VirtualNetworkLinkConfig[] = []");
        bicep.Should().Contain("param tags object = {}");
        bicep.Should().Contain("resource privateDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01'");
        bicep.Should().Contain("location: 'global'");
        bicep.Should().Contain("output id string = privateDnsZone.id");
        bicep.Should().Contain("output nameOutput string = privateDnsZone.name");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsVnetLinkForLoop()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("for link in virtualNetworkLinks");
        bicep.Should().Contain("parent: privateDnsZone");
        bicep.Should().Contain("link.name");
        bicep.Should().Contain("link.vnetId");
        bicep.Should().Contain("link.enableAutoRegistration");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsVirtualNetworkLinkConfigType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type VirtualNetworkLinkConfig =");
        types.Should().Contain("name: string");
        types.Should().Contain("vnetId: string");
        types.Should().Contain("enableAutoRegistration: bool");
    }
}
