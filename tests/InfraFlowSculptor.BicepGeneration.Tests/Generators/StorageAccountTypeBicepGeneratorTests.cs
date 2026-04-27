using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class StorageAccountTypeBicepGeneratorTests
{
    private readonly StorageAccountTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null)
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-storage",
            Type = AzureResourceTypes.ArmTypes.StorageAccount,
            ResourceGroupName = "rg-test",
            ResourceAbbreviation = "st",
            Properties = properties ?? new Dictionary<string, string>(),
        };
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.StorageAccount);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.StorageAccount);
    }

    // ── Module identity ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("storageAccount");
        spec.ModuleFolderName.Should().Be("StorageAccount");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.StorageAccount);
        spec.ModuleFileName.Should().BeNull();
    }

    // ── Imports ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ImportsFourTypesFromTypesBicep()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Imports.Should().ContainSingle()
            .Which.Should().Match<BicepImport>(i =>
                i.Path == "./types.bicep" &&
                i.Symbols != null &&
                i.Symbols.Contains("SkuName") &&
                i.Symbols.Contains("StorageKind") &&
                i.Symbols.Contains("AccessTier") &&
                i.Symbols.Contains("TlsVersion"));
    }

    // ── Params ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEightParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(8);
    }

    [Theory]
    [InlineData("location")]
    [InlineData("name")]
    public void Given_Resource_When_GenerateSpec_Then_HasStringParam(string paramName)
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().Contain(p => p.Name == paramName)
            .Which.Type.Should().Be(BicepType.String);
    }

    [Theory]
    [InlineData("sku", "SkuName", "Standard_LRS")]
    [InlineData("kind", "StorageKind", "StorageV2")]
    [InlineData("accessTier", "AccessTier", "Hot")]
    [InlineData("minimumTlsVersion", "TlsVersion", "TLS1_2")]
    public void Given_Resource_When_GenerateSpec_Then_HasCustomTypeParamWithDefault(
        string paramName, string typeName, string defaultValue)
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == paramName).Subject;
        param.Type.Should().BeOfType<BicepCustomType>().Which.Name.Should().Be(typeName);
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>().Which.Value.Should().Be(defaultValue);
    }

    [Theory]
    [InlineData("allowBlobPublicAccess")]
    [InlineData("supportsHttpsTrafficOnly")]
    public void Given_Resource_When_GenerateSpec_Then_HasBoolParam(string paramName)
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == paramName).Subject;
        param.Type.Should().Be(BicepType.Bool);
    }

    // ── Variables ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasNoVariables()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Variables.Should().BeEmpty();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Resource.Symbol.Should().Be("storage");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Storage/storageAccounts@2025-06-01");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasNameAndLocation()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Resource.Body.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepReference>().Which.Symbol.Should().Be("name");
        spec.Resource.Body.Should().Contain(p => p.Key == "location")
            .Which.Value.Should().BeOfType<BicepReference>().Which.Symbol.Should().Be("location");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasKindParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Resource.Body.Should().Contain(p => p.Key == "kind")
            .Which.Value.Should().BeOfType<BicepReference>().Which.Symbol.Should().Be("kind");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasSkuObject()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var sku = spec.Resource.Body.Should().Contain(p => p.Key == "sku").Subject;
        var skuObj = sku.Value.Should().BeOfType<BicepObjectExpression>().Subject;
        skuObj.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("name");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasIdentityObject()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var identity = spec.Resource.Body.Should().Contain(p => p.Key == "identity").Subject;
        var identityObj = identity.Value.Should().BeOfType<BicepObjectExpression>().Subject;
        identityObj.Properties.Should().ContainSingle()
            .Which.Key.Should().Be("type");
        identityObj.Properties[0].Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("SystemAssigned");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceBodyHasPropertiesWithFourItems()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties").Subject;
        var propsObj = properties.Value.Should().BeOfType<BicepObjectExpression>().Subject;
        propsObj.Properties.Should().HaveCount(4);
        propsObj.Properties.Should().Contain(p => p.Key == "allowBlobPublicAccess");
        propsObj.Properties.Should().Contain(p => p.Key == "supportsHttpsTrafficOnly");
        propsObj.Properties.Should().Contain(p => p.Key == "minimumTlsVersion");
        propsObj.Properties.Should().Contain(p => p.Key == "accessTier");
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSixOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(6);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("name")]
    [InlineData("primaryBlobEndpoint")]
    [InlineData("primaryTableEndpoint")]
    [InlineData("primaryQueueEndpoint")]
    [InlineData("primaryFileEndpoint")]
    public void Given_Resource_When_GenerateSpec_Then_HasStringOutput(string outputName)
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().Contain(o => o.Name == outputName)
            .Which.Type.Should().Be(BicepType.String);
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasFourExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("SkuName")]
    [InlineData("StorageKind")]
    [InlineData("AccessTier")]
    [InlineData("TlsVersion")]
    public void Given_Resource_When_GenerateSpec_Then_HasExportedType(string typeName)
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().Contain(t => t.Name == typeName)
            .Which.IsExported.Should().BeTrue();
    }

    // ── Legacy compat ──

    [Fact]
    public void Given_Resource_When_Generate_Then_ModuleFileNameIsNormalized()
    {
        var module = _sut.Generate(CreateResource());
        module.ModuleFileName.Should().Be("storageAccount.module.bicep");
    }

    [Fact]
    public void Given_Resource_When_Generate_Then_NoSecureParameters()
    {
        var module = _sut.Generate(CreateResource());
        module.SecureParameters.Should().BeEmpty();
    }

    [Fact]
    public void Given_ResourceWithBlobContainers_When_Generate_Then_HasBlobsCompanion()
    {
        var resource = CreateResource(new Dictionary<string, string>
        {
            ["blobContainerNames"] = "[\"logs\",\"data\"]",
        });
        var module = _sut.Generate(resource);
        module.CompanionModules.Should().Contain(c => c.ModuleSymbolSuffix == "Blobs");
    }

    [Fact]
    public void Given_ResourceWithQueues_When_Generate_Then_HasQueuesCompanion()
    {
        var resource = CreateResource(new Dictionary<string, string>
        {
            ["queueNames"] = "[\"queue1\",\"queue2\"]",
        });
        var module = _sut.Generate(resource);
        module.CompanionModules.Should().Contain(c => c.ModuleSymbolSuffix == "Queues");
    }

    [Fact]
    public void Given_ResourceWithTables_When_Generate_Then_HasTablesCompanion()
    {
        var resource = CreateResource(new Dictionary<string, string>
        {
            ["storageTableNames"] = "[\"table1\"]",
        });
        var module = _sut.Generate(resource);
        module.CompanionModules.Should().Contain(c => c.ModuleSymbolSuffix == "Tables");
    }

    [Fact]
    public void Given_ResourceWithDefaults_When_Generate_Then_ParametersHaveDefaults()
    {
        var module = _sut.Generate(CreateResource());
        module.Parameters.Should().ContainKey("sku");
        module.Parameters.Should().ContainKey("kind");
        module.Parameters.Should().ContainKey("accessTier");
    }

    // ── Emission ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsResourceDeclaration()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("resource storage 'Microsoft.Storage/storageAccounts@2025-06-01'");
        emitted.Should().Contain("param location string");
        emitted.Should().Contain("param sku SkuName");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsIdentityBlock()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("identity");
        emitted.Should().Contain("SystemAssigned");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsSixOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitted = new BicepEmitter().EmitModule(spec);

        emitted.Should().Contain("output id string");
        emitted.Should().Contain("output name string");
        emitted.Should().Contain("output primaryBlobEndpoint string");
        emitted.Should().Contain("output primaryTableEndpoint string");
        emitted.Should().Contain("output primaryQueueEndpoint string");
        emitted.Should().Contain("output primaryFileEndpoint string");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsAllFourTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitted = new BicepEmitter().EmitTypes(spec);

        emitted.Should().Contain("type SkuName");
        emitted.Should().Contain("type StorageKind");
        emitted.Should().Contain("type AccessTier");
        emitted.Should().Contain("type TlsVersion");
    }
}
