namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines Python-specific application pipeline behavior.</summary>
public sealed class PythonPipelineProfile : AppPipelineStackProfile
{
    /// <summary>Gets the Python package manager used by the pipeline.</summary>
    public PythonPackageManager PackageManager { get; private set; } = PythonPackageManager.Pip;

    /// <summary>Gets the Python test framework used by the pipeline.</summary>
    public PythonTestFramework TestFramework { get; private set; } = PythonTestFramework.Pytest;

    /// <summary>Gets a value indicating whether the pipeline collects code coverage.</summary>
    public bool CollectCoverage { get; private set; }

    private PythonPipelineProfile()
    {
    }

    private PythonPipelineProfile(
        PythonPackageManager packageManager,
        PythonTestFramework testFramework,
        bool collectCoverage)
    {
        PackageManager = packageManager;
        TestFramework = testFramework;
        CollectCoverage = collectCoverage;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.Python;

    /// <summary>Creates the default Python pipeline profile.</summary>
    /// <returns>The default Python pipeline profile.</returns>
    public static PythonPipelineProfile CreateDefault()
    {
        return Create(PythonPackageManager.Pip, PythonTestFramework.Pytest, collectCoverage: false);
    }

    /// <summary>Creates a Python pipeline profile.</summary>
    /// <param name="packageManager">Python package manager used by the pipeline.</param>
    /// <param name="testFramework">Python test framework used by the pipeline.</param>
    /// <param name="collectCoverage">Whether the pipeline collects code coverage.</param>
    /// <returns>The configured Python pipeline profile.</returns>
    public static PythonPipelineProfile Create(
        PythonPackageManager packageManager,
        PythonTestFramework testFramework,
        bool collectCoverage)
    {
        ArgumentNullException.ThrowIfNull(packageManager);
        ArgumentNullException.ThrowIfNull(testFramework);

        return new PythonPipelineProfile(packageManager, testFramework, collectCoverage);
    }
}