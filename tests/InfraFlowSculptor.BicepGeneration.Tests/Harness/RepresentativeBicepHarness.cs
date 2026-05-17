using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Harness;

internal static class RepresentativeBicepHarness
{
    private const string OutputDirectoryEnvironmentVariableName = "IFS_BICEP_REPRESENTATIVE_OUTPUT_DIR";
    private const string DevelopmentEnvironmentName = "Development";
    private const string DevelopmentEnvironmentShortName = "dev";
    private const string ResourceGroupName = "rg-app";
    private const string AzureLocation = "francecentral";
    private const string KeyVaultRoleDefinitionId = "4633458b-17de-408a-b874-0445c86b69e6";
    private const string KeyVaultRoleDefinitionName = "Key Vault Secrets User";
    private const string KeyVaultRoleDefinitionDescription = "Read secret contents including the secret portion of a certificate with private key.";
    private const string KeyVaultServiceCategory = "keyvault";
    private const string WebAppSecretSettingName = "KeyVault__ApiSecret";
    private const string WebAppSecretName = "api-secret";
    private const string ViaBicepParamSecretAssignment = "ViaBicepparam";

    internal static IReadOnlyDictionary<string, string> GenerateFiles()
    {
        var request = CreateRequest();
        var result = CreateEngine().Generate(request);

        if (result.IsError)
        {
            var errorMessage = string.Join(
                Environment.NewLine,
                result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Representative Bicep generation failed.{Environment.NewLine}{errorMessage}");
        }

        return result.Value.Files;
    }

    internal static string ResolveOutputDirectory()
    {
        var configuredOutputDirectory = Environment.GetEnvironmentVariable(OutputDirectoryEnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(configuredOutputDirectory))
        {
            return Path.GetFullPath(configuredOutputDirectory);
        }

        return Path.Combine(
            Path.GetTempPath(),
            $"{nameof(RepresentativeBicepHarness)}-{Guid.NewGuid():N}");
    }

    internal static bool ShouldPreserveConfiguredOutputDirectory()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(OutputDirectoryEnvironmentVariableName));
    }

    internal static IReadOnlyDictionary<string, string> WriteFiles(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var resolvedOutputDirectory = Path.GetFullPath(outputDirectory);
        var files = GenerateFiles();

        if (Directory.Exists(resolvedOutputDirectory))
        {
            Directory.Delete(resolvedOutputDirectory, recursive: true);
        }

        Directory.CreateDirectory(resolvedOutputDirectory);

        foreach (var (relativePath, content) in files.OrderBy(file => file.Key, StringComparer.Ordinal))
        {
            var fullPath = Path.Combine(
                resolvedOutputDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar));

            var directoryPath = Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException($"Could not resolve the directory for '{fullPath}'.");

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(fullPath, content);
        }

        return files;
    }

    private static BicepGenerationEngine CreateEngine()
    {
        var generators = new IResourceTypeBicepSpecGenerator[]
        {
            new AppServicePlanTypeBicepGenerator(),
            new KeyVaultTypeBicepGenerator(),
            new UserAssignedIdentityTypeBicepGenerator(),
            new WebAppTypeBicepGenerator(),
        };

        var pipeline = new BicepGenerationPipeline(
        [
            new IdentityAnalysisStage(),
            new AppSettingsAnalysisStage(),
            new ModuleBuildStage(generators),
            new IdentityInjectionStage(),
            new OutputInjectionStage(),
            new AppSettingsInjectionStage(),
            new TagsInjectionStage(),
            new ParentReferenceResolutionStage(),
            new SpecEmissionStage(),
            new AssemblyStage(),
            new IrOutputPruningStage(),
        ]);

        return new BicepGenerationEngine(pipeline);
    }

    private static GenerationRequest CreateRequest()
    {
        var appServicePlan = CreateAppServicePlanResource();
        var keyVault = CreateKeyVaultResource();
        var userAssignedIdentity = CreateUserAssignedIdentityResource();
        var webApp = CreateWebAppResource(appServicePlan);

        return new GenerationRequest
        {
            Resources = [appServicePlan, keyVault, userAssignedIdentity, webApp],
            Environments =
            [
                new EnvironmentDefinition
                {
                    Name = DevelopmentEnvironmentName,
                    ShortName = DevelopmentEnvironmentShortName,
                    Location = AzureLocation,
                },
            ],
            ResourceGroups =
            [
                new ResourceGroupDefinition
                {
                    Name = ResourceGroupName,
                    Location = AzureLocation,
                    ResourceAbbreviation = "rg",
                },
            ],
            EnvironmentNames = [DevelopmentEnvironmentName],
            NamingContext = new NamingContext(),
            ProjectTags = new Dictionary<string, string>
            {
                ["owner"] = "infraflowsculptor",
            },
            RoleAssignments = [CreateKeyVaultRoleAssignment(userAssignedIdentity, keyVault)],
            AppSettings = [CreateWebAppKeyVaultSecretSetting(webApp, keyVault)],
        };
    }

    private static ResourceDefinition CreateAppServicePlanResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "ifs-plan",
            Type = AzureResourceTypes.ArmTypes.AppServicePlanType,
            ResourceGroupName = ResourceGroupName,
            ResourceAbbreviation = "asp",
            Properties = new Dictionary<string, string>
            {
                ["sku"] = "F1",
                ["capacity"] = "1",
                ["osType"] = "Linux",
            },
        };
    }

    private static ResourceDefinition CreateKeyVaultResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "ifs-kv",
            Type = AzureResourceTypes.ArmTypes.KeyVaultType,
            ResourceGroupName = ResourceGroupName,
            ResourceAbbreviation = "kv",
            Sku = "Standard",
        };
    }

    private static ResourceDefinition CreateUserAssignedIdentityResource()
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "ifs-identity",
            Type = AzureResourceTypes.ArmTypes.UserAssignedIdentityType,
            ResourceGroupName = ResourceGroupName,
            ResourceAbbreviation = "id",
        };
    }

    private static ResourceDefinition CreateWebAppResource(ResourceDefinition appServicePlan)
    {
        return new ResourceDefinition
        {
            ResourceId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "ifs-api",
            Type = AzureResourceTypes.ArmTypes.WebAppType,
            ResourceGroupName = ResourceGroupName,
            ResourceAbbreviation = "app",
            Properties = new Dictionary<string, string>
            {
                ["appServicePlanId"] = appServicePlan.ResourceId.ToString(),
                ["deploymentMode"] = "Code",
                ["runtimeStack"] = "DOTNETCORE",
                ["runtimeVersion"] = "8.0",
                ["alwaysOn"] = "true",
                ["httpsOnly"] = "true",
            },
        };
    }

    private static RoleAssignmentDefinition CreateKeyVaultRoleAssignment(
        ResourceDefinition userAssignedIdentity,
        ResourceDefinition keyVault)
    {
        return new RoleAssignmentDefinition
        {
            SourceResourceName = userAssignedIdentity.Name,
            SourceResourceType = userAssignedIdentity.Type,
            SourceResourceTypeName = AzureResourceTypes.UserAssignedIdentity,
            SourceResourceGroupName = userAssignedIdentity.ResourceGroupName,
            TargetResourceName = keyVault.Name,
            TargetResourceType = keyVault.Type,
            TargetResourceTypeName = AzureResourceTypes.KeyVault,
            TargetResourceGroupName = keyVault.ResourceGroupName,
            TargetResourceAbbreviation = keyVault.ResourceAbbreviation,
            ManagedIdentityType = "UserAssigned",
            UserAssignedIdentityName = userAssignedIdentity.Name,
            UserAssignedIdentityResourceId = userAssignedIdentity.ResourceId,
            UserAssignedIdentityResourceGroupName = userAssignedIdentity.ResourceGroupName,
            RoleDefinitionId = KeyVaultRoleDefinitionId,
            RoleDefinitionName = KeyVaultRoleDefinitionName,
            RoleDefinitionDescription = KeyVaultRoleDefinitionDescription,
            ServiceCategory = KeyVaultServiceCategory,
        };
    }

    private static AppSettingDefinition CreateWebAppKeyVaultSecretSetting(
        ResourceDefinition webApp,
        ResourceDefinition keyVault)
    {
        return new AppSettingDefinition
        {
            Name = WebAppSecretSettingName,
            TargetResourceName = webApp.Name,
            IsKeyVaultReference = true,
            KeyVaultResourceName = keyVault.Name,
            SecretName = WebAppSecretName,
            SecretValueAssignment = ViaBicepParamSecretAssignment,
        };
    }
}
