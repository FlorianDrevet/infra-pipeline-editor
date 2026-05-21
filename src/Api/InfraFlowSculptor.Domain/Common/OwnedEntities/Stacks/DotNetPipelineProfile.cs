namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines .NET-specific application pipeline behavior.</summary>
public sealed class DotNetPipelineProfile : AppPipelineStackProfile
{
    /// <summary>Gets the default .NET test framework.</summary>
    public DotNetTestFramework TestFramework { get; private set; } = DotNetTestFramework.XUnit;

    /// <summary>Gets a value indicating whether the pipeline collects code coverage.</summary>
    public bool CollectCoverage { get; private set; }

    /// <summary>Gets the optional custom test project glob used by the pipeline.</summary>
    public string? CustomTestProjectGlob { get; private set; }

    private DotNetPipelineProfile()
    {
    }

    private DotNetPipelineProfile(
        DotNetTestFramework testFramework,
        bool collectCoverage,
        string? customTestProjectGlob)
    {
        TestFramework = testFramework;
        CollectCoverage = collectCoverage;
        CustomTestProjectGlob = customTestProjectGlob;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.DotNet;

    /// <summary>Creates the default .NET pipeline profile.</summary>
    /// <returns>The default .NET pipeline profile.</returns>
    public static DotNetPipelineProfile CreateDefault()
    {
        return Create(DotNetTestFramework.XUnit, collectCoverage: false, customTestProjectGlob: null);
    }

    /// <summary>Creates a .NET pipeline profile.</summary>
    /// <param name="testFramework">Test framework used by the pipeline.</param>
    /// <param name="collectCoverage">Whether the pipeline collects code coverage.</param>
    /// <param name="customTestProjectGlob">Optional custom test project glob.</param>
    /// <returns>The configured .NET pipeline profile.</returns>
    public static DotNetPipelineProfile Create(
        DotNetTestFramework testFramework,
        bool collectCoverage,
        string? customTestProjectGlob)
    {
        ArgumentNullException.ThrowIfNull(testFramework);

        return new DotNetPipelineProfile(
            testFramework,
            collectCoverage,
            customTestProjectGlob);
    }
}