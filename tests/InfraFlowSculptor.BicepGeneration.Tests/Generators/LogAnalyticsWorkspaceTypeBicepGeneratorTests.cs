using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class LogAnalyticsWorkspaceTypeBicepGeneratorTests
{
    private readonly LogAnalyticsWorkspaceTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-law",
        Type = AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "log",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.LogAnalyticsWorkspace);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("logAnalyticsWorkspace");
        spec.ModuleFolderName.Should().Be("LogAnalyticsWorkspace");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.LogAnalyticsWorkspace);
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
                p.Description == "Azure region for the Log Analytics workspace" &&
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
                p.Description == "Name of the Log Analytics workspace" &&
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
        skuParam.Description.Should().Be("SKU of the Log Analytics workspace");
        skuParam.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("PerGB2018");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasRetentionInDaysParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "retentionInDays").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.Description.Should().Be("Number of days to retain data");
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(30);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDailyQuotaGbParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "dailyQuotaGb").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.Description.Should().Be("Daily ingestion quota in GB (-1 for unlimited)");
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(-1);
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("logAnalyticsWorkspace");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.OperationalInsights/workspaces@2023-09-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameLocationAndProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(3);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsSkuAndRetentionAndCapping()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(3);
        properties.Properties[0].Key.Should().Be("sku");
        properties.Properties[1].Key.Should().Be("retentionInDays");
        properties.Properties[2].Key.Should().Be("workspaceCapping");
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTwoOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputsAreCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Outputs[0].Name.Should().Be("logAnalyticsWorkspaceId");
        spec.Outputs[0].Type.Should().Be(BicepType.String);
        spec.Outputs[0].IsSecure.Should().BeFalse();

        spec.Outputs[1].Name.Should().Be("customerId");
        spec.Outputs[1].Type.Should().Be(BicepType.String);
        spec.Outputs[1].Description.Should().Be("The customer ID (workspace ID) of the Log Analytics workspace");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeIsSkuName()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes[0];
        type.Name.Should().Be("SkuName");
        type.IsExported.Should().BeTrue();
        type.Description.Should().Be("SKU name for the Log Analytics workspace");
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

        module.ModuleName.Should().Be("logAnalyticsWorkspace");
        module.ModuleFolderName.Should().Be("LogAnalyticsWorkspace");
        module.ModuleBicepContent.Should().Contain("resource logAnalyticsWorkspace");
        module.ModuleTypesBicepContent.Should().Contain("type SkuName");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        // Import
        bicep.Should().Contain("import { SkuName } from './types.bicep'");

        // Params
        bicep.Should().Contain("@description('Azure region for the Log Analytics workspace')");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param sku SkuName = 'PerGB2018'");
        bicep.Should().Contain("param retentionInDays int = 30");
        bicep.Should().Contain("param dailyQuotaGb int = -1");

        // Resource
        bicep.Should().Contain("resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {");
        bicep.Should().Contain("  name: name");
        bicep.Should().Contain("  location: location");
        bicep.Should().Contain("  properties: {");
        bicep.Should().Contain("    sku: {");
        bicep.Should().Contain("      name: sku");
        bicep.Should().Contain("    retentionInDays: retentionInDays");
        bicep.Should().Contain("    workspaceCapping: {");
        bicep.Should().Contain("      dailyQuotaGb: dailyQuotaGb");

        // Outputs
        bicep.Should().Contain("output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id");
        bicep.Should().Contain("output customerId string = logAnalyticsWorkspace.properties.customerId");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsSkuNameType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('SKU name for the Log Analytics workspace')");
        types.Should().Contain("type SkuName =");
    }
}
