using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class CosmosDbTypeBicepGeneratorTests
{
    private readonly CosmosDbTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-cosmos",
        Type = AzureResourceTypes.ArmTypes.CosmosDb,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "cosmos",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.CosmosDb);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.CosmosDb);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("cosmosDb");
        spec.ModuleFolderName.Should().Be("CosmosDb");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.CosmosDb);
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsThreeTypesFromTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("DatabaseKind") &&
                i.Symbols.Contains("ConsistencyLevel") &&
                i.Symbols.Contains("BackupPolicyType"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasElevenParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(11);
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
    public void Given_Resource_When_GenerateSpec_Then_HasKindParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "kind").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("DatabaseKind");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("GlobalDocumentDB");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasConsistencyLevelParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "consistencyLevel").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("ConsistencyLevel");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Session");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMaxStalenessPrefixParamWithIntDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "maxStalenessPrefix").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(100);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMaxIntervalInSecondsParamWithIntDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "maxIntervalInSeconds").Subject;
        param.Type.Should().Be(BicepType.Int);
        param.DefaultValue.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(5);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnableAutomaticFailoverParamWithBoolDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "enableAutomaticFailover").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnableMultipleWriteLocationsParamWithBoolDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "enableMultipleWriteLocations").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasBackupPolicyTypeParamWithCustomTypeAndDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "backupPolicyType").Subject;
        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("BackupPolicyType");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Periodic");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnableFreeTierParamWithBoolDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "enableFreeTier").Subject;
        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCapabilitiesParamWithArrayTypeAndEmptyDefault()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var param = spec.Parameters.Should().Contain(p => p.Name == "capabilities").Subject;
        param.Type.Should().Be(BicepType.Array);
        param.DefaultValue.Should().BeOfType<BicepArrayExpression>()
            .Which.Items.Should().BeEmpty();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("cosmosDbAccount");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.DocumentDB/databaseAccounts@2024-05-15");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasNoParent()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Resource.ParentSymbol.Should().BeNull();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasFourTopLevelProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Body.Should().HaveCount(4);
        spec.Resource.Body[0].Key.Should().Be("name");
        spec.Resource.Body[1].Key.Should().Be("location");
        spec.Resource.Body[2].Key.Should().Be("kind");
        spec.Resource.Body[3].Key.Should().Be("properties");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_PropertiesHasEightProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties.Should().HaveCount(8);
        properties.Properties[0].Key.Should().Be("databaseAccountOfferType");
        properties.Properties[1].Key.Should().Be("consistencyPolicy");
        properties.Properties[2].Key.Should().Be("enableAutomaticFailover");
        properties.Properties[3].Key.Should().Be("enableMultipleWriteLocations");
        properties.Properties[4].Key.Should().Be("backupPolicy");
        properties.Properties[5].Key.Should().Be("enableFreeTier");
        properties.Properties[6].Key.Should().Be("capabilities");
        properties.Properties[7].Key.Should().Be("locations");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_DatabaseAccountOfferTypeIsStringLiteral()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[0].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Standard");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ConsistencyPolicyHasThreeNestedProps()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var consistencyPolicy = properties.Properties[1].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        consistencyPolicy.Properties.Should().HaveCount(3);
        consistencyPolicy.Properties[0].Key.Should().Be("defaultConsistencyLevel");
        consistencyPolicy.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("consistencyLevel");
        consistencyPolicy.Properties[1].Key.Should().Be("maxStalenessPrefix");
        consistencyPolicy.Properties[1].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("maxStalenessPrefix");
        consistencyPolicy.Properties[2].Key.Should().Be("maxIntervalInSeconds");
        consistencyPolicy.Properties[2].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("maxIntervalInSeconds");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_BackupPolicyHasOneNestedProp()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var backupPolicy = properties.Properties[4].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        backupPolicy.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("type");
        backupPolicy.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("backupPolicyType");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_LocationsIsArrayWithOneObject()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        var locations = properties.Properties[7].Value.Should().BeOfType<BicepArrayExpression>().Subject;
        locations.Items.Should().ContainSingle();

        var locationObj = locations.Items[0].Should().BeOfType<BicepObjectExpression>().Subject;
        locationObj.Properties.Should().HaveCount(3);
        locationObj.Properties[0].Key.Should().Be("locationName");
        locationObj.Properties[0].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("location");
        locationObj.Properties[1].Key.Should().Be("failoverPriority");
        locationObj.Properties[1].Value.Should().BeOfType<BicepIntLiteral>()
            .Which.Value.Should().Be(0);
        locationObj.Properties[2].Key.Should().Be("isZoneRedundant");
        locationObj.Properties[2].Value.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_BoolParamRefsInProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body[3].Value.Should().BeOfType<BicepObjectExpression>().Subject;
        properties.Properties[2].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("enableAutomaticFailover");
        properties.Properties[3].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("enableMultipleWriteLocations");
        properties.Properties[5].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("enableFreeTier");
        properties.Properties[6].Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("capabilities");
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
            .Which.RawBicep.Should().Be("cosmosDbAccount.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputDocumentEndpointIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "documentEndpoint").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("cosmosDbAccount.properties.documentEndpoint");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_OutputNameIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "name").Subject;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("cosmosDbAccount.name");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeDatabaseKindIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "DatabaseKind").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'GlobalDocumentDB' | 'MongoDB' | 'Parse'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeConsistencyLevelIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "ConsistencyLevel").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Eventual' | 'Session' | 'BoundedStaleness' | 'Strong' | 'ConsistentPrefix'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ExportedTypeBackupPolicyTypeIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "BackupPolicyType").Subject;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Periodic' | 'Continuous'");
    }

    // ── No companions / no variables / no existing resources ──

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

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoExistingResources()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExistingResources.Should().BeEmpty();
    }

    // ── Legacy backward compatibility ──

    [Fact]
    public void Given_Resource_When_Generate_Then_ReturnsLegacyModule()
    {
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.ModuleName.Should().Be("cosmosDb");
        legacy.ModuleBicepContent.Should().NotBeNullOrWhiteSpace();
        legacy.ModuleTypesBicepContent.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_ParametersDictIsEmpty()
    {
        var resource = CreateResource();
        var legacy = _sut.Generate(resource);

        legacy.Parameters.Should().BeEmpty();
    }

    // ── Emission parity ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllExpectedSections()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { DatabaseKind, ConsistencyLevel, BackupPolicyType } from './types.bicep'");
        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param kind DatabaseKind = 'GlobalDocumentDB'");
        bicep.Should().Contain("param consistencyLevel ConsistencyLevel = 'Session'");
        bicep.Should().Contain("param maxStalenessPrefix int = 100");
        bicep.Should().Contain("param maxIntervalInSeconds int = 5");
        bicep.Should().Contain("param enableAutomaticFailover bool = false");
        bicep.Should().Contain("param enableMultipleWriteLocations bool = false");
        bicep.Should().Contain("param backupPolicyType BackupPolicyType = 'Periodic'");
        bicep.Should().Contain("param enableFreeTier bool = false");
        bicep.Should().Contain("param capabilities array = []");
        bicep.Should().Contain("resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15'");
        bicep.Should().Contain("name: name");
        bicep.Should().Contain("location: location");
        bicep.Should().Contain("kind: kind");
        bicep.Should().Contain("databaseAccountOfferType: 'Standard'");
        bicep.Should().Contain("defaultConsistencyLevel: consistencyLevel");
        bicep.Should().Contain("maxStalenessPrefix: maxStalenessPrefix");
        bicep.Should().Contain("maxIntervalInSeconds: maxIntervalInSeconds");
        bicep.Should().Contain("enableAutomaticFailover: enableAutomaticFailover");
        bicep.Should().Contain("enableMultipleWriteLocations: enableMultipleWriteLocations");
        bicep.Should().Contain("type: backupPolicyType");
        bicep.Should().Contain("enableFreeTier: enableFreeTier");
        bicep.Should().Contain("capabilities: capabilities");
        bicep.Should().Contain("locationName: location");
        bicep.Should().Contain("failoverPriority: 0");
        bicep.Should().Contain("isZoneRedundant: false");
        bicep.Should().Contain("output id string = cosmosDbAccount.id");
        bicep.Should().Contain("output documentEndpoint string = cosmosDbAccount.properties.documentEndpoint");
        bicep.Should().Contain("output name string = cosmosDbAccount.name");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsAllThreeExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("type DatabaseKind = 'GlobalDocumentDB' | 'MongoDB' | 'Parse'");
        types.Should().Contain("type ConsistencyLevel = 'Eventual' | 'Session' | 'BoundedStaleness' | 'Strong' | 'ConsistentPrefix'");
        types.Should().Contain("type BackupPolicyType = 'Periodic' | 'Continuous'");
    }
}
