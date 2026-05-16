using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class MainBicepAssemblerParameterMetadataTests
{
    [Fact]
    public void Given_ResourceAndAppSettingParameters_When_Generate_Then_EmitsDescriptionsAndGroupedEnvironmentVariableComments()
    {
        // Arrange
        var modules = new[]
        {
            NewModule(
                moduleName: "webAppApiService",
                logicalName: "api-service",
                resourceTypeName: AzureResourceTypes.WebApp,
                folder: "WebApp",
                file: "webApp.module.bicep",
                parameters: new Dictionary<string, object>
                {
                    ["sku"] = "B1",
                },
                secureParameters: ["adminPassword"]),
        };

        var resourceGroups = new[]
        {
            new ResourceGroupDefinition
            {
                Name = "apps",
                ResourceAbbreviation = "rg",
            },
        };

        var appSettings = new[]
        {
            new AppSettingDefinition
            {
                TargetResourceName = "api-service",
                Name = "ConnectionString",
                EnvironmentValues = new Dictionary<string, string>
                {
                    ["dev"] = "Server=tcp:api-service.database.windows.net",
                },
            },
            new AppSettingDefinition
            {
                TargetResourceName = "api-service",
                Name = "API_KEY",
                EnvironmentValues = new Dictionary<string, string>
                {
                    ["dev"] = "super-secret",
                },
            },
            new AppSettingDefinition
            {
                TargetResourceName = "worker-service",
                Name = "BASE_URL",
                EnvironmentValues = new Dictionary<string, string>
                {
                    ["dev"] = "https://worker-service.internal",
                },
            },
            new AppSettingDefinition
            {
                TargetResourceName = "api-service",
                Name = "DbPassword",
                IsKeyVaultReference = true,
                SecretName = "db-password",
                SecretValueAssignment = "ViaBicepparam",
            },
        };

        // Act
        var result = MainBicepAssembler.Generate(
            modules,
            resourceGroups,
            new NamingContext(),
            roleAssignments: [],
            appSettings,
            existingResourceReferences: []);

        var content = result.Content.ReplaceLineEndings("\n");

        // Assert
        content.Should().Contain("@description('Value for the \\\'sku\\\' input of WebApp resource \\\'api-service\\\'.')\nparam webAppApiServiceSku string");
        content.Should().Contain("@secure()\n@description('Secure value for the \\\'adminPassword\\\' input of WebApp resource \\\'api-service\\\'.')\nparam webAppApiServiceAdminPassword string");
        content.Should().Contain("// The following inputs are used as environment variables for application api-service.\nparam apiServiceConnectionstring string\nparam apiServiceApiKey string");
        content.Should().Contain("// The following inputs are used as environment variables for application worker-service.\nparam workerServiceBaseUrl string");
        content.Should().Contain("@secure()\n@description('Secret value for Key Vault secret \\\'db-password\\\' used by api-service')\nparam apiServiceDbPasswordSecretValue string");
        content.Should().NotContain("@description('Value for the \\\'ConnectionString\\\' input");
        content.Should().NotContain("@description('Value for the \\\'API_KEY\\\' input");
        content.Should().NotContain("@description('Value for the \\\'BASE_URL\\\' input");
    }

    private static GeneratedTypeModule NewModule(
        string moduleName,
        string logicalName,
        string resourceTypeName,
        string folder,
        string file,
        IReadOnlyDictionary<string, object> parameters,
        IReadOnlyList<string> secureParameters)
    {
        return new GeneratedTypeModule
        {
            ModuleName = moduleName,
            ModuleFileName = file,
            ModuleFolderName = folder,
            ResourceGroupName = "apps",
            LogicalResourceName = logicalName,
            ResourceTypeName = resourceTypeName,
            ResourceAbbreviation = "app",
            Parameters = parameters,
            SecureParameters = secureParameters,
        };
    }
}