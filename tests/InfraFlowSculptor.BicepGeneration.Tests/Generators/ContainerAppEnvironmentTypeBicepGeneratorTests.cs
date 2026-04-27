using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class ContainerAppEnvironmentTypeBicepGeneratorTests
{
    private readonly ContainerAppEnvironmentTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-cae",
        Type = AzureResourceTypes.ArmTypes.ContainerAppEnvironment,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "cae",
        Properties = properties ?? new Dictionary<string, string>(),
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.ContainerAppEnvironment);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerAppEnvironment);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("containerAppEnvironment");
        spec.ModuleFolderName.Should().Be("ContainerAppEnvironment");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerAppEnvironment);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsWorkloadProfileTypeFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("WorkloadProfileType"));
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
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "name")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasWorkloadProfileTypeParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "workloadProfileType").Which;

        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("WorkloadProfileType");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Consumption");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasInternalLoadBalancerEnabledParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "internalLoadBalancerEnabled").Which;

        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasZoneRedundancyEnabledParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "zoneRedundancyEnabled").Which;

        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasLogAnalyticsWorkspaceIdParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "logAnalyticsWorkspaceId").Which;

        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().BeEmpty();
    }

    // ── Primary resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PrimaryResourceHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("containerAppEnv");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.App/managedEnvironments@2024-03-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PrimaryResourceHasNoCondition()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Resource.Condition.Should().BeNull();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasNameAndLocation()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var body = spec.Resource.Body;

        body.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("name");

        body.Should().Contain(p => p.Key == "location")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("location");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasZoneRedundant()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        properties.Properties.Should().Contain(p => p.Key == "zoneRedundant")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("zoneRedundancyEnabled");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasVnetConfiguration()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var vnet = properties.Properties.Should().Contain(p => p.Key == "vnetConfiguration")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var internalProp = vnet.Properties.Should().ContainSingle().Which;
        internalProp.Key.Should().Be("internal");
        internalProp.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("internalLoadBalancerEnabled");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasAppLogsConfigConditional()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var appLogs = properties.Properties.Should().Contain(p => p.Key == "appLogsConfiguration")
            .Which.Value.Should().BeOfType<BicepConditionalExpression>().Subject;

        appLogs.Condition.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("logAnalyticsWorkspaceId != ''");

        var consequentObj = appLogs.Consequent.Should().BeOfType<BicepObjectExpression>().Subject;
        var destProp = consequentObj.Properties.Should().ContainSingle().Which;
        destProp.Key.Should().Be("destination");
        destProp.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("azure-monitor");

        appLogs.Alternate.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("null");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasWorkloadProfilesArray()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var workloadProfiles = properties.Properties.Should().Contain(p => p.Key == "workloadProfiles")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        workloadProfiles.Items.Should().ContainSingle()
            .Which.Should().BeOfType<BicepObjectExpression>()
            .Which.Properties.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_WorkloadProfileObjectHasNameAndType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var arr = properties.Properties.Should().Contain(p => p.Key == "workloadProfiles")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        var obj = arr.Items[0].Should().BeOfType<BicepObjectExpression>().Subject;

        obj.Properties.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("workloadProfileType");

        obj.Properties.Should().Contain(p => p.Key == "workloadProfileType")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("workloadProfileType");
    }

    // ── Additional resource: diagnosticSettings ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneAdditionalResource()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.AdditionalResources.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticSettingsHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        diag.Symbol.Should().Be("diagnosticSettings");
        diag.ArmTypeWithApiVersion.Should().Be("Microsoft.Insights/diagnosticSettings@2021-05-01-preview");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticSettingsHasCondition()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        diag.Condition.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("logAnalyticsWorkspaceId != ''");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticSettingsHasScope()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        diag.Scope.Should().Be("containerAppEnv");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticSettingsHasNameProperty()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        diag.Body.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("containerAppEnvLogs");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticSettingsPropertiesHasWorkspaceIdAndLogs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        var props = diag.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        props.Properties.Should().Contain(p => p.Key == "workspaceId")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("logAnalyticsWorkspaceId");

        var logs = props.Properties.Should().Contain(p => p.Key == "logs")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        logs.Items.Should().ContainSingle()
            .Which.Should().BeOfType<BicepObjectExpression>()
            .Which.Properties.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DiagnosticLogsObjectHasCorrectProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var diag = spec.AdditionalResources[0];

        var props = diag.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var logs = props.Properties.Should().Contain(p => p.Key == "logs")
            .Which.Value.Should().BeOfType<BicepArrayExpression>().Subject;

        var logObj = logs.Items[0].Should().BeOfType<BicepObjectExpression>().Subject;

        logObj.Properties.Should().Contain(p => p.Key == "categoryGroup")
            .Which.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("allLogs");

        logObj.Properties.Should().Contain(p => p.Key == "enabled")
            .Which.Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeTrue();
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasIdOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("containerAppEnv.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDefaultDomainOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "defaultDomain").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("containerAppEnv.properties.defaultDomain");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasStaticIpOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "staticIp").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("containerAppEnv.properties.staticIp");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().ContainSingle();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasWorkloadProfileTypeExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "WorkloadProfileType").Which;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'");
    }

    // ── Legacy compatibility ──

    [Fact]
    public void Given_Generator_Then_AlsoImplementsIResourceTypeBicepGenerator()
    {
        _sut.Should().BeAssignableTo<IResourceTypeBicepGenerator>();
    }

    [Fact]
    public void Given_Resource_When_LegacyGenerate_Then_ModuleIdentityIsCorrect()
    {
        var module = _sut.Generate(CreateResource());

        module.ModuleName.Should().Be("containerAppEnvironment");
        module.ModuleFolderName.Should().Be("ContainerAppEnvironment");
        module.ResourceTypeName.Should().Be(AzureResourceTypes.ContainerAppEnvironment);
    }

    [Fact]
    public void Given_Resource_When_LegacyGenerate_Then_ParametersDictIsEmpty()
    {
        var module = _sut.Generate(CreateResource());
        module.Parameters.Should().BeEmpty();
    }

    // ── Emission ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsImport()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { WorkloadProfileType } from './types.bicep'");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param workloadProfileType WorkloadProfileType = 'Consumption'");
        bicep.Should().Contain("param internalLoadBalancerEnabled bool = false");
        bicep.Should().Contain("param zoneRedundancyEnabled bool = false");
        bicep.Should().Contain("param logAnalyticsWorkspaceId string = ''");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsPrimaryResourceDeclaration()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsConditionalAppLogsConfiguration()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("appLogsConfiguration: logAnalyticsWorkspaceId != ''");
        bicep.Should().Contain("destination");
        bicep.Should().Contain("azure-monitor");
        bicep.Should().Contain("null");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsDiagnosticSettingsWithCondition()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (logAnalyticsWorkspaceId != '') {");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsDiagnosticSettingsScope()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("scope: containerAppEnv");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsDiagnosticSettingsBody()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("name: 'containerAppEnvLogs'");
        bicep.Should().Contain("workspaceId: logAnalyticsWorkspaceId");
        bicep.Should().Contain("categoryGroup: 'allLogs'");
        bicep.Should().Contain("enabled: true");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("output id string = containerAppEnv.id");
        bicep.Should().Contain("output defaultDomain string = containerAppEnv.properties.defaultDomain");
        bicep.Should().Contain("output staticIp string = containerAppEnv.properties.staticIp");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsWorkloadProfileType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('Workload profile type for the Container App Environment')");
        types.Should().Contain("type WorkloadProfileType = 'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'");
    }
}
