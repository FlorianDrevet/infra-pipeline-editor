using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Server (<c>Microsoft.Sql/servers@2023-08-01-preview</c>).</summary>
public sealed class SqlServerTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string SqlServerModuleName = "sqlServer";
    private const string SqlServerModuleFolderName = "SqlServer";
    private const string TypesFilePath = "./types.bicep";
    private const string SqlServerVersionTypeName = "SqlServerVersion";
    private const string TlsVersionTypeName = "TlsVersion";
    private const string VersionParameterName = "version";
    private const string MinimalTlsVersionParameterName = "minimalTlsVersion";
    private const string SqlServerArmType = "Microsoft.Sql/servers@2023-08-01-preview";
    private const string DefaultSqlServerVersion = "12.0";
    private const string LegacySqlServerVersion = "V12";
    private const string DefaultMinimumTlsVersion = "1.2";
    private const string SupportedTlsVersionUnion = "'1.0' | '1.1' | '1.2'";
    private const string PublicNetworkAccessEnabled = "Enabled";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.SqlServer;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.SqlServer;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(SqlServerModuleName, SqlServerModuleFolderName, ResourceTypeName)
            .Import(TypesFilePath, SqlServerVersionTypeName, TlsVersionTypeName)
            .Param("location", BicepType.String, "Azure region for the SQL Server")
            .Param("name", BicepType.String, "Name of the SQL Server")
            .Param(VersionParameterName, BicepType.Custom(SqlServerVersionTypeName), "SQL Server version",
                defaultValue: new BicepStringLiteral(DefaultSqlServerVersion))
            .Param("administratorLogin", BicepType.String, "Administrator login name")
            .Param("administratorLoginPassword", BicepType.String, "Administrator login password",
                secure: true)
            .Param(MinimalTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral(DefaultMinimumTlsVersion))
            .Resource(SqlServerModuleName, SqlServerArmType)
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property(VersionParameterName, new BicepReference(VersionParameterName))
                .Property("administratorLogin", new BicepReference("administratorLogin"))
                .Property("administratorLoginPassword", new BicepReference("administratorLoginPassword"))
                .Property(MinimalTlsVersionParameterName, new BicepReference(MinimalTlsVersionParameterName))
                .Property("publicNetworkAccess", new BicepStringLiteral(PublicNetworkAccessEnabled)))
            .Output("id", BicepType.String, new BicepRawExpression($"{SqlServerModuleName}.id"),
                description: "The resource ID of the SQL Server")
            .Output("fullyQualifiedDomainName", BicepType.String,
                new BicepRawExpression($"{SqlServerModuleName}.properties.fullyQualifiedDomainName"),
                description: "The fully qualified domain name of the SQL Server")
            .ExportedType(SqlServerVersionTypeName,
                new BicepRawExpression($"'{DefaultSqlServerVersion}'"),
                description: "SQL Server version")
            .ExportedType(TlsVersionTypeName,
                new BicepRawExpression(SupportedTlsVersionUnion),
                description: "Minimum TLS version for SQL Server connections")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var rawVersion = resource.Properties.GetValueOrDefault("version", AzureResourceDefaults.SqlServerVersion);
        var version = NormalizeSqlServerVersion(rawVersion);

        return new GeneratedTypeModule
        {
            ModuleName = SqlServerModuleName,
            ModuleFileName = SqlServerModuleName,
            ModuleFolderName = SqlServerModuleFolderName,
            ModuleBicepContent = SqlServerModuleTemplate,
            ModuleTypesBicepContent = SqlServerTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                [VersionParameterName] = version,
                ["administratorLogin"] = resource.Properties.GetValueOrDefault("administratorLogin", AzureResourceDefaults.SqlServerAdministratorLogin),
                [MinimalTlsVersionParameterName] = resource.Properties.GetValueOrDefault(MinimalTlsVersionParameterName, AzureResourceDefaults.MinimumTlsVersion),
            },
            SecureParameters = ["administratorLoginPassword"]
        };
    }

    /// <summary>
    /// Normalizes legacy SQL Server version identifiers (e.g. <c>V12</c>) to the ARM-accepted format (<c>12.0</c>).
    /// </summary>
    private static string NormalizeSqlServerVersion(string version) => version.ToUpperInvariant() switch
    {
        LegacySqlServerVersion => DefaultSqlServerVersion,
        _ => version
    };

    private static readonly string SqlServerTypesTemplate = $$"""
        @export()
        @description('SQL Server version')
        type {{SqlServerVersionTypeName}} = '{{DefaultSqlServerVersion}}'

        @export()
        @description('Minimum TLS version for SQL Server connections')
        type {{TlsVersionTypeName}} = '1.0' | '1.1' | '{{DefaultMinimumTlsVersion}}'
        """;

    private static readonly string SqlServerModuleTemplate = $$"""
        import { {{SqlServerVersionTypeName}}, {{TlsVersionTypeName}} } from '{{TypesFilePath}}'

        @description('Azure region for the SQL Server')
        param location string

        @description('Name of the SQL Server')
        param name string

        @description('SQL Server version')
        param {{VersionParameterName}} {{SqlServerVersionTypeName}} = '{{DefaultSqlServerVersion}}'

        @description('Administrator login name')
        param administratorLogin string

        @secure()
        @description('Administrator login password')
        param administratorLoginPassword string

        @description('Minimum TLS version for client connections')
        param {{MinimalTlsVersionParameterName}} {{TlsVersionTypeName}} = '{{DefaultMinimumTlsVersion}}'

        resource {{SqlServerModuleName}} '{{SqlServerArmType}}' = {
          name: name
          location: location
          properties: {
            {{VersionParameterName}}: {{VersionParameterName}}
            administratorLogin: administratorLogin
            administratorLoginPassword: administratorLoginPassword
            {{MinimalTlsVersionParameterName}}: {{MinimalTlsVersionParameterName}}
            publicNetworkAccess: '{{PublicNetworkAccessEnabled}}'
          }
        }

        output id string = {{SqlServerModuleName}}.id
        output fullyQualifiedDomainName string = {{SqlServerModuleName}}.properties.fullyQualifiedDomainName
        """;
}
