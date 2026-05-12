using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Cache for Redis (<c>Microsoft.Cache/Redis@2023-08-01</c>).
/// </summary>
public sealed class RedisCacheTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "redisCache";
    private const string ModuleFolderName = "RedisCache";
    private const string ResourceSymbol = "redis";
    private const string SkuNameTypeName = "SkuName";
    private const string SkuFamilyTypeName = "SkuFamily";
    private const string TlsVersionTypeName = "TlsVersion";
    private const string SkuNameParameterName = "skuName";
    private const string SkuFamilyParameterName = "skuFamily";
    private const string CapacityParameterName = "capacity";
    private const string RedisVersionParameterName = "redisVersion";
    private const string EnableNonSslPortParameterName = "enableNonSslPort";
    private const string MinimumTlsVersionParameterName = "minimumTlsVersion";
    private const string DisableAccessKeyAuthenticationParameterName = "disableAccessKeyAuthentication";
    private const string AadEnabledParameterName = "aadEnabled";
    private const string RedisArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.RedisCacheArmType;
    private const string AadEnabledConfigurationKey = "'aad-enabled'";
    private const string DefaultSkuName = "Basic";
    private const string DefaultSkuFamily = "C";
    private const string DefaultMinimumTlsVersion = "1.2";
    private const string DefaultCapacityText = "1";
    private const string DefaultRedisVersion = "6";
    private const string SkuPropertyName = "sku";
    private const string FamilyPropertyName = "family";
    private const string RedisConfigurationPropertyName = "redisConfiguration";
    private const string HostNameOutputName = "hostName";
    private const string SslPortOutputName = "sslPort";
    private const string PortOutputName = "port";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string HostNameExpression = ResourceSymbol + ".properties.hostName";
    private const string SslPortExpression = ResourceSymbol + ".properties.sslPort";
    private const string PortExpression = ResourceSymbol + ".properties.port";
    private const string SkuNameUnion = "'Basic' | 'Standard' | 'Premium'";
    private const string SkuFamilyUnion = "'C' | 'P'";
    private const string TlsVersionUnion = "'1.0' | '1.1' | '1.2'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.RedisCacheType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.RedisCache;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName, SkuFamilyTypeName, TlsVersionTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Redis Cache")
            .Param(NameParameterName, BicepType.String, "Name of the Redis Cache")
            .Param(SkuNameParameterName, BicepType.Custom(SkuNameTypeName), "SKU name of the Redis Cache",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(SkuFamilyParameterName, BicepType.Custom(SkuFamilyTypeName), "SKU family of the Redis Cache",
                defaultValue: new BicepStringLiteral(DefaultSkuFamily))
            .Param(CapacityParameterName, BicepType.Int, "Cache capacity (number of shards for Basic/Standard, shard count for Premium)")
            .Param(RedisVersionParameterName, BicepType.String, "Redis server version")
            .Param(EnableNonSslPortParameterName, BicepType.Bool, "Whether the non-SSL port (6379) is enabled")
            .Param(MinimumTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral(DefaultMinimumTlsVersion))
            .Param(DisableAccessKeyAuthenticationParameterName, BicepType.Bool, "Whether access key authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(AadEnabledParameterName, BicepType.Bool, "Whether Microsoft Entra ID (AAD) authentication is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Resource(ResourceSymbol, RedisArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(SkuPropertyName, sku => sku
                    .Property(NamePropertyName, new BicepReference(SkuNameParameterName))
                    .Property(FamilyPropertyName, new BicepReference(SkuFamilyParameterName))
                    .Property(CapacityParameterName, new BicepReference(CapacityParameterName)))
                .Property(RedisVersionParameterName, new BicepReference(RedisVersionParameterName))
                .Property(EnableNonSslPortParameterName, new BicepReference(EnableNonSslPortParameterName))
                .Property(MinimumTlsVersionParameterName, new BicepReference(MinimumTlsVersionParameterName))
                .Property(DisableAccessKeyAuthenticationParameterName, new BicepReference(DisableAccessKeyAuthenticationParameterName))
                .Property(RedisConfigurationPropertyName, rc => rc
                    .Property(AadEnabledConfigurationKey, new BicepConditionalExpression(
                        new BicepReference(AadEnabledParameterName),
                        new BicepStringLiteral(BooleanTrueString),
                        new BicepStringLiteral(BooleanFalseString)))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Redis Cache")
            .Output(HostNameOutputName, BicepType.String, new BicepRawExpression(HostNameExpression),
                description: "The host name of the Redis Cache")
            .Output(SslPortOutputName, BicepType.Int, new BicepRawExpression(SslPortExpression),
                description: "The SSL port of the Redis Cache")
            .Output(PortOutputName, BicepType.Int, new BicepRawExpression(PortExpression),
                description: "The non-SSL port of the Redis Cache")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the Redis Cache")
            .ExportedType(SkuFamilyTypeName,
                new BicepRawExpression(SkuFamilyUnion),
                description: "SKU family for the Redis Cache (C for Basic/Standard, P for Premium)")
            .ExportedType(TlsVersionTypeName,
                new BicepRawExpression(TlsVersionUnion),
                description: "Minimum TLS version for Redis Cache connections")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = RedisCacheModuleTemplate,
            ModuleTypesBicepContent = RedisCacheTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new RedisCacheParameters
            {
                SkuName = resource.Properties.GetValueOrDefault(SkuNameParameterName, DefaultSkuName),
                SkuFamily = resource.Properties.GetValueOrDefault(SkuFamilyParameterName, DefaultSkuFamily),
                Capacity = int.TryParse(resource.Properties.GetValueOrDefault(CapacityParameterName, DefaultCapacityText), out var cap) ? cap : 1,
                RedisVersion = resource.Properties.GetValueOrDefault(RedisVersionParameterName, DefaultRedisVersion),
                EnableNonSslPort = resource.Properties.GetValueOrDefault(EnableNonSslPortParameterName, BooleanFalseString) == BooleanTrueString,
                MinimumTlsVersion = resource.Properties.GetValueOrDefault(MinimumTlsVersionParameterName, DefaultMinimumTlsVersion),
                DisableAccessKeyAuthentication = resource.Properties.GetValueOrDefault(DisableAccessKeyAuthenticationParameterName, BooleanFalseString) == BooleanTrueString,
                AadEnabled = resource.Properties.GetValueOrDefault(AadEnabledParameterName, BooleanFalseString) == BooleanTrueString,
            })
        };
    }

    private const string RedisCacheTypesTemplate = """
        @export()
        @description('SKU name for the Redis Cache')
        type SkuName = 'Basic' | 'Standard' | 'Premium'

        @export()
        @description('SKU family for the Redis Cache (C for Basic/Standard, P for Premium)')
        type SkuFamily = 'C' | 'P'

        @export()
        @description('Minimum TLS version for Redis Cache connections')
        type TlsVersion = '1.0' | '1.1' | '1.2'
        """;

    private static readonly string RedisCacheModuleTemplate = $$"""
        import { SkuName, SkuFamily, TlsVersion } from './types.bicep'

        @description('Azure region for the Redis Cache')
        param location string

        @description('Name of the Redis Cache')
        param name string

        @description('SKU name of the Redis Cache')
        param skuName SkuName = 'Basic'

        @description('SKU family of the Redis Cache')
        param skuFamily SkuFamily = 'C'

        @description('Cache capacity (number of shards for Basic/Standard, shard count for Premium)')
        param capacity int

        @description('Redis server version')
        param redisVersion string

        @description('Whether the non-SSL port (6379) is enabled')
        param enableNonSslPort bool

        @description('Minimum TLS version for client connections')
        param minimumTlsVersion TlsVersion = '1.2'

        @description('Whether access key authentication is disabled')
        param disableAccessKeyAuthentication bool = false

        @description('Whether Microsoft Entra ID (AAD) authentication is enabled')
        param aadEnabled bool = false

        resource redis '{{RedisArmType}}' = {
          name: name
          location: location
          properties: {
            sku: {
              name: skuName
              family: skuFamily
              capacity: capacity
            }
            redisVersion: redisVersion
            enableNonSslPort: enableNonSslPort
            minimumTlsVersion: minimumTlsVersion
            disableAccessKeyAuthentication: disableAccessKeyAuthentication
            redisConfiguration: {
              'aad-enabled': aadEnabled ? 'true' : 'false'
            }
          }
        }

        @description('The resource ID of the Redis Cache')
        output id string = redis.id

        @description('The host name of the Redis Cache')
        output hostName string = redis.properties.hostName

        @description('The SSL port of the Redis Cache')
        output sslPort int = redis.properties.sslPort

        @description('The non-SSL port of the Redis Cache')
        output port int = redis.properties.port
        """;
}
