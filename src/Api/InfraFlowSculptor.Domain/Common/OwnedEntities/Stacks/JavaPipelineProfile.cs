namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines Java-specific application pipeline behavior.</summary>
public sealed class JavaPipelineProfile : AppPipelineStackProfile
{
    /// <summary>Gets the Java build tool used by the pipeline.</summary>
    public JavaBuildTool BuildTool { get; private set; } = JavaBuildTool.Maven;

    /// <summary>Gets the Java test framework used by the pipeline.</summary>
    public JavaTestFramework TestFramework { get; private set; } = JavaTestFramework.JUnit5;

    /// <summary>Gets a value indicating whether the pipeline collects code coverage.</summary>
    public bool CollectCoverage { get; private set; }

    private JavaPipelineProfile()
    {
    }

    private JavaPipelineProfile(
        JavaBuildTool buildTool,
        JavaTestFramework testFramework,
        bool collectCoverage)
    {
        BuildTool = buildTool;
        TestFramework = testFramework;
        CollectCoverage = collectCoverage;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.Java;

    /// <summary>Creates the default Java pipeline profile.</summary>
    /// <returns>The default Java pipeline profile.</returns>
    public static JavaPipelineProfile CreateDefault()
    {
        return Create(JavaBuildTool.Maven, JavaTestFramework.JUnit5, collectCoverage: false);
    }

    /// <summary>Creates a Java pipeline profile.</summary>
    /// <param name="buildTool">Java build tool used by the pipeline.</param>
    /// <param name="testFramework">Java test framework used by the pipeline.</param>
    /// <param name="collectCoverage">Whether the pipeline collects code coverage.</param>
    /// <returns>The configured Java pipeline profile.</returns>
    public static JavaPipelineProfile Create(
        JavaBuildTool buildTool,
        JavaTestFramework testFramework,
        bool collectCoverage)
    {
        ArgumentNullException.ThrowIfNull(buildTool);
        ArgumentNullException.ThrowIfNull(testFramework);

        return new JavaPipelineProfile(buildTool, testFramework, collectCoverage);
    }
}