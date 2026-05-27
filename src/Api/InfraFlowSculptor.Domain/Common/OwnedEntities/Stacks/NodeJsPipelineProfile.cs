namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines Node.js-specific application pipeline behavior.</summary>
public sealed class NodeJsPipelineProfile : AppPipelineStackProfile
{
    private const string DefaultTestScriptName = "test";
    private const string DefaultLintScriptName = "lint";
    private const string BlankScriptNameMessage = "Script name must not be empty.";

    /// <summary>Gets the package manager used by the pipeline.</summary>
    public NodePackageManager PackageManager { get; private set; } = NodePackageManager.Npm;

    /// <summary>Gets the test framework used by the pipeline.</summary>
    public NodeTestFramework TestFramework { get; private set; } = NodeTestFramework.Jest;

    /// <summary>Gets a value indicating whether the pipeline runs the lint script.</summary>
    public bool RunLintScript { get; private set; }

    /// <summary>Gets the package script name used for tests.</summary>
    public string TestScriptName { get; private set; } = DefaultTestScriptName;

    /// <summary>Gets the package script name used for linting.</summary>
    public string LintScriptName { get; private set; } = DefaultLintScriptName;

    private NodeJsPipelineProfile()
    {
    }

    private NodeJsPipelineProfile(
        NodePackageManager packageManager,
        NodeTestFramework testFramework,
        bool runLintScript,
        string testScriptName,
        string lintScriptName)
    {
        PackageManager = packageManager;
        TestFramework = testFramework;
        RunLintScript = runLintScript;
        TestScriptName = testScriptName;
        LintScriptName = lintScriptName;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.NodeJs;

    /// <summary>Creates the default Node.js pipeline profile.</summary>
    /// <returns>The default Node.js pipeline profile.</returns>
    public static NodeJsPipelineProfile CreateDefault()
    {
        return Create(
            NodePackageManager.Npm,
            NodeTestFramework.Jest,
            runLintScript: false,
            DefaultTestScriptName,
            DefaultLintScriptName);
    }

    /// <summary>Creates a Node.js pipeline profile.</summary>
    /// <param name="packageManager">Package manager used by the pipeline.</param>
    /// <param name="testFramework">Test framework used by the pipeline.</param>
    /// <param name="runLintScript">Whether the pipeline runs the lint script.</param>
    /// <param name="testScriptName">Package script name used for tests.</param>
    /// <param name="lintScriptName">Package script name used for linting.</param>
    /// <returns>The configured Node.js pipeline profile.</returns>
    /// <exception cref="ArgumentException">Thrown when a script name is empty.</exception>
    public static NodeJsPipelineProfile Create(
        NodePackageManager packageManager,
        NodeTestFramework testFramework,
        bool runLintScript,
        string testScriptName,
        string lintScriptName)
    {
        ArgumentNullException.ThrowIfNull(packageManager);
        ArgumentNullException.ThrowIfNull(testFramework);

        return new NodeJsPipelineProfile(
            packageManager,
            testFramework,
            runLintScript,
            NormalizeRequiredScriptName(testScriptName, nameof(testScriptName)),
            NormalizeRequiredScriptName(lintScriptName, nameof(lintScriptName)));
    }

    private static string NormalizeRequiredScriptName(string scriptName, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(scriptName))
        {
            throw new ArgumentException(BlankScriptNameMessage, parameterName);
        }

        return scriptName.Trim();
    }
}