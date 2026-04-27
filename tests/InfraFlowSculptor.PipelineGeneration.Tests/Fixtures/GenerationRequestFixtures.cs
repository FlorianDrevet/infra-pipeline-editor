using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

/// <summary>
/// Synthetic, deterministic fixtures for <see cref="GenerationRequest"/>.
/// All values are hardcoded to guarantee byte-for-byte stable golden file capture
/// for the <c>PipelineGenerationEngine</c> and <c>MonoRepoPipelineAssembler</c>.
/// </summary>
internal static class GenerationRequestFixtures
{
    private const string SubscriptionDev = "00000000-0000-0000-0000-000000000001";
    private const string SubscriptionQa = "00000000-0000-0000-0000-000000000002";
    private const string SubscriptionProd = "00000000-0000-0000-0000-000000000003";

    /// <summary>Single <c>dev</c> environment, no resources, no variable groups.</summary>
    public static GenerationRequest MinimalStandalone() => new()
    {
        Environments =
        [
            BuildEnvironment("dev", SubscriptionDev),
        ],
        EnvironmentNames = ["dev"],
    };

    /// <summary>Two environments (<c>dev</c> + <c>prod</c>) with one resource group, zero resources.</summary>
    public static GenerationRequest StandardStandalone() => new()
    {
        Environments =
        [
            BuildEnvironment("dev", SubscriptionDev),
            BuildEnvironment("prod", SubscriptionProd),
        ],
        EnvironmentNames = ["dev", "prod"],
        ResourceGroups =
        [
            new ResourceGroupDefinition
            {
                Name = "core",
                Location = "westeurope",
                ResourceAbbreviation = "rg",
            },
        ],
    };

    /// <summary>Two environments with one variable group and one secure parameter override.</summary>
    public static GenerationRequest WithVariableGroupsAndSecureParams() => new()
    {
        Environments =
        [
            BuildEnvironment("dev", SubscriptionDev),
            BuildEnvironment("prod", SubscriptionProd),
        ],
        EnvironmentNames = ["dev", "prod"],
        ResourceGroups =
        [
            new ResourceGroupDefinition
            {
                Name = "core",
                Location = "westeurope",
                ResourceAbbreviation = "rg",
            },
        ],
        PipelineVariableGroups =
        [
            new PipelineVariableGroupDefinition
            {
                GroupName = "ifs-shared-dev",
                Mappings =
                [
                    new PipelineVariableMappingDefinition
                    {
                        PipelineVariableName = "SqlAdminPassword",
                        BicepParameterName = "sqlServerAdministratorLoginPassword",
                    },
                ],
            },
        ],
        SecureParameterOverrides = ["sqlServerAdministratorLoginPassword"],
    };

    /// <summary>Three environments (<c>dev</c>, <c>qa</c>, <c>prod</c>), used by mono-repo assembler tests.</summary>
    public static IReadOnlyList<EnvironmentDefinition> ThreeEnvironments() =>
    [
        BuildEnvironment("dev", SubscriptionDev),
        BuildEnvironment("qa", SubscriptionQa),
        BuildEnvironment("prod", SubscriptionProd),
    ];

    /// <summary>Single <c>dev</c> environment list for shared template captures.</summary>
    public static IReadOnlyList<EnvironmentDefinition> OneEnvironment() =>
    [
        BuildEnvironment("dev", SubscriptionDev),
    ];

    private static EnvironmentDefinition BuildEnvironment(string name, string subscriptionId) => new()
    {
        Name = name,
        ShortName = name,
        Location = "westeurope",
        Prefix = name,
        Suffix = name,
        AzureResourceManagerConnection = $"sc-{name}",
        SubscriptionId = subscriptionId,
    };
}
