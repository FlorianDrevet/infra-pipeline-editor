using System.Text.Json;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class StorageAccountTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Storage/storageAccounts";

    /// <inheritdoc />
    public string ResourceTypeName => "StorageAccount";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var blobContainerNames = ParseBlobContainerNames(resource.Properties);
      var storageTableNames = ParseStorageTableNames(resource.Properties);
        var corsRules = ParseCorsRules(resource.Properties);

      var companions = new List<GeneratedCompanionModule>();

        if (blobContainerNames.Count > 0 || corsRules.Count > 0)
        {
        companions.Add(new GeneratedCompanionModule
            {
          ModuleSymbolSuffix = "Blobs",
          DeploymentNameSuffix = "Blobs",
                FileName = "storage.blobs.module.bicep",
                FolderName = "StorageAccount",
                BicepContent = BlobsModuleTemplate,
                TypesBicepContent = BlobsTypesTemplate,
                BlobContainerNames = blobContainerNames,
                CorsRules = corsRules
        });
      }

      if (storageTableNames.Count > 0)
      {
        companions.Add(new GeneratedCompanionModule
        {
          ModuleSymbolSuffix = "Tables",
          DeploymentNameSuffix = "Tables",
          FileName = "storage.table.module.bicep",
          FolderName = "StorageAccount",
          BicepContent = TablesModuleTemplate,
          TypesBicepContent = BlobsTypesTemplate,
          StorageTableNames = storageTableNames
        });
        }

        return new GeneratedTypeModule
        {
            ModuleName = "storageAccount",
            ModuleFileName = "storageAccount",
            ModuleFolderName = "StorageAccount",
            ModuleBicepContent = StorageAccountModuleTemplate,
            ModuleTypesBicepContent = StorageAccountTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            CompanionModules = companions,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Properties.GetValueOrDefault("sku", "Standard_LRS"),
                ["kind"] = resource.Properties.GetValueOrDefault("kind", "StorageV2"),
                ["accessTier"] = resource.Properties.GetValueOrDefault("accessTier", "Hot"),
                ["allowBlobPublicAccess"] = resource.Properties.GetValueOrDefault("allowBlobPublicAccess", "false") == "true",
                ["supportsHttpsTrafficOnly"] = resource.Properties.GetValueOrDefault("supportsHttpsTrafficOnly", "true") == "true",
                ["minimumTlsVersion"] = resource.Properties.GetValueOrDefault("minimumTlsVersion", "TLS1_2"),
            }
        };
    }

    private static List<string> ParseBlobContainerNames(IReadOnlyDictionary<string, string> properties)
    {
        if (!properties.TryGetValue("blobContainerNames", out var json) || string.IsNullOrEmpty(json))
            return [];

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static List<string> ParseStorageTableNames(IReadOnlyDictionary<string, string> properties)
    {
      if (!properties.TryGetValue("storageTableNames", out var json) || string.IsNullOrEmpty(json))
        return [];

      return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static List<BlobCorsRuleData> ParseCorsRules(IReadOnlyDictionary<string, string> properties)
    {
        if (!properties.TryGetValue("corsRules", out var json) || string.IsNullOrEmpty(json))
            return [];

        var raw = JsonSerializer.Deserialize<List<CorsRuleJson>>(json);
        if (raw is null) return [];

        return raw.Select(r => new BlobCorsRuleData(
            r.allowedOrigins ?? [],
            r.allowedMethods ?? [],
            r.allowedHeaders ?? [],
            r.exposedHeaders ?? [],
            r.maxAgeInSeconds))
            .ToList();
    }

    private sealed record CorsRuleJson(
        List<string>? allowedOrigins,
        List<string>? allowedMethods,
        List<string>? allowedHeaders,
        List<string>? exposedHeaders,
        int maxAgeInSeconds);


    private const string StorageAccountTypesTemplate = """
        @export()
        @description('SKU name for the Storage Account')
        type SkuName = 'Standard_LRS' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS'

        @export()
        @description('Kind of Storage Account')
        type StorageKind = 'BlobStorage' | 'BlockBlobStorage' | 'FileStorage' | 'Storage' | 'StorageV2'

        @export()
        @description('Access tier for the Storage Account')
        type AccessTier = 'Hot' | 'Cool' | 'Premium'

        @export()
        @description('Minimum TLS version for Storage Account connections')
        type TlsVersion = 'TLS1_0' | 'TLS1_1' | 'TLS1_2'
        """;

    private const string StorageAccountModuleTemplate = """
        import { SkuName, StorageKind, AccessTier, TlsVersion } from './types.bicep'

        @description('Azure region for the Storage Account')
        param location string

        @description('Name of the Storage Account')
        param name string

        @description('SKU of the Storage Account')
        param sku SkuName = 'Standard_LRS'

        @description('Kind of Storage Account')
        param kind StorageKind = 'StorageV2'

        @description('Access tier for blob storage')
        param accessTier AccessTier = 'Hot'

        @description('Whether public access to blobs is allowed')
        param allowBlobPublicAccess bool

        @description('Whether HTTPS traffic only is enforced')
        param supportsHttpsTrafficOnly bool

        @description('Minimum TLS version for client connections')
        param minimumTlsVersion TlsVersion = 'TLS1_2'

        resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' = {
          name: name
          location: location
          kind: kind
          sku: {
            name: sku
          }
          identity: {
            type: 'SystemAssigned'
          }
          properties: {
            allowBlobPublicAccess: allowBlobPublicAccess
            supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
            minimumTlsVersion: minimumTlsVersion
            accessTier: accessTier
          }
        }
        """;

    private const string BlobsTypesTemplate = """
        @export()
        @description('Describes a single CORS rule for the blob service')
        type CorsRuleDescription = {
          @description('Allowed origins')
          allowedOrigins: string[]
          @description('Allowed methods')
          allowedMethods: string[]
          @description('Allowed headers')
          allowedHeaders: string[]
          @description('Exposed headers')
          exposedHeaders: string[]
          @description('Max age in seconds')
          maxAgeInSeconds: int
        }
        """;

    private const string BlobsModuleTemplate = """
        import { CorsRuleDescription } from './types.bicep'

        @description('Storage account name')
        param storageAccountName string

        @description('Blob containers names')
        param blobContainerNames string[]

        @description('CORS rules')
        param corsRules CorsRuleDescription[] = []

        resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
          name: storageAccountName
        }

        resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' = {
          name: 'default'
          parent: storageAccount
          properties: {
            cors: {
              corsRules: corsRules
            }
          }
        }

        resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-06-01' = [
          for blobContainerName in blobContainerNames: {
            name: blobContainerName
            parent: blobService
          }
        ]
        """;

    private const string TablesModuleTemplate = """
        import { CorsRuleDescription } from './types.bicep'

        @description('Storage account name')
        param storageAccountName string

        @description('Table names')
        param tableNames string[]

        @description('CORS rules')
        param corsRules CorsRuleDescription[] = []

        resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
          name: storageAccountName
        }

        resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2025-06-01' = {
          name: 'default'
          parent: storageAccount
          properties: {
            cors: {
              corsRules: corsRules
            }
          }
        }

        resource table 'Microsoft.Storage/storageAccounts/tableServices/tables@2025-06-01' = [
          for tableName in tableNames: {
            name: tableName
            parent: tableService
          }
        ]
        """;
}
