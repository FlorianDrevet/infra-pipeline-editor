using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class AppServicePlanTypeBicepGeneratorTests
{
    private readonly AppServicePlanTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-asp",
        Type = AzureResourceTypes.ArmTypes.AppServicePlan,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "asp",
        Properties = new Dictionary<string, string>
        {
            ["sku"] = "F1",
            ["capacity"] = "1",
            ["osType"] = "Linux",
        },
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.AppServicePlan);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.AppServicePlan);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("appServicePlan");
        spec.ModuleFolderName.Should().Be("AppServicePlan");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.AppServicePlan);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSkuNameAndOsTypeFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SkuName") &&
                i.Symbols.Contains("OsType"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasFiveParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(5);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLocationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "location")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.Description == "Azure region for the App Service Plan" &&
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
                p.Description == "Name of the App Service Plan" &&
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
        skuParam.Description.Should().Be("SKU name of the App Service Plan");
        skuParam.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("F1");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCapacityParamWithNoDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "capacity").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.Description.Should().Be("Number of instances allocated to the plan");
        param.DefaultValue.Should().BeNull();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOsTypeParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "osType").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("OsType");
        param.Description.Should().Be("Operating system type");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Linux");
    }

    // ── Variables ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTwoVariables()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Variables.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_IsLinuxVariableUsesEquality()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var isLinux = spec.Variables.Should().Contain(v => v.Name == "isLinux").Subject;
        isLinux.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("osType == 'Linux'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_KindVariableUsesConditional()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var kind = spec.Variables.Should().Contain(v => v.Name == "kind").Subject;
        kind.Expression.Should().BeOfType<BicepConditionalExpression>();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("asp");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Web/serverfarms@2023-12-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameLocationKindSkuProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(5);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("kind");
        spec.Resource.Body[3].Key.Should().Be("sku");
        spec.Resource.Body[4].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuObjectHasNameAndCapacity()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().HaveCount(2);
        sku.Properties[0].Key.Should().Be("name");
        sku.Properties[1].Key.Should().Be("capacity");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsReserved()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[4].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("reserved");
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

        var output = spec.Outputs[0];
        output.Name.Should().Be("id");
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("asp.id");
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
        type.Description.Should().Be("SKU name for the App Service Plan");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeOsTypeIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "OsType").Subject;
        type.IsExported.Should().BeTrue();
        type.Description.Should().Be("Operating system type for the App Service Plan");
    }

    // ── No companions, no secure params ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoCompanions()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Companions.Should().BeEmpty();
    }

    // ── Legacy backward compatibility ──

    [Fact]
    public void Given_Resource_When_Generate_Then_ReturnsLegacyModule()
    {
        var module = _sut.Generate(CreateResource());

        module.ModuleName.Should().Be("appServicePlan");
        module.ModuleFolderName.Should().Be("AppServicePlan");
        module.ModuleBicepContent.Should().Contain("resource asp");
        module.ModuleTypesBicepContent.Should().Contain("type SkuName");
        module.ModuleTypesBicepContent.Should().Contain("type OsType");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        // Import
        bicep.Should().Contain("import { SkuName, OsType } from './types.bicep'");

        // Params
        bicep.Should().Contain("@description('Azure region for the App Service Plan')");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'F1'");
        bicep.Should().Contain("param capacity int");
        bicep.Should().Contain("param osType OsType = 'Linux'");

        // Variables
        bicep.Should().Contain("var isLinux = osType == 'Linux'");
        bicep.Should().Contain("var kind = isLinux ? 'linux' : 'app'");

        // Resource
        bicep.Should().Contain("resource asp 'Microsoft.Web/serverfarms@2023-12-01' = {");
        bicep.Should().Contain("  name: name");
        bicep.Should().Contain("  location: location");
        bicep.Should().Contain("  kind: kind");
        bicep.Should().Contain("  sku: {");
        bicep.Should().Contain("    name: sku");
        bicep.Should().Contain("    capacity: capacity");
        bicep.Should().Contain("  properties: {");
        bicep.Should().Contain("    reserved: isLinux");

        // Output
        bicep.Should().Contain("output id string = asp.id");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('SKU name for the App Service Plan')");
        types.Should().Contain("type SkuName =");
        types.Should().Contain("@description('Operating system type for the App Service Plan')");
        types.Should().Contain("type OsType =");
    }
}
