namespace InfraFlowSculptor.BicepGeneration.Generators;

using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

public sealed partial class StorageAccountTypeBicepGenerator
{
    private static readonly string StorageAccountTypesTemplate = $$"""
        @export()
        @description('SKU name for the Storage Account')
        type {{SkuTypeName}} = '{{DefaultSkuName}}' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS'

        @export()
        @description('Kind of Storage Account')
        type {{StorageKindTypeName}} = 'BlobStorage' | 'BlockBlobStorage' | 'FileStorage' | 'Storage' | '{{DefaultStorageKind}}'

        @export()
        @description('Access tier for the Storage Account')
        type {{AccessTierTypeName}} = '{{DefaultAccessTier}}' | 'Cool' | 'Premium'

        @export()
        @description('Minimum TLS version for Storage Account connections')
        type {{TlsVersionTypeName}} = 'TLS1_0' | 'TLS1_1' | '{{DefaultMinimumTlsVersion}}'
        """;

    private static readonly string StorageAccountModuleTemplate = $$"""
        import { {{SkuTypeName}}, {{StorageKindTypeName}}, {{AccessTierTypeName}}, {{TlsVersionTypeName}} } from '{{TypesImportPath}}'

        @description('Azure region for the Storage Account')
        param location string

        @description('Name of the Storage Account')
        param name string

        @description('SKU of the Storage Account')
        param {{SkuParameterName}} {{SkuTypeName}} = '{{DefaultSkuName}}'

        @description('Kind of Storage Account')
        param {{KindParameterName}} {{StorageKindTypeName}} = '{{DefaultStorageKind}}'

        @description('Access tier for blob storage')
        param {{AccessTierParameterName}} {{AccessTierTypeName}} = '{{DefaultAccessTier}}'

        @description('Whether public access to blobs is allowed')
        param allowBlobPublicAccess bool

        @description('Whether HTTPS traffic only is enforced')
        param supportsHttpsTrafficOnly bool

        @description('Minimum TLS version for client connections')
        param {{MinimumTlsVersionParameterName}} {{TlsVersionTypeName}} = '{{DefaultMinimumTlsVersion}}'

        resource {{StorageResourceSymbol}} '{{StorageAccountArmType}}' = {
          name: name
          location: location
          {{KindParameterName}}: {{KindParameterName}}
          sku: {
            name: {{SkuParameterName}}
          }
          identity: {
            type: '{{SystemAssignedIdentityType}}'
          }
          properties: {
            allowBlobPublicAccess: allowBlobPublicAccess
            supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
            {{MinimumTlsVersionParameterName}}: {{MinimumTlsVersionParameterName}}
            {{AccessTierParameterName}}: {{AccessTierParameterName}}
          }
        }

        @description('The resource ID of the Storage Account')
        output id string = {{StorageResourceSymbol}}.id

        @description('The name of the Storage Account')
        output name string = {{StorageResourceSymbol}}.name

        @description('The connection string of the Storage Account')
        output {{ConnectionStringOutputName}} string = {{ConnectionStringExpression}}

        @description('The primary blob endpoint')
        output primaryBlobEndpoint string = {{StorageResourceSymbol}}.properties.primaryEndpoints.blob

        @description('The primary table endpoint')
        output primaryTableEndpoint string = {{StorageResourceSymbol}}.properties.primaryEndpoints.table

        @description('The primary queue endpoint')
        output primaryQueueEndpoint string = {{StorageResourceSymbol}}.properties.primaryEndpoints.queue

        @description('The primary file endpoint')
        output primaryFileEndpoint string = {{StorageResourceSymbol}}.properties.primaryEndpoints.file
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

        @export()
        @description('Describes a lifecycle rule for automatic blob deletion by TTL')
        type ContainerLifecycleRule = {
          @description('Name of the lifecycle rule')
          ruleName: string
          @description('Blob container names this rule applies to')
          containerNames: string[]
          @description('Number of days after which blobs are deleted')
          timeToLiveInDays: int
        }
        """;

    private const string BlobsModuleTemplate = """
        import { CorsRuleDescription, ContainerLifecycleRule } from './types.bicep'

        @description('Storage account name')
        param storageAccountName string

        @description('Blob containers names')
        param blobContainerNames string[]

        @description('CORS rules')
        param corsRules CorsRuleDescription[] = []

        @description('Blob lifecycle management rules')
        param containerLifecycleRules ContainerLifecycleRule[] = []

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

        var lifecyclePolicyRules = [
          for rule in containerLifecycleRules: {
            enabled: true
            name: rule.ruleName
            type: 'Lifecycle'
            definition: {
              actions: {
                baseBlob: {
                  delete: {
                    daysAfterModificationGreaterThan: rule.timeToLiveInDays
                  }
                }
              }
              filters: {
                blobTypes: ['blockBlob']
                prefixMatch: map(rule.containerNames, cn => '${cn}/')
              }
            }
          }
        ]

        resource managementPolicies 'Microsoft.Storage/storageAccounts/managementPolicies@2025-06-01' = if (!empty(containerLifecycleRules)) {
          name: 'default'
          parent: storageAccount
          properties: {
            policy: {
              rules: lifecyclePolicyRules
            }
          }
        }
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

    private const string QueuesModuleTemplate = """
        @description('Storage account name')
        param storageAccountName string

        @description('Queue names')
        param queueNames string[]

        resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
          name: storageAccountName
        }

        resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2025-06-01' = {
          name: 'default'
          parent: storageAccount
        }

        resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2025-06-01' = [
          for queueName in queueNames: {
            name: queueName
            parent: queueService
          }
        ]
        """;
}
