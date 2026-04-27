using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class UserAssignedIdentityTypeBicepGeneratorTests
{
    private readonly UserAssignedIdentityTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-identity",
        Type = AzureResourceTypes.ArmTypes.UserAssignedIdentity,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "id",
    };

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("userAssignedIdentity");
        spec.ModuleFolderName.Should().Be("UserAssignedIdentity");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.UserAssignedIdentity);
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.Description == "Azure region for the User Assigned Identity" &&
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
                p.Description == "Name of the User Assigned Identity" &&
                !p.IsSecure &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasExactlyTwoParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(2);
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolIsIdentity()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("identity");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameAndLocation()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(2);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputsAreCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Outputs[0].Name.Should().Be("resourceId");
        spec.Outputs[0].Type.Should().Be(BicepType.String);

        spec.Outputs[1].Name.Should().Be("principalId");
        spec.Outputs[1].Type.Should().Be(BicepType.String);

        spec.Outputs[2].Name.Should().Be("clientId");
        spec.Outputs[2].Type.Should().Be(BicepType.String);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_NoOutputsAreSecure()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().AllSatisfy(o => o.IsSecure.Should().BeFalse());
    }

    // ── No types, imports, companions, variables ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().BeEmpty();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoImports()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Imports.Should().BeEmpty();
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

    // ── Legacy Generate() still works ──

    [Fact]
    public void Given_Resource_When_Generate_Then_ReturnsLegacyModule()
    {
        var module = _sut.Generate(CreateResource());

        module.ModuleName.Should().Be("userAssignedIdentity");
        module.ModuleFolderName.Should().Be("UserAssignedIdentity");
        module.ModuleBicepContent.Should().Contain("resource identity");
    }

    // ── Interface contracts ──

    [Fact]
    public void Given_Generator_Then_ImplementsIResourceTypeBicepSpecGenerator()
    {
        _sut.Should().BeAssignableTo<IResourceTypeBicepSpecGenerator>();
    }

    [Fact]
    public void Given_Generator_Then_ResourceTypeIsCorrectArmType()
    {
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.UserAssignedIdentity);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.UserAssignedIdentity);
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitSpec_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        // Params
        bicep.Should().Contain("@description('Azure region for the User Assigned Identity')");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("@description('Name of the User Assigned Identity')");
        bicep.Should().Contain("param name string");

        // Resource
        bicep.Should().Contain("resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {");
        bicep.Should().Contain("  name: name");
        bicep.Should().Contain("  location: location");

        // Outputs
        bicep.Should().Contain("output resourceId string = identity.id");
        bicep.Should().Contain("output principalId string = identity.properties.principalId");
        bicep.Should().Contain("output clientId string = identity.properties.clientId");
    }

    [Fact]
    public void Given_Resource_When_EmitSpec_Then_TypesContentIsEmpty()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().BeEmpty();
    }
}
