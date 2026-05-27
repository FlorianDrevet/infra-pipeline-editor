using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class PrivateEndpointTypeBicepGeneratorTests
{
    private readonly PrivateEndpointTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-pe",
        Type = AzureResourceTypes.ArmTypes.PrivateEndpointType,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "pe",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.PrivateEndpointType);
        _sut.ResourceTypeName.Should().Be("PrivateEndpoint");
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("privateEndpoint");
        spec.ModuleFolderName.Should().Be("Common");
        spec.ResourceTypeName.Should().Be("PrivateEndpoint");
    }

    // ── No imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoImports()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Imports.Should().BeEmpty();
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEightParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(8);
    }

    [Theory]
    [InlineData("location")]
    [InlineData("name")]
    [InlineData("privateLinkServiceId")]
    [InlineData("subnetId")]
    public void Given_Resource_When_GenerateSpec_Then_HasRequiredStringParam(string paramName)
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == paramName).Subject;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeNull();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasGroupIdsArrayParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "groupIds").Subject;
        param.Type.Should().Be(BicepType.Array);
        param.DefaultValue.Should().BeNull();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasPrivateDnsZoneIdParamWithEmptyStringDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "privateDnsZoneId").Subject;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().BeEmpty();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCustomNetworkInterfaceNameParamWithEmptyStringDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "customNetworkInterfaceName").Subject;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().BeEmpty();
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

        spec.Resource.Symbol.Should().Be("privateEndpoint");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/privateEndpoints@2023-11-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasExpectedTopLevelProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Select(p => p.Key).Should().Contain("name")
            .And.Contain("location")
            .And.Contain("tags")
            .And.Contain("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsPrivateLinkServiceConnections()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.First(p => p.Key == "properties")
            .Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Select(p => p.Key).Should()
            .Contain("privateLinkServiceConnections")
            .And.Contain("subnet")
            .And.Contain("customNetworkInterfaceName");
    }

    // ── Additional resources (DNS zone group) ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneAdditionalResource()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.AdditionalResources.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_AdditionalResourceIsDnsZoneGroup()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var dnsGroup = spec.AdditionalResources.Should().ContainSingle().Subject;
        dnsGroup.Symbol.Should().Be("dnsZoneGroup");
        dnsGroup.ArmTypeWithApiVersion.Should().Be("Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-11-01");
        dnsGroup.ParentSymbol.Should().Be("privateEndpoint");
        dnsGroup.Condition.Should().NotBeNull();
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputIdIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("privateEndpoint.id");
    }

    // ── No exported types / no companions / no variables ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().BeEmpty();
    }

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

        legacy.ModuleName.Should().Be("privateEndpoint");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param privateLinkServiceId string");
        bicep.Should().Contain("param groupIds array");
        bicep.Should().Contain("param subnetId string");
        bicep.Should().Contain("param privateDnsZoneId string = ''");
        bicep.Should().Contain("param customNetworkInterfaceName string = ''");
        bicep.Should().Contain("param tags object = {}");
        bicep.Should().Contain("resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-11-01'");
        bicep.Should().Contain("privateLinkServiceConnections:");
        bicep.Should().Contain("privateLinkServiceId: privateLinkServiceId");
        bicep.Should().Contain("groupIds: groupIds");
        bicep.Should().Contain("subnet:");
        bicep.Should().Contain("id: subnetId");
        bicep.Should().Contain("output id string = privateEndpoint.id");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsConditionalDnsZoneGroup()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("dnsZoneGroup");
        bicep.Should().Contain("privateDnsZoneGroups@2023-11-01");
        bicep.Should().Contain("parent: privateEndpoint");
        bicep.Should().Contain("privateDnsZoneConfigs:");
        bicep.Should().Contain("privateDnsZoneId: privateDnsZoneId");
    }
}
