using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.GenerationParity.Tests;

/// <summary>
/// In-memory fixture builders for the parity harness.
/// Fixtures are intentionally minimal — just enough to exercise the engine code paths
/// without any DB, Application, or Domain dependency.
/// </summary>
internal static class FixtureBuilders
{
    /// <summary>
    /// Builds the Bicep engine with all generators required by the fixtures
    /// (KeyVault, AppServicePlan, StorageAccount).
    /// </summary>
    public static BicepGenerationEngine CreateBicepEngine()
    {
        var generators = new IResourceTypeBicepGenerator[]
        {
            new KeyVaultTypeBicepGenerator(),
            new AppServicePlanTypeBicepGenerator(),
            new StorageAccountTypeBicepGenerator(),
        };
        return new BicepGenerationEngine(generators);
    }

    public static PipelineGeneration.PipelineGenerationEngine CreatePipelineEngine()
        => new();

    private static NamingContext DefaultNamingContext() => new()
    {
        DefaultTemplate = "{name}-{resourceAbbr}-{suffix}",
        ResourceTemplates = new Dictionary<string, string>(),
        ResourceAbbreviations = new Dictionary<string, string>
        {
            [AzureResourceTypes.ResourceGroup] = "rg",
            [AzureResourceTypes.KeyVault] = "kv",
            [AzureResourceTypes.AppServicePlan] = "asp",
            [AzureResourceTypes.StorageAccount] = "st",
        },
    };

    private static EnvironmentDefinition DevEnv() => new()
    {
        Name = "dev",
        ShortName = "dev",
        Location = "westeurope",
        Prefix = "dev",
        Suffix = "dev",
        AzureResourceManagerConnection = "sc-arm-dev",
        SubscriptionId = "00000000-0000-0000-0000-000000000001",
        Tags = new Dictionary<string, string> { ["env"] = "dev" },
    };

    /// <summary>
    /// Fixture A — single mono-config with 1 resource group + 1 KeyVault + 1 environment.
    /// Exercises <see cref="BicepGenerationEngine.Generate(GenerationRequest)"/> and
    /// <see cref="PipelineGeneration.PipelineGenerationEngine.Generate(GenerationRequest, string, bool)"/>.
    /// </summary>
    public static GenerationRequest BuildSingleConfigRequest()
    {
        var resources = new List<ResourceDefinition>
        {
            new()
            {
                ResourceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "kvtest",
                Sku = "standard",
                Type = AzureResourceTypes.ArmTypes.KeyVault,
                ResourceGroupName = "rg-test",
                ResourceAbbreviation = "kv",
                Properties = new Dictionary<string, string>
                {
                    ["sku"] = "standard",
                },
                EnvironmentConfigs = new Dictionary<string, IReadOnlyDictionary<string, string>>(),
            },
        };

        return new GenerationRequest
        {
            Resources = resources,
            ResourceGroups =
            [
                new ResourceGroupDefinition
                {
                    Name = "rg-test",
                    Location = "westeurope",
                    ResourceAbbreviation = "rg",
                },
            ],
            Environments = [DevEnv()],
            EnvironmentNames = ["dev"],
            NamingContext = DefaultNamingContext(),
            ProjectTags = new Dictionary<string, string> { ["project"] = "parity" },
            ConfigTags = new Dictionary<string, string>(),
        };
    }

    /// <summary>
    /// Fixture B — two configurations sharing one environment.
    /// Config "api" : KeyVault + AppServicePlan.
    /// Config "worker" : StorageAccount.
    /// Exercises <see cref="BicepGenerationEngine.GenerateMonoRepo(MonoRepoGenerationRequest)"/>
    /// and <see cref="PipelineGeneration.MonoRepoPipelineAssembler.Assemble"/>.
    /// </summary>
    public static MonoRepoGenerationRequest BuildMonoRepoRequest()
    {
        var apiRequest = new GenerationRequest
        {
            Resources =
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                    Name = "kvapi",
                    Sku = "standard",
                    Type = AzureResourceTypes.ArmTypes.KeyVault,
                    ResourceGroupName = "rg-api",
                    ResourceAbbreviation = "kv",
                    Properties = new Dictionary<string, string> { ["sku"] = "standard" },
                },
                new ResourceDefinition
                {
                    ResourceId = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                    Name = "aspapi",
                    Sku = "B1",
                    Type = AzureResourceTypes.ArmTypes.AppServicePlan,
                    ResourceGroupName = "rg-api",
                    ResourceAbbreviation = "asp",
                    Properties = new Dictionary<string, string>
                    {
                        ["sku"] = "B1",
                        ["capacity"] = "1",
                        ["osType"] = "Linux",
                    },
                },
            ],
            ResourceGroups =
            [
                new ResourceGroupDefinition { Name = "rg-api", Location = "westeurope", ResourceAbbreviation = "rg" },
            ],
            Environments = [DevEnv()],
            EnvironmentNames = ["dev"],
            NamingContext = DefaultNamingContext(),
            ProjectTags = new Dictionary<string, string> { ["project"] = "parity" },
            ConfigTags = new Dictionary<string, string> { ["component"] = "api" },
        };

        var workerRequest = new GenerationRequest
        {
            Resources =
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                    Name = "stworker",
                    Sku = "Standard_LRS",
                    Type = AzureResourceTypes.ArmTypes.StorageAccount,
                    ResourceGroupName = "rg-worker",
                    ResourceAbbreviation = "st",
                    Properties = new Dictionary<string, string>
                    {
                        ["sku"] = "Standard_LRS",
                    },
                },
            ],
            ResourceGroups =
            [
                new ResourceGroupDefinition { Name = "rg-worker", Location = "westeurope", ResourceAbbreviation = "rg" },
            ],
            Environments = [DevEnv()],
            EnvironmentNames = ["dev"],
            NamingContext = DefaultNamingContext(),
            ProjectTags = new Dictionary<string, string> { ["project"] = "parity" },
            ConfigTags = new Dictionary<string, string> { ["component"] = "worker" },
        };

        return new MonoRepoGenerationRequest
        {
            ConfigRequests = new Dictionary<string, GenerationRequest>
            {
                ["api"] = apiRequest,
                ["worker"] = workerRequest,
            },
            NamingContext = DefaultNamingContext(),
            Environments = [DevEnv()],
            EnvironmentNames = ["dev"],
        };
    }
}
