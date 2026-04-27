using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Cosmos DB (<c>Microsoft.DocumentDB/databaseAccounts@2024-05-15</c>).
/// </summary>
public sealed class CosmosDbTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.CosmosDb;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.CosmosDb;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("cosmosDb", "CosmosDb", ResourceTypeName)
            .Import("./types.bicep", "DatabaseKind", "ConsistencyLevel", "BackupPolicyType")
            .Param("location", BicepType.String, "Azure region for the Cosmos DB account")
            .Param("name", BicepType.String, "Name of the Cosmos DB account")
            .Param("kind", BicepType.Custom("DatabaseKind"), "Kind of Cosmos DB account (API type)",
                defaultValue: new BicepStringLiteral("GlobalDocumentDB"))
            .Param("consistencyLevel", BicepType.Custom("ConsistencyLevel"), "Default consistency level",
                defaultValue: new BicepStringLiteral("Session"))
            .Param("maxStalenessPrefix", BicepType.Int, "Maximum staleness prefix for BoundedStaleness consistency",
                defaultValue: new BicepIntLiteral(100))
            .Param("maxIntervalInSeconds", BicepType.Int, "Maximum interval in seconds for BoundedStaleness consistency",
                defaultValue: new BicepIntLiteral(5))
            .Param("enableAutomaticFailover", BicepType.Bool, "Whether automatic failover is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("enableMultipleWriteLocations", BicepType.Bool, "Whether multiple write locations are enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("backupPolicyType", BicepType.Custom("BackupPolicyType"), "Backup policy type",
                defaultValue: new BicepStringLiteral("Periodic"))
            .Param("enableFreeTier", BicepType.Bool, "Whether the free tier is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("capabilities", BicepType.Array, "Additional capabilities (e.g. EnableServerless)",
                defaultValue: new BicepArrayExpression([]))
            .Resource("cosmosDbAccount", "Microsoft.DocumentDB/databaseAccounts@2024-05-15")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("kind", new BicepReference("kind"))
            .Property("properties", props => props
                .Property("databaseAccountOfferType", "Standard")
                .Property("consistencyPolicy", cp => cp
                    .Property("defaultConsistencyLevel", new BicepReference("consistencyLevel"))
                    .Property("maxStalenessPrefix", new BicepReference("maxStalenessPrefix"))
                    .Property("maxIntervalInSeconds", new BicepReference("maxIntervalInSeconds")))
                .Property("enableAutomaticFailover", new BicepReference("enableAutomaticFailover"))
                .Property("enableMultipleWriteLocations", new BicepReference("enableMultipleWriteLocations"))
                .Property("backupPolicy", bp => bp
                    .Property("type", new BicepReference("backupPolicyType")))
                .Property("enableFreeTier", new BicepReference("enableFreeTier"))
                .Property("capabilities", new BicepReference("capabilities"))
                .Property("locations", new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("locationName", new BicepReference("location")),
                        new BicepPropertyAssignment("failoverPriority", new BicepIntLiteral(0)),
                        new BicepPropertyAssignment("isZoneRedundant", new BicepBoolLiteral(false)),
                    ])
                ])))
            .Output("id", BicepType.String, new BicepRawExpression("cosmosDbAccount.id"),
                description: "The resource ID of the Cosmos DB account")
            .Output("documentEndpoint", BicepType.String,
                new BicepRawExpression("cosmosDbAccount.properties.documentEndpoint"),
                description: "The document endpoint of the Cosmos DB account")
            .Output("name", BicepType.String, new BicepRawExpression("cosmosDbAccount.name"),
                description: "The name of the Cosmos DB account")
            .ExportedType("DatabaseKind",
                new BicepRawExpression("'GlobalDocumentDB' | 'MongoDB' | 'Parse'"),
                description: "Kind of Cosmos DB account (API type)")
            .ExportedType("ConsistencyLevel",
                new BicepRawExpression("'Eventual' | 'Session' | 'BoundedStaleness' | 'Strong' | 'ConsistentPrefix'"),
                description: "Default consistency level for the Cosmos DB account")
            .ExportedType("BackupPolicyType",
                new BicepRawExpression("'Periodic' | 'Continuous'"),
                description: "Backup policy type for the Cosmos DB account")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "cosmosDb",
            ModuleFileName = "cosmosDb",
            ModuleFolderName = "CosmosDb",
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
