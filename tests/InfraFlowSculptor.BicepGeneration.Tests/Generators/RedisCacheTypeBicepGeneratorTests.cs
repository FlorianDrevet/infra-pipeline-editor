using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Generators;

public sealed class RedisCacheTypeBicepGeneratorTests
{
    private readonly RedisCacheTypeBicepGenerator _sut = new();

    private static ResourceDefinition CreateResource(
        Dictionary<string, string>? properties = null) => new()
    {
        ResourceId = Guid.NewGuid(),
        Name = "my-redis",
        Type = AzureResourceTypes.ArmTypes.RedisCache,
        ResourceGroupName = "rg-test",
        ResourceAbbreviation = "redis",
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
        _sut.ResourceType.Should().Be(AzureResourceTypes.ArmTypes.RedisCache);
        _sut.ResourceTypeName.Should().Be(AzureResourceTypes.RedisCache);
    }

    // ── Spec structure ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ModuleIdentityIsCorrect()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.ModuleName.Should().Be("redisCache");
        spec.ModuleFolderName.Should().Be("RedisCache");
        spec.ResourceTypeName.Should().Be(AzureResourceTypes.RedisCache);
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
                i.Symbols.Contains("SkuName") &&
                i.Symbols.Contains("SkuFamily") &&
                i.Symbols.Contains("TlsVersion"));
    }

    // ── Parameters ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTenParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Parameters.Should().HaveCount(10);
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
    public void Given_Resource_When_GenerateSpec_Then_HasSkuNameParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "skuName").Which;

        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuName");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("Basic");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuFamilyParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "skuFamily").Which;

        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("SkuFamily");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("C");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasCapacityParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "capacity")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.Int &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasRedisVersionParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "redisVersion")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.String &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasEnableNonSslPortParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Parameters.Should().Contain(p => p.Name == "enableNonSslPort")
            .Which.Should().Match<BicepParam>(p =>
                p.Type == BicepType.Bool &&
                p.DefaultValue == null);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasMinimumTlsVersionParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "minimumTlsVersion").Which;

        param.Type.Should().BeOfType<BicepCustomType>()
            .Which.Name.Should().Be("TlsVersion");
        param.DefaultValue.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("1.2");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasDisableAccessKeyAuthenticationParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "disableAccessKeyAuthentication").Which;

        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasAadEnabledParam()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var param = spec.Parameters.Should().Contain(p => p.Name == "aadEnabled").Which;

        param.Type.Should().Be(BicepType.Bool);
        param.DefaultValue.Should().BeOfType<BicepBoolLiteral>()
            .Which.Value.Should().BeFalse();
    }

    // ── Resource ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasCorrectSymbolAndArmType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        spec.Resource.Symbol.Should().Be("redis");
        spec.Resource.ArmTypeWithApiVersion.Should().Be("Microsoft.Cache/Redis@2023-08-01");
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
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasNestedSku()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var body = spec.Resource.Body;

        var properties = body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var sku = properties.Properties.Should().Contain(p => p.Key == "sku")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        sku.Properties.Should().HaveCount(3);

        sku.Properties.Should().Contain(p => p.Key == "name")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("skuName");

        sku.Properties.Should().Contain(p => p.Key == "family")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("skuFamily");

        sku.Properties.Should().Contain(p => p.Key == "capacity")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("capacity");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasScalarProperties()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        properties.Properties.Should().Contain(p => p.Key == "redisVersion")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("redisVersion");

        properties.Properties.Should().Contain(p => p.Key == "enableNonSslPort")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("enableNonSslPort");

        properties.Properties.Should().Contain(p => p.Key == "minimumTlsVersion")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("minimumTlsVersion");

        properties.Properties.Should().Contain(p => p.Key == "disableAccessKeyAuthentication")
            .Which.Value.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("disableAccessKeyAuthentication");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_ResourceHasRedisConfigurationWithConditionalAad()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var redisConfig = properties.Properties.Should().Contain(p => p.Key == "redisConfiguration")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var aadProp = redisConfig.Properties.Should().ContainSingle().Subject;
        aadProp.Key.Should().Be("'aad-enabled'");
        aadProp.Value.Should().BeOfType<BicepConditionalExpression>();
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_AadConditionalHasCorrectBranches()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var properties = spec.Resource.Body.Should().Contain(p => p.Key == "properties")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var redisConfig = properties.Properties.Should().Contain(p => p.Key == "redisConfiguration")
            .Which.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        var conditional = redisConfig.Properties.First().Value.Should()
            .BeOfType<BicepConditionalExpression>().Subject;

        conditional.Condition.Should().BeOfType<BicepReference>()
            .Which.Symbol.Should().Be("aadEnabled");
        conditional.Consequent.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("true");
        conditional.Alternate.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("false");
    }

    // ── Outputs ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasFourOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.Outputs.Should().HaveCount(4);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasIdOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "id").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("redis.id");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasHostNameOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "hostName").Which;
        output.Type.Should().Be(BicepType.String);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("redis.properties.hostName");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSslPortOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "sslPort").Which;
        output.Type.Should().Be(BicepType.Int);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("redis.properties.sslPort");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasPortOutput()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var output = spec.Outputs.Should().Contain(o => o.Name == "port").Which;
        output.Type.Should().Be(BicepType.Int);
        output.Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("redis.properties.port");
    }

    // ── Exported types ──

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasThreeExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        spec.ExportedTypes.Should().HaveCount(3);
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuNameExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SkuName").Which;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'Basic' | 'Standard' | 'Premium'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasSkuFamilyExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "SkuFamily").Which;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'C' | 'P'");
    }

    [Fact]
    public void Given_Resource_When_GenerateSpec_Then_HasTlsVersionExportedType()
    {
        var spec = _sut.GenerateSpec(CreateResource());

        var type = spec.ExportedTypes.Should().Contain(t => t.Name == "TlsVersion").Which;
        type.Body.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("'1.0' | '1.1' | '1.2'");
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

        module.ModuleName.Should().Be("redisCache");
        module.ModuleFolderName.Should().Be("RedisCache");
        module.ResourceTypeName.Should().Be(AzureResourceTypes.RedisCache);
    }

    [Fact]
    public void Given_Resource_When_LegacyGenerate_Then_ParametersDictHasDefaultValues()
    {
        var module = _sut.Generate(CreateResource());

        module.Parameters.Should().ContainKey("skuName").WhoseValue.Should().Be("Basic");
        module.Parameters.Should().ContainKey("skuFamily").WhoseValue.Should().Be("C");
        module.Parameters.Should().ContainKey("capacity").WhoseValue.Should().Be(1);
        module.Parameters.Should().ContainKey("redisVersion").WhoseValue.Should().Be("6");
        module.Parameters.Should().ContainKey("enableNonSslPort").WhoseValue.Should().Be(false);
        module.Parameters.Should().ContainKey("minimumTlsVersion").WhoseValue.Should().Be("1.2");
        module.Parameters.Should().ContainKey("disableAccessKeyAuthentication").WhoseValue.Should().Be(false);
        module.Parameters.Should().ContainKey("aadEnabled").WhoseValue.Should().Be(false);
    }

    [Fact]
    public void Given_ResourceWithOverrides_When_LegacyGenerate_Then_ParametersDictReflectsOverrides()
    {
        var props = new Dictionary<string, string>
        {
            ["skuName"] = "Premium",
            ["skuFamily"] = "P",
            ["capacity"] = "3",
            ["redisVersion"] = "4",
            ["enableNonSslPort"] = "true",
            ["minimumTlsVersion"] = "1.1",
            ["disableAccessKeyAuthentication"] = "true",
            ["aadEnabled"] = "true",
        };

        var module = _sut.Generate(CreateResource(props));

        module.Parameters["skuName"].Should().Be("Premium");
        module.Parameters["skuFamily"].Should().Be("P");
        module.Parameters["capacity"].Should().Be(3);
        module.Parameters["redisVersion"].Should().Be("4");
        module.Parameters["enableNonSslPort"].Should().Be(true);
        module.Parameters["minimumTlsVersion"].Should().Be("1.1");
        module.Parameters["disableAccessKeyAuthentication"].Should().Be(true);
        module.Parameters["aadEnabled"].Should().Be(true);
    }

    [Fact]
    public void Given_ResourceWithInvalidCapacity_When_LegacyGenerate_Then_DefaultsToOne()
    {
        var props = new Dictionary<string, string> { ["capacity"] = "not-a-number" };

        var module = _sut.Generate(CreateResource(props));

        module.Parameters["capacity"].Should().Be(1);
    }

    // ── Emission ──

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsImport()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("import { SkuName, SkuFamily, TlsVersion } from './types.bicep'");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllParams()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("param location string");
        bicep.Should().Contain("param name string");
        bicep.Should().Contain("param skuName SkuName = 'Basic'");
        bicep.Should().Contain("param skuFamily SkuFamily = 'C'");
        bicep.Should().Contain("param capacity int");
        bicep.Should().Contain("param redisVersion string");
        bicep.Should().Contain("param enableNonSslPort bool");
        bicep.Should().Contain("param minimumTlsVersion TlsVersion = '1.2'");
        bicep.Should().Contain("param disableAccessKeyAuthentication bool = false");
        bicep.Should().Contain("param aadEnabled bool = false");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsResourceDeclaration()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("resource redis 'Microsoft.Cache/Redis@2023-08-01' = {");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsSkuBlock()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("name: skuName");
        bicep.Should().Contain("family: skuFamily");
        bicep.Should().Contain("capacity: capacity");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsRedisConfigurationWithConditional()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("'aad-enabled': aadEnabled ? 'true' : 'false'");
    }

    [Fact]
    public void Given_Resource_When_EmitModule_Then_ContainsAllOutputs()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var bicep = emitter.EmitModule(spec);

        bicep.Should().Contain("output id string = redis.id");
        bicep.Should().Contain("output hostName string = redis.properties.hostName");
        bicep.Should().Contain("output sslPort int = redis.properties.sslPort");
        bicep.Should().Contain("output port int = redis.properties.port");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_ContainsAllExportedTypes()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("type SkuName = 'Basic' | 'Standard' | 'Premium'");
        types.Should().Contain("type SkuFamily = 'C' | 'P'");
        types.Should().Contain("type TlsVersion = '1.0' | '1.1' | '1.2'");
    }

    [Fact]
    public void Given_Resource_When_EmitTypes_Then_HasExportAndDescriptionDecorators()
    {
        var spec = _sut.GenerateSpec(CreateResource());
        var emitter = new BicepEmitter();
        var types = emitter.EmitTypes(spec);

        types.Should().Contain("@export()");
        types.Should().Contain("@description('SKU name for the Redis Cache')");
        types.Should().Contain("@description('SKU family for the Redis Cache (C for Basic/Standard, P for Premium)')");
        types.Should().Contain("@description('Minimum TLS version for Redis Cache connections')");
    }
}
