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
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.RedisCache;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.RedisCache;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("redisCache", "RedisCache", ResourceTypeName)
            .Import("./types.bicep", "SkuName", "SkuFamily", "TlsVersion")
            .Param("location", BicepType.String, "Azure region for the Redis Cache")
            .Param("name", BicepType.String, "Name of the Redis Cache")
            .Param("skuName", BicepType.Custom("SkuName"), "SKU name of the Redis Cache",
                defaultValue: new BicepStringLiteral("Basic"))
            .Param("skuFamily", BicepType.Custom("SkuFamily"), "SKU family of the Redis Cache",
                defaultValue: new BicepStringLiteral("C"))
            .Param("capacity", BicepType.Int, "Cache capacity (number of shards for Basic/Standard, shard count for Premium)")
            .Param("redisVersion", BicepType.String, "Redis server version")
            .Param("enableNonSslPort", BicepType.Bool, "Whether the non-SSL port (6379) is enabled")
            .Param("minimumTlsVersion", BicepType.Custom("TlsVersion"), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral("1.2"))
            .Param("disableAccessKeyAuthentication", BicepType.Bool, "Whether access key authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("aadEnabled", BicepType.Bool, "Whether Microsoft Entra ID (AAD) authentication is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Resource("redis", "Microsoft.Cache/Redis@2023-08-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("sku", sku => sku
                    .Property("name", new BicepReference("skuName"))
                    .Property("family", new BicepReference("skuFamily"))
                    .Property("capacity", new BicepReference("capacity")))
                .Property("redisVersion", new BicepReference("redisVersion"))
                .Property("enableNonSslPort", new BicepReference("enableNonSslPort"))
                .Property("minimumTlsVersion", new BicepReference("minimumTlsVersion"))
                .Property("disableAccessKeyAuthentication", new BicepReference("disableAccessKeyAuthentication"))
                .Property("redisConfiguration", rc => rc
                    .Property("'aad-enabled'", new BicepConditionalExpression(
                        new BicepReference("aadEnabled"),
                        new BicepStringLiteral("true"),
                        new BicepStringLiteral("false")))))
            .Output("id", BicepType.String, new BicepRawExpression("redis.id"),
                description: "The resource ID of the Redis Cache")
            .Output("hostName", BicepType.String, new BicepRawExpression("redis.properties.hostName"),
                description: "The host name of the Redis Cache")
            .Output("sslPort", BicepType.Int, new BicepRawExpression("redis.properties.sslPort"),
                description: "The SSL port of the Redis Cache")
            .Output("port", BicepType.Int, new BicepRawExpression("redis.properties.port"),
                description: "The non-SSL port of the Redis Cache")
            .ExportedType("SkuName",
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium'"),
                description: "SKU name for the Redis Cache")
            .ExportedType("SkuFamily",
                new BicepRawExpression("'C' | 'P'"),
                description: "SKU family for the Redis Cache (C for Basic/Standard, P for Premium)")
            .ExportedType("TlsVersion",
                new BicepRawExpression("'1.0' | '1.1' | '1.2'"),
                description: "Minimum TLS version for Redis Cache connections")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "redisCache",
            ModuleFileName = "redisCache",
            ModuleFolderName = "RedisCache",
            ModuleBicepContent = RedisCacheModuleTemplate,
            ModuleTypesBicepContent = RedisCacheTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["skuName"] = resource.Properties.GetValueOrDefault("skuName", "Basic"),
                ["skuFamily"] = resource.Properties.GetValueOrDefault("skuFamily", "C"),
                ["capacity"] = int.TryParse(resource.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                ["redisVersion"] = resource.Properties.GetValueOrDefault("redisVersion", "6"),
                ["enableNonSslPort"] = resource.Properties.GetValueOrDefault("enableNonSslPort", "false") == "true",
                ["minimumTlsVersion"] = resource.Properties.GetValueOrDefault("minimumTlsVersion", "1.2"),
                ["disableAccessKeyAuthentication"] = resource.Properties.GetValueOrDefault("disableAccessKeyAuthentication", "false") == "true",
                ["aadEnabled"] = resource.Properties.GetValueOrDefault("aadEnabled", "false") == "true",
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
