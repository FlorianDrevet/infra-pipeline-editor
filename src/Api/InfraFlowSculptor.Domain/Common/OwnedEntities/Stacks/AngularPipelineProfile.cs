namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines Angular-specific application pipeline behavior.</summary>
public sealed class AngularPipelineProfile : AppPipelineStackProfile
{
    /// <summary>Gets the package manager used by the pipeline.</summary>
    public NodePackageManager PackageManager { get; private set; } = NodePackageManager.Npm;

    /// <summary>Gets a value indicating whether the pipeline runs Angular tests.</summary>
    public bool RunNgTest { get; private set; } = true;

    /// <summary>Gets a value indicating whether the pipeline runs Angular linting.</summary>
    public bool RunNgLint { get; private set; }

    /// <summary>Gets a value indicating whether the pipeline runs a production Angular build.</summary>
    public bool RunNgBuildProduction { get; private set; } = true;

    /// <summary>Gets the optional Angular project name used in workspace commands.</summary>
    public string? ProjectName { get; private set; }

    private AngularPipelineProfile()
    {
    }

    private AngularPipelineProfile(
        NodePackageManager packageManager,
        bool runNgTest,
        bool runNgLint,
        bool runNgBuildProduction,
        string? projectName)
    {
        PackageManager = packageManager;
        RunNgTest = runNgTest;
        RunNgLint = runNgLint;
        RunNgBuildProduction = runNgBuildProduction;
        ProjectName = projectName;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.Angular;

    /// <summary>Creates the default Angular pipeline profile with tests and production build enabled.</summary>
    /// <returns>The default Angular pipeline profile.</returns>
    public static AngularPipelineProfile CreateDefault()
    {
        return Create(
            NodePackageManager.Npm,
            runNgTest: true,
            runNgLint: false,
            runNgBuildProduction: true,
            projectName: null);
    }

    /// <summary>Creates an Angular pipeline profile.</summary>
    /// <param name="packageManager">Package manager used by the pipeline.</param>
    /// <param name="runNgTest">Whether the pipeline runs Angular tests.</param>
    /// <param name="runNgLint">Whether the pipeline runs Angular linting.</param>
    /// <param name="runNgBuildProduction">Whether the pipeline runs a production Angular build.</param>
    /// <param name="projectName">Optional Angular project name used in workspace commands.</param>
    /// <returns>The configured Angular pipeline profile.</returns>
    public static AngularPipelineProfile Create(
        NodePackageManager packageManager,
        bool runNgTest,
        bool runNgLint,
        bool runNgBuildProduction,
        string? projectName)
    {
        ArgumentNullException.ThrowIfNull(packageManager);

        return new AngularPipelineProfile(
            packageManager,
            runNgTest,
            runNgLint,
            runNgBuildProduction,
            NormalizeOptionalText(projectName));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}