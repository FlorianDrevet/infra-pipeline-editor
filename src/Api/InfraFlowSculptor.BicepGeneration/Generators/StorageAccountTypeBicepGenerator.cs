using System.Text.Json;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class StorageAccountTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
  private const string StorageAccountModuleName = "storageAccount";
  private const string StorageAccountModuleFolderName = "StorageAccount";
  private const string StorageResourceSymbol = "storage";
  private const string StorageAccountArmType = "Microsoft.Storage/storageAccounts@2025-06-01";
  private const string TypesFilePath = "./types.bicep";
  private const string SkuTypeName = "SkuName";
  private const string StorageKindTypeName = "StorageKind";
  private const string AccessTierTypeName = "AccessTier";
  private const string TlsVersionTypeName = "TlsVersion";
  private const string SkuParameterName = "sku";
  private const string KindParameterName = "kind";
  private const string AccessTierParameterName = "accessTier";
  private const string MinimumTlsVersionParameterName = "minimumTlsVersion";
  private const string DefaultSkuName = "Standard_LRS";
  private const string DefaultStorageKind = "StorageV2";
  private const string DefaultAccessTier = "Hot";
  private const string DefaultMinimumTlsVersion = "TLS1_2";
  private const string SystemAssignedIdentityType = "SystemAssigned";
  private const string BooleanTrueString = "true";
  private const string BooleanFalseString = "false";
  private const string BlobContainerNamesPropertyName = "blobContainerNames";
  private const string StorageTableNamesPropertyName = "storageTableNames";
  private const string QueueNamesPropertyName = "queueNames";
  private const string CorsRulesPropertyName = "corsRules";
  private const string TableCorsRulesPropertyName = "tableCorsRules";
  private const string LifecycleRulesPropertyName = "lifecycleRules";

    public string ResourceType
        => AzureResourceTypes.ArmTypes.StorageAccount;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.StorageAccount;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        var builder = new BicepModuleBuilder()
        .Module(StorageAccountModuleName, StorageAccountModuleFolderName, ResourceTypeName)
        .Import(TypesFilePath, SkuTypeName, StorageKindTypeName, AccessTierTypeName, TlsVersionTypeName)
            .Param("location", BicepType.String, "Azure region for the Storage Account")
            .Param("name", BicepType.String, "Name of the Storage Account")
        .Param(SkuParameterName, BicepType.Custom(SkuTypeName), "SKU of the Storage Account",
          defaultValue: new BicepStringLiteral(DefaultSkuName))
        .Param(KindParameterName, BicepType.Custom(StorageKindTypeName), "Kind of Storage Account",
          defaultValue: new BicepStringLiteral(DefaultStorageKind))
        .Param(AccessTierParameterName, BicepType.Custom(AccessTierTypeName), "Access tier for blob storage",
          defaultValue: new BicepStringLiteral(DefaultAccessTier))
            .Param("allowBlobPublicAccess", BicepType.Bool, "Whether public access to blobs is allowed")
            .Param("supportsHttpsTrafficOnly", BicepType.Bool, "Whether HTTPS traffic only is enforced")
        .Param(MinimumTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version for client connections",
          defaultValue: new BicepStringLiteral(DefaultMinimumTlsVersion));

      builder.Resource(StorageResourceSymbol, StorageAccountArmType)
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
        .Property(KindParameterName, new BicepReference(KindParameterName))
            .Property("sku", sku => sku
          .Property("name", new BicepReference(SkuParameterName)))
            .Property("identity", identity => identity
          .Property("type", new BicepStringLiteral(SystemAssignedIdentityType)))
            .Property("properties", props => props
                .Property("allowBlobPublicAccess", new BicepReference("allowBlobPublicAccess"))
                .Property("supportsHttpsTrafficOnly", new BicepReference("supportsHttpsTrafficOnly"))
          .Property(MinimumTlsVersionParameterName, new BicepReference(MinimumTlsVersionParameterName))
          .Property(AccessTierParameterName, new BicepReference(AccessTierParameterName)));

        builder
        .Output("id", BicepType.String, new BicepRawExpression($"{StorageResourceSymbol}.id"),
                description: "The resource ID of the Storage Account")
        .Output("name", BicepType.String, new BicepRawExpression($"{StorageResourceSymbol}.name"),
                description: "The name of the Storage Account")
            .Output("primaryBlobEndpoint", BicepType.String,
          new BicepRawExpression($"{StorageResourceSymbol}.properties.primaryEndpoints.blob"),
                description: "The primary blob endpoint")
            .Output("primaryTableEndpoint", BicepType.String,
          new BicepRawExpression($"{StorageResourceSymbol}.properties.primaryEndpoints.table"),
                description: "The primary table endpoint")
            .Output("primaryQueueEndpoint", BicepType.String,
          new BicepRawExpression($"{StorageResourceSymbol}.properties.primaryEndpoints.queue"),
                description: "The primary queue endpoint")
            .Output("primaryFileEndpoint", BicepType.String,
          new BicepRawExpression($"{StorageResourceSymbol}.properties.primaryEndpoints.file"),
                description: "The primary file endpoint");

        builder
        .ExportedType(SkuTypeName,
          new BicepRawExpression($"'{DefaultSkuName}' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS'"),
                description: "SKU name for the Storage Account")
        .ExportedType(StorageKindTypeName,
          new BicepRawExpression($"'BlobStorage' | 'BlockBlobStorage' | 'FileStorage' | 'Storage' | '{DefaultStorageKind}'"),
                description: "Kind of Storage Account")
        .ExportedType(AccessTierTypeName,
          new BicepRawExpression($"'{DefaultAccessTier}' | 'Cool' | 'Premium'"),
                description: "Access tier for the Storage Account")
        .ExportedType(TlsVersionTypeName,
          new BicepRawExpression($"'TLS1_0' | 'TLS1_1' | '{DefaultMinimumTlsVersion}'"),
                description: "Minimum TLS version for Storage Account connections");

        return builder.Build();
    }

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var blobContainerNames = ParseBlobContainerNames(resource.Properties);
        var queueNames = ParseQueueNames(resource.Properties);
        var storageTableNames = ParseStorageTableNames(resource.Properties);
        var corsRules = ParseCorsRules(resource.Properties);
        var tableCorsRules = ParseTableCorsRules(resource.Properties);
        var lifecycleRules = ParseLifecycleRules(resource.Properties);

        var companions = new List<GeneratedCompanionModule>();

        if (blobContainerNames.Count > 0 || corsRules.Count > 0 || lifecycleRules.Count > 0)
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
                CorsRules = corsRules,
                LifecycleRules = lifecycleRules
            });
        }

        if (queueNames.Count > 0)
        {
            companions.Add(new GeneratedCompanionModule
            {
                ModuleSymbolSuffix = "Queues",
                DeploymentNameSuffix = "Queues",
                FileName = "storage.queues.module.bicep",
                FolderName = "StorageAccount",
                BicepContent = QueuesModuleTemplate,
                QueueNames = queueNames
            });
        }

        if (storageTableNames.Count > 0 || tableCorsRules.Count > 0)
        {
            companions.Add(new GeneratedCompanionModule
            {
                ModuleSymbolSuffix = "Tables",
                DeploymentNameSuffix = "Tables",
                FileName = "storage.table.module.bicep",
                FolderName = "StorageAccount",
                BicepContent = TablesModuleTemplate,
                TypesBicepContent = BlobsTypesTemplate,
                StorageTableNames = storageTableNames,
                TableCorsRules = tableCorsRules
            });
        }

        return new GeneratedTypeModule
        {
          ModuleName = StorageAccountModuleName,
          ModuleFileName = StorageAccountModuleName,
          ModuleFolderName = StorageAccountModuleFolderName,
            ModuleBicepContent = StorageAccountModuleTemplate,
            ModuleTypesBicepContent = StorageAccountTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            CompanionModules = companions,
            Parameters = new Dictionary<string, object>
            {
            [SkuParameterName] = resource.Properties.GetValueOrDefault(SkuParameterName, DefaultSkuName),
            [KindParameterName] = resource.Properties.GetValueOrDefault(KindParameterName, DefaultStorageKind),
            [AccessTierParameterName] = resource.Properties.GetValueOrDefault(AccessTierParameterName, DefaultAccessTier),
            ["allowBlobPublicAccess"] = resource.Properties.GetValueOrDefault("allowBlobPublicAccess", BooleanFalseString) == BooleanTrueString,
            ["supportsHttpsTrafficOnly"] = resource.Properties.GetValueOrDefault("supportsHttpsTrafficOnly", BooleanTrueString) == BooleanTrueString,
            [MinimumTlsVersionParameterName] = resource.Properties.GetValueOrDefault(MinimumTlsVersionParameterName, DefaultMinimumTlsVersion),
            }
        };
    }

    private static List<string> ParseBlobContainerNames(IReadOnlyDictionary<string, string> properties)
    {
        if (!properties.TryGetValue(BlobContainerNamesPropertyName, out var json) || string.IsNullOrEmpty(json))
            return [];

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static List<string> ParseStorageTableNames(IReadOnlyDictionary<string, string> properties)
    {
        if (!properties.TryGetValue(StorageTableNamesPropertyName, out var json) || string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static List<string> ParseQueueNames(IReadOnlyDictionary<string, string> properties)
    {
      if (!properties.TryGetValue(QueueNamesPropertyName, out var json) || string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static List<BlobCorsRuleData> ParseCorsRules(IReadOnlyDictionary<string, string> properties)
    {
      if (!properties.TryGetValue(CorsRulesPropertyName, out var json) || string.IsNullOrEmpty(json))
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

        private static List<BlobCorsRuleData> ParseTableCorsRules(IReadOnlyDictionary<string, string> properties)
        {
          if (!properties.TryGetValue(TableCorsRulesPropertyName, out var json) || string.IsNullOrEmpty(json))
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

    private static List<ContainerLifecycleRuleData> ParseLifecycleRules(IReadOnlyDictionary<string, string> properties)
    {
      if (!properties.TryGetValue(LifecycleRulesPropertyName, out var json) || string.IsNullOrEmpty(json))
            return [];

        var raw = JsonSerializer.Deserialize<List<LifecycleRuleJson>>(json);
        if (raw is null) return [];

        return raw.Select(r => new ContainerLifecycleRuleData(
            r.ruleName ?? string.Empty,
            r.containerNames ?? [],
            r.timeToLiveInDays))
            .ToList();
    }

    private sealed record LifecycleRuleJson(
        string? ruleName,
        List<string>? containerNames,
        int timeToLiveInDays);


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
        import { {{SkuTypeName}}, {{StorageKindTypeName}}, {{AccessTierTypeName}}, {{TlsVersionTypeName}} } from '{{TypesFilePath}}'

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
