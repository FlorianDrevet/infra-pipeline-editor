using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Cosmos DB (<c>Microsoft.DocumentDB/databaseAccounts@2024-05-15</c>).
/// </summary>
public sealed class CosmosDbTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.CosmosDb;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.CosmosDb;

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

    private static readonly string CosmosDbModuleTemplate = $$"""
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

        resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@{{AzureResourceTypes.ApiVersions.CosmosDb}}' = {
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
