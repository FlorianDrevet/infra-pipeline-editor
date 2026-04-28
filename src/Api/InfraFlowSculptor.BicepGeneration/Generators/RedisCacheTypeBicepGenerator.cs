using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

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
    private const string RedisVersionParameterName = "redisVersion";
    private const string EnableNonSslPortParameterName = "enableNonSslPort";
    private const string MinimumTlsVersionParameterName = "minimumTlsVersion";
    private const string DisableAccessKeyAuthenticationParameterName = "disableAccessKeyAuthentication";
    private const string AadEnabledParameterName = "aadEnabled";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.RedisCache;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.RedisCache;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import("./types.bicep", SkuNameTypeName, SkuFamilyTypeName, TlsVersionTypeName)
            .Param("location", BicepType.String, "Azure region for the Redis Cache")
            .Param("name", BicepType.String, "Name of the Redis Cache")
            .Param(SkuNameParameterName, BicepType.Custom(SkuNameTypeName), "SKU name of the Redis Cache",
                defaultValue: new BicepStringLiteral("Basic"))
            .Param(SkuFamilyParameterName, BicepType.Custom(SkuFamilyTypeName), "SKU family of the Redis Cache",
                defaultValue: new BicepStringLiteral("C"))
            .Param("capacity", BicepType.Int, "Cache capacity (number of shards for Basic/Standard, shard count for Premium)")
            .Param(RedisVersionParameterName, BicepType.String, "Redis server version")
            .Param(EnableNonSslPortParameterName, BicepType.Bool, "Whether the non-SSL port (6379) is enabled")
            .Param(MinimumTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral("1.2"))
            .Param(DisableAccessKeyAuthenticationParameterName, BicepType.Bool, "Whether access key authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(AadEnabledParameterName, BicepType.Bool, "Whether Microsoft Entra ID (AAD) authentication is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Resource(ResourceSymbol, "Microsoft.Cache/Redis@2023-08-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("sku", sku => sku
                    .Property("name", new BicepReference(SkuNameParameterName))
                    .Property("family", new BicepReference(SkuFamilyParameterName))
                    .Property("capacity", new BicepReference("capacity")))
                .Property("redisVersion", new BicepReference(RedisVersionParameterName))
                .Property("enableNonSslPort", new BicepReference(EnableNonSslPortParameterName))
                .Property("minimumTlsVersion", new BicepReference(MinimumTlsVersionParameterName))
                .Property("disableAccessKeyAuthentication", new BicepReference(DisableAccessKeyAuthenticationParameterName))
                .Property("redisConfiguration", rc => rc
                    .Property("'aad-enabled'", new BicepConditionalExpression(
                        new BicepReference(AadEnabledParameterName),
                        new BicepStringLiteral("true"),
                        new BicepStringLiteral("false")))))
            .Output("id", BicepType.String, new BicepRawExpression($"{ResourceSymbol}.id"),
                description: "The resource ID of the Redis Cache")
            .Output("hostName", BicepType.String, new BicepRawExpression($"{ResourceSymbol}.properties.hostName"),
                description: "The host name of the Redis Cache")
            .Output("sslPort", BicepType.Int, new BicepRawExpression($"{ResourceSymbol}.properties.sslPort"),
                description: "The SSL port of the Redis Cache")
            .Output("port", BicepType.Int, new BicepRawExpression($"{ResourceSymbol}.properties.port"),
                description: "The non-SSL port of the Redis Cache")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium'"),
                description: "SKU name for the Redis Cache")
            .ExportedType(SkuFamilyTypeName,
                new BicepRawExpression("'C' | 'P'"),
                description: "SKU family for the Redis Cache (C for Basic/Standard, P for Premium)")
            .ExportedType(TlsVersionTypeName,
                new BicepRawExpression("'1.0' | '1.1' | '1.2'"),
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
            Parameters = new Dictionary<string, object>
            {
                [SkuNameParameterName] = resource.Properties.GetValueOrDefault(SkuNameParameterName, "Basic"),
                [SkuFamilyParameterName] = resource.Properties.GetValueOrDefault(SkuFamilyParameterName, "C"),
                ["capacity"] = int.TryParse(resource.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                [RedisVersionParameterName] = resource.Properties.GetValueOrDefault(RedisVersionParameterName, "6"),
                [EnableNonSslPortParameterName] = resource.Properties.GetValueOrDefault(EnableNonSslPortParameterName, "false") == "true",
                [MinimumTlsVersionParameterName] = resource.Properties.GetValueOrDefault(MinimumTlsVersionParameterName, "1.2"),
                [DisableAccessKeyAuthenticationParameterName] = resource.Properties.GetValueOrDefault(DisableAccessKeyAuthenticationParameterName, "false") == "true",
                [AadEnabledParameterName] = resource.Properties.GetValueOrDefault(AadEnabledParameterName, "false") == "true",
            }
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

    private const string RedisCacheModuleTemplate = """
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

        resource redis 'Microsoft.Cache/Redis@2023-08-01' = {
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
