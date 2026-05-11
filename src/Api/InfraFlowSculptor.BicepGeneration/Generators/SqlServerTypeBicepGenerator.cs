using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Server (<c>Microsoft.Sql/servers@2023-08-01-preview</c>).</summary>
public sealed class SqlServerTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string SqlServerModuleName = "sqlServer";
    private const string SqlServerModuleFolderName = "SqlServer";
    private const string SqlServerVersionTypeName = "SqlServerVersion";
    private const string TlsVersionTypeName = "TlsVersion";
    private const string VersionParameterName = "version";
    private const string AdministratorLoginParameterName = "administratorLogin";
    private const string AdministratorLoginPasswordParameterName = "administratorLoginPassword";
    private const string MinimalTlsVersionParameterName = "minimalTlsVersion";
    private const string SqlServerArmType = "Microsoft.Sql/servers@2023-08-01-preview";
    private const string DefaultSqlServerVersion = "12.0";
    private const string LegacySqlServerVersion = "V12";
    private const string DefaultMinimumTlsVersion = "1.2";
    private const string SupportedTlsVersionUnion = "'1.0' | '1.1' | '1.2'";
    private const string PublicNetworkAccessEnabled = "Enabled";
    private const string PublicNetworkAccessPropertyName = "publicNetworkAccess";
    private const string FullyQualifiedDomainNameOutputName = "fullyQualifiedDomainName";
    private const string ResourceIdExpression = SqlServerModuleName + ".id";
    private const string FullyQualifiedDomainNameExpression = SqlServerModuleName + ".properties.fullyQualifiedDomainName";
    private const string SqlServerVersionUnion = "'" + DefaultSqlServerVersion + "'";

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
            .Import(TypesImportPath, SqlServerVersionTypeName, TlsVersionTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the SQL Server")
            .Param(NameParameterName, BicepType.String, "Name of the SQL Server")
            .Param(VersionParameterName, BicepType.Custom(SqlServerVersionTypeName), "SQL Server version",
                defaultValue: new BicepStringLiteral(DefaultSqlServerVersion))
            .Param(AdministratorLoginParameterName, BicepType.String, "Administrator login name")
            .Param(AdministratorLoginPasswordParameterName, BicepType.String, "Administrator login password",
                secure: true)
            .Param(MinimalTlsVersionParameterName, BicepType.Custom(TlsVersionTypeName), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral(DefaultMinimumTlsVersion))
            .Resource(SqlServerModuleName, SqlServerArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(VersionParameterName, new BicepReference(VersionParameterName))
                .Property(AdministratorLoginParameterName, new BicepReference(AdministratorLoginParameterName))
                .Property(AdministratorLoginPasswordParameterName, new BicepReference(AdministratorLoginPasswordParameterName))
                .Property(MinimalTlsVersionParameterName, new BicepReference(MinimalTlsVersionParameterName))
                .Property(PublicNetworkAccessPropertyName, new BicepStringLiteral(PublicNetworkAccessEnabled)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the SQL Server")
            .Output(FullyQualifiedDomainNameOutputName, BicepType.String,
                new BicepRawExpression(FullyQualifiedDomainNameExpression),
                description: "The fully qualified domain name of the SQL Server")
            .ExportedType(SqlServerVersionTypeName,
                new BicepRawExpression(SqlServerVersionUnion),
                description: "SQL Server version")
            .ExportedType(TlsVersionTypeName,
                new BicepRawExpression(SupportedTlsVersionUnion),
                description: "Minimum TLS version for SQL Server connections")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var rawVersion = resource.Properties.GetValueOrDefault(VersionParameterName, AzureResourceDefaults.SqlServerVersion);
        var version = NormalizeSqlServerVersion(rawVersion);

        return new GeneratedTypeModule
        {
            ModuleName = SqlServerModuleName,
            ModuleFileName = SqlServerModuleName,
            ModuleFolderName = SqlServerModuleFolderName,
            ModuleBicepContent = SqlServerModuleTemplate,
            ModuleTypesBicepContent = SqlServerTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new SqlServerParameters
            {
                Version = version,
                AdministratorLogin = resource.Properties.GetValueOrDefault(AdministratorLoginParameterName, AzureResourceDefaults.SqlServerAdministratorLogin),
                MinimalTlsVersion = resource.Properties.GetValueOrDefault(MinimalTlsVersionParameterName, AzureResourceDefaults.MinimumTlsVersion),
            }),
            SecureParameters = [AdministratorLoginPasswordParameterName]
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
        import { {{SqlServerVersionTypeName}}, {{TlsVersionTypeName}} } from '{{TypesImportPath}}'

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
