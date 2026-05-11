using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Cosmos DB (<c>Microsoft.DocumentDB/databaseAccounts@2024-05-15</c>).
/// </summary>
public sealed class CosmosDbTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "cosmosDb";
    private const string ModuleFolderName = "CosmosDb";
    private const string ModuleFileName = "cosmosDb";
    private const string DatabaseKindTypeName = "DatabaseKind";
    private const string ConsistencyLevelTypeName = "ConsistencyLevel";
    private const string BackupPolicyTypeName = "BackupPolicyType";
    private const string KindParameterName = "kind";
    private const string ConsistencyLevelParameterName = "consistencyLevel";
    private const string MaxStalenessPrefixParameterName = "maxStalenessPrefix";
    private const string MaxIntervalInSecondsParameterName = "maxIntervalInSeconds";
    private const string EnableAutomaticFailoverParameterName = "enableAutomaticFailover";
    private const string EnableMultipleWriteLocationsParameterName = "enableMultipleWriteLocations";
    private const string BackupPolicyTypeParameterName = "backupPolicyType";
    private const string EnableFreeTierParameterName = "enableFreeTier";
    private const string CapabilitiesParameterName = "capabilities";
    private const string ResourceSymbol = "cosmosDbAccount";
    private const string CosmosDbArmType = "Microsoft.DocumentDB/databaseAccounts@2024-05-15";
    private const string DatabaseAccountOfferTypePropertyName = "databaseAccountOfferType";
    private const string StandardOfferValue = "Standard";
    private const string ConsistencyPolicyPropertyName = "consistencyPolicy";
    private const string DefaultConsistencyLevelPropertyName = "defaultConsistencyLevel";
    private const string BackupPolicyPropertyName = "backupPolicy";
    private const string TypePropertyName = "type";
    private const string LocationsPropertyName = "locations";
    private const string LocationNamePropertyName = "locationName";
    private const string FailoverPriorityPropertyName = "failoverPriority";
    private const string IsZoneRedundantPropertyName = "isZoneRedundant";
    private const string DocumentEndpointOutputName = "documentEndpoint";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string DocumentEndpointExpression = ResourceSymbol + ".properties.documentEndpoint";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string DefaultDatabaseKind = "GlobalDocumentDB";
    private const string DefaultConsistencyLevel = "Session";
    private const string DefaultBackupPolicyType = "Periodic";
    private const string DatabaseKindUnion = "'GlobalDocumentDB' | 'MongoDB' | 'Parse'";
    private const string ConsistencyLevelUnion = "'Eventual' | 'Session' | 'BoundedStaleness' | 'Strong' | 'ConsistentPrefix'";
    private const string BackupPolicyTypeUnion = "'Periodic' | 'Continuous'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.CosmosDb;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.CosmosDb;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, DatabaseKindTypeName, ConsistencyLevelTypeName, BackupPolicyTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Cosmos DB account")
            .Param(NameParameterName, BicepType.String, "Name of the Cosmos DB account")
            .Param(KindParameterName, BicepType.Custom(DatabaseKindTypeName), "Kind of Cosmos DB account (API type)",
                defaultValue: new BicepStringLiteral(DefaultDatabaseKind))
            .Param(ConsistencyLevelParameterName, BicepType.Custom(ConsistencyLevelTypeName), "Default consistency level",
                defaultValue: new BicepStringLiteral(DefaultConsistencyLevel))
            .Param(MaxStalenessPrefixParameterName, BicepType.Int, "Maximum staleness prefix for BoundedStaleness consistency",
                defaultValue: new BicepIntLiteral(100))
            .Param(MaxIntervalInSecondsParameterName, BicepType.Int, "Maximum interval in seconds for BoundedStaleness consistency",
                defaultValue: new BicepIntLiteral(5))
            .Param(EnableAutomaticFailoverParameterName, BicepType.Bool, "Whether automatic failover is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(EnableMultipleWriteLocationsParameterName, BicepType.Bool, "Whether multiple write locations are enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(BackupPolicyTypeParameterName, BicepType.Custom(BackupPolicyTypeName), "Backup policy type",
                defaultValue: new BicepStringLiteral(DefaultBackupPolicyType))
            .Param(EnableFreeTierParameterName, BicepType.Bool, "Whether the free tier is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(CapabilitiesParameterName, BicepType.Array, "Additional capabilities (e.g. EnableServerless)",
                defaultValue: new BicepArrayExpression([]))
            .Resource(ResourceSymbol, CosmosDbArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(KindPropertyName, new BicepReference(KindParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(DatabaseAccountOfferTypePropertyName, StandardOfferValue)
                .Property(ConsistencyPolicyPropertyName, cp => cp
                    .Property(DefaultConsistencyLevelPropertyName, new BicepReference(ConsistencyLevelParameterName))
                    .Property(MaxStalenessPrefixParameterName, new BicepReference(MaxStalenessPrefixParameterName))
                    .Property(MaxIntervalInSecondsParameterName, new BicepReference(MaxIntervalInSecondsParameterName)))
                .Property(EnableAutomaticFailoverParameterName, new BicepReference(EnableAutomaticFailoverParameterName))
                .Property(EnableMultipleWriteLocationsParameterName, new BicepReference(EnableMultipleWriteLocationsParameterName))
                .Property(BackupPolicyPropertyName, bp => bp
                    .Property(TypePropertyName, new BicepReference(BackupPolicyTypeParameterName)))
                .Property(EnableFreeTierParameterName, new BicepReference(EnableFreeTierParameterName))
                .Property(CapabilitiesParameterName, new BicepReference(CapabilitiesParameterName))
                .Property(LocationsPropertyName, new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(LocationNamePropertyName, new BicepReference(LocationParameterName)),
                        new BicepPropertyAssignment(FailoverPriorityPropertyName, new BicepIntLiteral(0)),
                        new BicepPropertyAssignment(IsZoneRedundantPropertyName, new BicepBoolLiteral(false)),
                    ])
                ])))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Cosmos DB account")
            .Output(DocumentEndpointOutputName, BicepType.String,
                new BicepRawExpression(DocumentEndpointExpression),
                description: "The document endpoint of the Cosmos DB account")
            .Output(NameParameterName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Cosmos DB account")
            .ExportedType(DatabaseKindTypeName,
                new BicepRawExpression(DatabaseKindUnion),
                description: "Kind of Cosmos DB account (API type)")
            .ExportedType(ConsistencyLevelTypeName,
                new BicepRawExpression(ConsistencyLevelUnion),
                description: "Default consistency level for the Cosmos DB account")
            .ExportedType(BackupPolicyTypeName,
                new BicepRawExpression(BackupPolicyTypeUnion),
                description: "Backup policy type for the Cosmos DB account")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleFileName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = CosmosDbModuleTemplate,
            ModuleTypesBicepContent = CosmosDbTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string CosmosDbTypesTemplate = """
        @export()
        @description('Kind of Cosmos DB account (API type)')
        type DatabaseKind = 'GlobalDocumentDB' | 'MongoDB' | 'Parse'

        @export()
        @description('Default consistency level for the Cosmos DB account')
        type ConsistencyLevel = 'Eventual' | 'Session' | 'BoundedStaleness' | 'Strong' | 'ConsistentPrefix'

        @export()
        @description('Backup policy type for the Cosmos DB account')
        type BackupPolicyType = 'Periodic' | 'Continuous'
        """;

    private const string CosmosDbModuleTemplate = """
        import { DatabaseKind, ConsistencyLevel, BackupPolicyType } from './types.bicep'

        @description('Azure region for the Cosmos DB account')
        param location string

        @description('Name of the Cosmos DB account')
        param name string

        @description('Kind of Cosmos DB account (API type)')
        param kind DatabaseKind = 'GlobalDocumentDB'

        @description('Default consistency level')
        param consistencyLevel ConsistencyLevel = 'Session'

        @description('Maximum staleness prefix for BoundedStaleness consistency')
        param maxStalenessPrefix int = 100

        @description('Maximum interval in seconds for BoundedStaleness consistency')
        param maxIntervalInSeconds int = 5

        @description('Whether automatic failover is enabled')
        param enableAutomaticFailover bool = false

        @description('Whether multiple write locations are enabled')
        param enableMultipleWriteLocations bool = false

        @description('Backup policy type')
        param backupPolicyType BackupPolicyType = 'Periodic'

        @description('Whether the free tier is enabled')
        param enableFreeTier bool = false

        @description('Additional capabilities (e.g. EnableServerless)')
        param capabilities array = []

        resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
          name: name
          location: location
          kind: kind
          properties: {
            databaseAccountOfferType: 'Standard'
            consistencyPolicy: {
              defaultConsistencyLevel: consistencyLevel
              maxStalenessPrefix: maxStalenessPrefix
              maxIntervalInSeconds: maxIntervalInSeconds
            }
            enableAutomaticFailover: enableAutomaticFailover
            enableMultipleWriteLocations: enableMultipleWriteLocations
            backupPolicy: {
              type: backupPolicyType
            }
            enableFreeTier: enableFreeTier
            capabilities: capabilities
            locations: [
              {
                locationName: location
                failoverPriority: 0
                isZoneRedundant: false
              }
            ]
          }
        }

        @description('The resource ID of the Cosmos DB account')
        output id string = cosmosDbAccount.id

        @description('The document endpoint of the Cosmos DB account')
        output documentEndpoint string = cosmosDbAccount.properties.documentEndpoint

        @description('The name of the Cosmos DB account')
        output name string = cosmosDbAccount.name
        """;
}
