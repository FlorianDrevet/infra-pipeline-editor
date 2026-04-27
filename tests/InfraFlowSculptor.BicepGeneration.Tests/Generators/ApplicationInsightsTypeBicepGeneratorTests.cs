using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class ApplicationInsightsTypeBicepGeneratorTests
{
    private readonly ApplicationInsightsTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource() => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-appinsights",
        Type = AzureResourceTypes.ArmTypes.ApplicationInsights,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "appi",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.ApplicationInsights);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.ApplicationInsights);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("applicationInsights");
        spec.ModuleFolderName.Should().Be("ApplicationInsights");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.ApplicationInsights);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsIngestionModeFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("IngestionMode"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEightParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(8);
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
    public void Given_Resource_When_GenerateSpec_Then_HasLogAnalyticsWorkspaceIdParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "logAnalyticsWorkspaceId")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSamplingPercentageParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "samplingPercentage").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(100);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasRetentionInDaysParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "retentionInDays").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(90);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDisableIpMaskingParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "disableIpMasking").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDisableLocalAuthParamWithDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "disableLocalAuth").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasIngestionModeParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "ingestionMode").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("IngestionMode");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("LogAnalytics");
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("applicationInsights");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Insights/components@2020-02-02");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameLocationKindProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(4);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("kind");
        spec.Resource.Body[3].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_KindIsWebStringLiteral()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body[2].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("web");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesContainsSevenProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(7);
        properties.Properties[0].Key.Should().Be("Application_Type");
        properties.Properties[1].Key.Should().Be("WorkspaceResourceId");
        properties.Properties[2].Key.Should().Be("SamplingPercentage");
        properties.Properties[3].Key.Should().Be("RetentionInDays");
        properties.Properties[4].Key.Should().Be("DisableIpMasking");
        properties.Properties[5].Key.Should().Be("DisableLocalAuth");
        properties.Properties[6].Key.Should().Be("IngestionMode");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ApplicationTypeIsWebStringLiteral()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[0].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("web");
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
            .Which.RawBicep.Should().Be("applicationInsights.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputInstrumentationKeyIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "instrumentationKey").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("applicationInsights.properties.InstrumentationKey");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputConnectionStringIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "connectionString").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("applicationInsights.properties.ConnectionString");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeIngestionModeIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "IngestionMode").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'ApplicationInsights' | 'ApplicationInsightsWithDiagnosticSettings' | 'LogAnalytics'");
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

        legacy.ModuleName.Should().Be("applicationInsights");
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

        bicep.Should().Contain("import { IngestionMode } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param logAnalyticsWorkspaceId string");
        bicep.Should().Contain("param samplingPercentage int = 100");
        bicep.Should().Contain("param retentionInDays int = 90");
        bicep.Should().Contain("param disableIpMasking bool = false");
        bicep.Should().Contain("param disableLocalAuth bool = false");
        bicep.Should().Contain("param ingestionMode IngestionMode = 'LogAnalytics'");
        bicep.Should().Contain("resource applicationInsights 'Microsoft.Insights/components@2020-02-02'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("kind: 'web'");
        bicep.Should().Contain("Application_Type: 'web'");
        bicep.Should().Contain("WorkspaceResourceId: logAnalyticsWorkspaceId");
        bicep.Should().Contain("SamplingPercentage: samplingPercentage");
        bicep.Should().Contain("RetentionInDays: retentionInDays");
        bicep.Should().Contain("DisableIpMasking: disableIpMasking");
        bicep.Should().Contain("DisableLocalAuth: disableLocalAuth");
        bicep.Should().Contain("IngestionMode: ingestionMode");
        bicep.Should().Contain("output id string = applicationInsights.id");
        bicep.Should().Contain("output instrumentationKey string = applicationInsights.properties.InstrumentationKey");
        bicep.Should().Contain("output connectionString string = applicationInsights.properties.ConnectionString");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type IngestionMode = 'ApplicationInsights' | 'ApplicationInsightsWithDiagnosticSettings' | 'LogAnalytics'");
    }
}
