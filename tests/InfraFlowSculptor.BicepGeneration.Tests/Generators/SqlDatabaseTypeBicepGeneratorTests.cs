using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class SqlDatabaseTypeBicepGeneratorTests
{
    private readonly SqlDatabaseTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-sqldb",
        Type = AzureResourceTypes.ArmTypes.SqlDatabase,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "sqldb",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.SqlDatabase);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.SqlDatabase);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("sqlDatabase");
        spec.ModuleFolderName.Should().Be("SqlDatabase");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.SqlDatabase);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSevenParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(7);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSqlServerNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "sqlServerName")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "sku").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Basic");
        param.IsSecure.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMaxSizeBytesParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "maxSizeBytes")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.Int &&
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCollationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "collation")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasZoneRedundantParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "zoneRedundant")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.Bool &&
                p.DefaultValue == null &&
                !p.IsSecure);
    }

    // ── Existing resources ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasOneExistingResource()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ExistingResources.Should().ContainSingle()
            .Which.Should().Match<BicepExistingResource>(e =>
                e.Symbol == "sqlServer" &&
                e.ArmTypeWithApiVersion == "Microsoft.Sql/servers@2023-08-01-preview" &&
                e.NameExpression == "sqlServerName");
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("sqlDatabase");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Sql/servers/databases@2023-08-01-preview");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasParentSymbol()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.ParentSymbol.Should().Be("sqlServer");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasFourTopLevelProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(4);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("sku");
        spec.Resource.Body[3].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_SkuHasOneNestedProp()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var sku = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        sku.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("name");
        sku.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("sku");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasThreeProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(3);
        properties.Properties[0].Key.Should().Be("collation");
        properties.Properties[1].Key.Should().Be("maxSizeBytes");
        properties.Properties[2].Key.Should().Be("zoneRedundant");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesUseParamReferences()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("collation");
        properties.Properties[1].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("maxSizeBytes");
        properties.Properties[2].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("zoneRedundant");
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
            .Which.RawBicep.Should().Be("sqlDatabase.id");
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
            .Which.RawBicep.Should().Be("'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'");
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

        legacy.ModuleName.Should().Be("sqlDatabase");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
        legacy.ModuleTypesBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_ParametersDictContainsExpectedKeys()
    {
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.Parameters.Should().ContainKey("sku")
            .WhoseValue.Should().Be("Basic");
        legacy.Parameters.Should().ContainKey("maxSizeBytes");
        legacy.Parameters.Should().ContainKey("collation")
            .WhoseValue.Should().Be("SQL_Latin1_General_CP1_CI_AS");
        legacy.Parameters.Should().ContainKey("zoneRedundant")
            .WhoseValue.Should().Be(false);
    }

    [Fact]
    public void Given_ResourceWithCustomSku_When_Generate_Then_ParametersReflectSku()
    {
        var resource = CreateResource(new Dictionary<string, string> { ["sku"] = "Standard" });
        var legacy = _sut.Generate(resource);

        legacy.Parameters["sku"].Should().Be("Standard");
    }

    [Fact]
    public void Given_ResourceWithMaxSizeGb_When_Generate_Then_ConvertsToBytes()
    {
        var resource = CreateResource(new Dictionary<string, string> { ["maxSizeGb"] = "5" });
        var legacy = _sut.Generate(resource);

        legacy.Parameters["maxSizeBytes"].Should().Be((long)5 * 1024 * 1024 * 1024);
    }

    [Fact]
    public void Given_ResourceWithZoneRedundantTrue_When_Generate_Then_ParameterIsTrue()
    {
        var resource = CreateResource(new Dictionary<string, string> { ["zoneRedundant"] = "true" });
        var legacy = _sut.Generate(resource);

        legacy.Parameters["zoneRedundant"].Should().Be(true);
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
        bicep.Should().Contain("param sqlServerName string");
        bicep.Should().Contain("param sku SkuName = 'Basic'");
        bicep.Should().Contain("param maxSizeBytes int");
        bicep.Should().Contain("param collation string");
        bicep.Should().Contain("param zoneRedundant bool");
        bicep.Should().Contain("resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {");
        bicep.Should().Contain("name: sqlServerName");
        bicep.Should().Contain("resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview'");
        bicep.Should().Contain("parent: sqlServer");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("collation: collation");
        bicep.Should().Contain("maxSizeBytes: maxSizeBytes");
        bicep.Should().Contain("zoneRedundant: zoneRedundant");
        bicep.Should().Contain("output id string = sqlDatabase.id");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ExistingResourceAppearsBeforeMainResource()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        var existingIndex = bicep.IndexOf("existing = {");
        var mainResourceIndex = bicep.IndexOf("resource sqlDatabase");
        existingIndex.Should().BeLessThan(mainResourceIndex);
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsSkuNameExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SkuName = 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'");
    }
}
