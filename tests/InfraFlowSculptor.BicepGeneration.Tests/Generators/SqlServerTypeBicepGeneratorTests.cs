using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class SqlServerTypeBicepGeneratorTests
{
    private readonly SqlServerTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-sql",
        Type = AzureResourceTypes.ArmTypes.SqlServer,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "sql",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.SqlServer);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.SqlServer);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("sqlServer");
        spec.ModuleFolderName.Should().Be("SqlServer");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.SqlServer);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsSqlServerVersionAndTlsVersionFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SqlServerVersion") &&
                i.Symbols.Contains("TlsVersion"));
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
    public void Given_Resource_When_GenerateSpec_Then_HasVersionParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "version").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SqlServerVersion");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("12.0");
        param.IsSecure.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAdministratorLoginParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "administratorLogin").Subject;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeNull();
        param.IsSecure.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAdministratorLoginPasswordParamSecure()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "administratorLoginPassword").Subject;
        param.Type.Should().Be(BicepType.String);
        param.DefaultValue.Should().BeNull();
        param.IsSecure.Should().BeTrue();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMinimalTlsVersionParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "minimalTlsVersion").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("TlsVersion");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("1.2");
        param.IsSecure.Should().BeFalse();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("sqlServer");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Sql/servers@2023-08-01-preview");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasThreeTopLevelProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(3);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasFiveProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(5);
        properties.Properties[0].Key.Should().Be("version");
        properties.Properties[1].Key.Should().Be("administratorLogin");
        properties.Properties[2].Key.Should().Be("administratorLoginPassword");
        properties.Properties[3].Key.Should().Be("minimalTlsVersion");
        properties.Properties[4].Key.Should().Be("publicNetworkAccess");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PublicNetworkAccessIsStringLiteral()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[4].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Enabled");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesUseParamReferences()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[2].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("version");
        properties.Properties[1].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("administratorLogin");
        properties.Properties[2].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("administratorLoginPassword");
        properties.Properties[3].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("minimalTlsVersion");
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
            .Which.RawBicep.Should().Be("sqlServer.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputFqdnIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "fullyQualifiedDomainName").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("sqlServer.properties.fullyQualifiedDomainName");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTwoExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeSqlServerVersionIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SqlServerVersion").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'12.0'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeTlsVersionIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "TlsVersion").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'1.0' | '1.1' | '1.2'");
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

        legacy.ModuleName.Should().Be("sqlServer");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
        legacy.ModuleTypesBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_ParametersDictContainsExpectedKeys()
    {
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.Parameters.Should().ContainKey("version")
            .WhoseValue.Should().Be("12.0");
        legacy.Parameters.Should().ContainKey("administratorLogin")
            .WhoseValue.Should().Be("sqladmin");
        legacy.Parameters.Should().ContainKey("minimalTlsVersion")
            .WhoseValue.Should().Be("1.2");
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_SecureParametersContainsPassword()
    {
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.SecureParameters.Should().Contain("administratorLoginPassword");
    }

    [Fact]
    public void Given_ResourceWithV12Version_When_Generate_Then_NormalizesTo12Dot0()
    {
        var resource = CreateResource(new Dictionary<string, string> { ["version"] = "V12" });
        var legacy = _sut.Generate(resource);

        legacy.Parameters["version"].Should().Be("12.0");
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SqlServerVersion, TlsVersion } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param version SqlServerVersion = '12.0'");
        bicep.Should().Contain("param administratorLogin string");
        bicep.Should().Contain("@secure()");
        bicep.Should().Contain("param administratorLoginPassword string");
        bicep.Should().Contain("param minimalTlsVersion TlsVersion = '1.2'");
        bicep.Should().Contain("resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("version: version");
        bicep.Should().Contain("administratorLogin: administratorLogin");
        bicep.Should().Contain("administratorLoginPassword: administratorLoginPassword");
        bicep.Should().Contain("minimalTlsVersion: minimalTlsVersion");
        bicep.Should().Contain("publicNetworkAccess: 'Enabled'");
        bicep.Should().Contain("output id string = sqlServer.id");
        bicep.Should().Contain("output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsBothExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type SqlServerVersion = '12.0'");
        types.Should().Contain("type TlsVersion = '1.0' | '1.1' | '1.2'");
    }
}
