using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Database (<c>Microsoft.Sql/servers/databases@2023-08-01-preview</c>).</summary>
public sealed class SqlDatabaseTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "sqlDatabase";
    private const string ModuleFolderName = "SqlDatabase";
    private const string SkuNameTypeName = "SkuName";
    private const string SqlServerNameParameterName = "sqlServerName";
    private const string SkuParameterName = "sku";
    private const string MaxSizeBytesParameterName = "maxSizeBytes";
    private const string MaxSizeGbPropertyName = "maxSizeGb";
    private const string CollationParameterName = "collation";
    private const string ZoneRedundantParameterName = "zoneRedundant";
    private const string SqlServerResourceName = "sqlServer";
    private const string SqlDatabaseResourceName = "sqlDatabase";
    private const string SqlServerArmType = "Microsoft.Sql/servers@2023-08-01-preview";
    private const string SqlDatabaseArmType = "Microsoft.Sql/servers/databases@2023-08-01-preview";
    private const string DefaultSkuName = "Basic";
    private const string DefaultMaxSizeGb = "2";
    private const string DefaultCollation = "SQL_Latin1_General_CP1_CI_AS";
    private const string SkuNameUnion = "'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'";
    private const string ResourceIdExpression = SqlDatabaseResourceName + ".id";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.SqlDatabaseType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.SqlDatabase;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the SQL Database")
            .Param(NameParameterName, BicepType.String, "Name of the SQL Database")
            .Param(SqlServerNameParameterName, BicepType.String, "Name of the parent SQL Server")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU of the SQL Database",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(MaxSizeBytesParameterName, BicepType.Int, "Maximum size of the database in bytes")
            .Param(CollationParameterName, BicepType.String, "Collation of the database")
            .Param(ZoneRedundantParameterName, BicepType.Bool, "Whether the database is zone redundant")
            .ExistingResource(SqlServerResourceName, SqlServerArmType, SqlServerNameParameterName)
            .Resource(SqlDatabaseResourceName, SqlDatabaseArmType)
            .Parent(SqlServerResourceName)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(CollationParameterName, new BicepReference(CollationParameterName))
                .Property(MaxSizeBytesParameterName, new BicepReference(MaxSizeBytesParameterName))
                .Property(ZoneRedundantParameterName, new BicepReference(ZoneRedundantParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the SQL Database")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the SQL Database")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var sku = resource.Properties.GetValueOrDefault(SkuParameterName, DefaultSkuName);
        var maxSizeGb = int.TryParse(resource.Properties.GetValueOrDefault(MaxSizeGbPropertyName, DefaultMaxSizeGb), out var sz) ? sz : 2;

        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = SqlDatabaseModuleTemplate,
            ModuleTypesBicepContent = SqlDatabaseTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new SqlDatabaseParameters
            {
                Sku = sku,
                MaxSizeBytes = (long)maxSizeGb * 1024 * 1024 * 1024,
                Collation = resource.Properties.GetValueOrDefault(CollationParameterName, DefaultCollation),
                ZoneRedundant = resource.Properties.GetValueOrDefault(ZoneRedundantParameterName, BooleanFalseString) == BooleanTrueString,
            })
        };
    }

    private const string SqlDatabaseTypesTemplate = """
        @export()
        @description('SKU name for the SQL Database')
        type SkuName = 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'
        """;

    private const string SqlDatabaseModuleTemplate = """
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

        resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
          name: sqlServerName
        }

        resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
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
