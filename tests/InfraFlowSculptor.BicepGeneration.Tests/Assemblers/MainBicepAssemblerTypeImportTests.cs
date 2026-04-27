using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class MainBicepAssemblerTypeImportTests
{
    [Fact]
    public void Given_CollidingModuleTypeNames_When_Generate_Then_AliasesImportsAndParameterTypes()
    {
        // Arrange
        var modules = new[]
        {
            NewModule(
                moduleName: "sqlDatabaseIfs",
                logicalName: "ifs",
                resourceTypeName: AzureResourceTypes.SqlDatabase,
                folder: "SqlDatabase",
                file: "sqlDatabase.module.bicep",
                parameters: new Dictionary<string, object>
                {
                    ["sku"] = "Basic",
                },
                parameterTypeOverrides: new Dictionary<string, string>
                {
                    ["sku"] = "SkuName",
                }),
            NewModule(
                moduleName: "storageAccountIfs",
                logicalName: "ifs",
                resourceTypeName: AzureResourceTypes.StorageAccount,
                folder: "StorageAccount",
                file: "storageAccount.module.bicep",
                parameters: new Dictionary<string, object>
                {
                    ["sku"] = "Standard_LRS",
                    ["kind"] = "StorageV2",
                    ["accessTier"] = "Hot",
                    ["minimumTlsVersion"] = "TLS1_2",
                },
                parameterTypeOverrides: new Dictionary<string, string>
                {
                    ["sku"] = "SkuName",
                    ["kind"] = "StorageKind",
                    ["accessTier"] = "AccessTier",
                    ["minimumTlsVersion"] = "TlsVersion",
                }),
            NewModule(
                moduleName: "keyVaultIfs",
                logicalName: "ifs",
                resourceTypeName: AzureResourceTypes.KeyVault,
                folder: "KeyVault",
                file: "keyVault.module.bicep",
                parameters: new Dictionary<string, object>
                {
                    ["sku"] = "standard",
                },
                parameterTypeOverrides: new Dictionary<string, string>
                {
                    ["sku"] = "SkuName",
                }),
            NewModule(
                moduleName: "sqlServerInfraFlow",
                logicalName: "infra-flow",
                resourceTypeName: AzureResourceTypes.SqlServer,
                folder: "SqlServer",
                file: "sqlServer.module.bicep",
                parameters: new Dictionary<string, object>
                {
                    ["version"] = "12.0",
                    ["minimalTlsVersion"] = "1.2",
                },
                parameterTypeOverrides: new Dictionary<string, string>
                {
                    ["version"] = "SqlServerVersion",
                    ["minimalTlsVersion"] = "TlsVersion",
                }),
        };

        var resourceGroups = new[]
        {
            new ResourceGroupDefinition
            {
                Name = "ifs",
                ResourceAbbreviation = "rg",
            },
        };

        // Act
        var result = MainBicepAssembler.Generate(
            modules,
            resourceGroups,
            new NamingContext(),
            roleAssignments: [],
            appSettings: [],
            existingResourceReferences: []);

        // Assert
        result.Content.Should().Contain("import { SkuName as SqlDatabaseSkuName } from './modules/SqlDatabase/types.bicep'");
        result.Content.Should().Contain("import { SkuName as KeyVaultSkuName } from './modules/KeyVault/types.bicep'");
        result.Content.Should().Contain("import { AccessTier, SkuName as StorageAccountSkuName, StorageKind, TlsVersion as StorageAccountTlsVersion } from './modules/StorageAccount/types.bicep'");
        result.Content.Should().Contain("import { SqlServerVersion, TlsVersion as SqlServerTlsVersion } from './modules/SqlServer/types.bicep'");

        result.Content.Should().Contain("param sqlDatabaseIfsSku SqlDatabaseSkuName");
        result.Content.Should().Contain("param storageAccountIfsSku StorageAccountSkuName");
        result.Content.Should().Contain("param storageAccountIfsMinimumTlsVersion StorageAccountTlsVersion");
        result.Content.Should().Contain("param keyVaultIfsSku KeyVaultSkuName");
        result.Content.Should().Contain("param sqlServerInfraFlowVersion SqlServerVersion");
        result.Content.Should().Contain("param sqlServerInfraFlowMinimalTlsVersion SqlServerTlsVersion");
    }

    private static GeneratedTypeModule NewModule(
        string moduleName,
        string logicalName,
        string resourceTypeName,
        string folder,
        string file,
        IReadOnlyDictionary<string, object> parameters,
        IReadOnlyDictionary<string, string> parameterTypeOverrides)
    {
        return new GeneratedTypeModule
        {
            ModuleName = moduleName,
            ModuleFileName = file,
            ModuleFolderName = folder,
            ResourceGroupName = "ifs",
            LogicalResourceName = logicalName,
            ResourceTypeName = resourceTypeName,
            ResourceAbbreviation = "res",
            Parameters = parameters,
            ParameterTypeOverrides = parameterTypeOverrides,
        };
    }
}