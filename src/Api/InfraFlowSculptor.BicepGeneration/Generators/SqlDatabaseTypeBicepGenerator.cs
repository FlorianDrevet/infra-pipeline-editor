using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Database (<c>Microsoft.Sql/servers/databases@2023-08-01-preview</c>).</summary>
public sealed class SqlDatabaseTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.SqlDatabase;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.SqlDatabase;

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var sku = resource.Properties.GetValueOrDefault("sku", "Basic");
        var maxSizeGb = int.TryParse(resource.Properties.GetValueOrDefault("maxSizeGb", "2"), out var sz) ? sz : 2;

        return new GeneratedTypeModule
        {
            ModuleName = "sqlDatabase",
            ModuleFileName = "sqlDatabase",
            ModuleFolderName = "SqlDatabase",
            ModuleBicepContent = SqlDatabaseModuleTemplate,
            ModuleTypesBicepContent = SqlDatabaseTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = sku,
                ["maxSizeBytes"] = (long)maxSizeGb * 1024 * 1024 * 1024,
                ["collation"] = resource.Properties.GetValueOrDefault("collation", "SQL_Latin1_General_CP1_CI_AS"),
                ["zoneRedundant"] = resource.Properties.GetValueOrDefault("zoneRedundant", "false") == "true",
            }
        };
    }

    private const string SqlDatabaseTypesTemplate = """
        @export()
        @description('SKU name for the SQL Database')
        type SkuName = 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'
        """;

    private static readonly string SqlDatabaseModuleTemplate = $$"""
        import { SkuName } from './types.bicep'

        @description('Azure region for the SQL Database')
        param location string

        @description('Name of the SQL Database')
        param name string

        @description('Name of the parent SQL Server')
        param sqlServerName string

        @description('SKU of the SQL Database')
        param sku SkuName = 'Basic'

        @description('Maximum size of the database in bytes')
        param maxSizeBytes int

        @description('Collation of the database')
        param collation string

        @description('Whether the database is zone redundant')
        param zoneRedundant bool

        resource sqlServer 'Microsoft.Sql/servers@{{AzureResourceTypes.ApiVersions.SqlServer}}' existing = {
          name: sqlServerName
        }

        resource sqlDatabase 'Microsoft.Sql/servers/databases@{{AzureResourceTypes.ApiVersions.SqlDatabase}}' = {
          parent: sqlServer
          name: name
          location: location
          sku: {
            name: sku
          }
          properties: {
            collation: collation
            maxSizeBytes: maxSizeBytes
            zoneRedundant: zoneRedundant
          }
        }

        output id string = sqlDatabase.id
        """;
}
