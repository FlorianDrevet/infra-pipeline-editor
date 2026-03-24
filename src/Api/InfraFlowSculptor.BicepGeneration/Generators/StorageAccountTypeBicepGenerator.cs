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
        return new GeneratedTypeModule
        {
            ModuleName = "storageAccount",
            ModuleFileName = "storageAccount.bicep",
            ModuleFolderName = "StorageAccount",
            ModuleBicepContent = StorageAccountModuleTemplate,
            ModuleTypesBicepContent = StorageAccountTypesTemplate,
            ResourceTypeName = ResourceTypeName,
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

    private const string StorageAccountTypesTemplate = """
        @export()
        @description('SKU name for the Storage Account')
        type SkuName = 'Standard_LRS' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS'

        @export()
        @description('Kind of Storage Account')
        type StorageKind = 'StorageV2' | 'BlobStorage' | 'BlockBlobStorage' | 'FileStorage' | 'Storage'

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

        var storagePropertiesBase = {
          allowBlobPublicAccess: allowBlobPublicAccess
          supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
          minimumTlsVersion: minimumTlsVersion
        }

        var storageAccessTierProperties = contains([
          'BlobStorage'
          'StorageV2'
        ], kind) ? {
          accessTier: accessTier
        } : {}

        resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
          name: name
          location: location
          kind: kind
          sku: {
            name: sku
          }
          properties: union(storagePropertiesBase, storageAccessTierProperties)
        }
        """;
}
