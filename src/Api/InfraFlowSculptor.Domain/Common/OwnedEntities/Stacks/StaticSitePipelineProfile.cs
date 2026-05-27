namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines static-site-specific application pipeline behavior.</summary>
public sealed class StaticSitePipelineProfile : AppPipelineStackProfile
{
    private const string BlankBuildCommandMessage = "Build command must not be empty.";
    private const string BlankOutputDirectoryMessage = "Output directory must not be empty.";

    /// <summary>Gets the build command used by the pipeline.</summary>
    public string BuildCommand { get; private set; } = string.Empty;

    /// <summary>Gets the output directory produced by the build command.</summary>
    public string OutputDirectory { get; private set; } = string.Empty;

    private StaticSitePipelineProfile()
    {
    }

    private StaticSitePipelineProfile(string buildCommand, string outputDirectory)
    {
        BuildCommand = buildCommand;
        OutputDirectory = outputDirectory;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.StaticSite;

    /// <summary>Creates a static site pipeline profile.</summary>
    /// <param name="buildCommand">Build command used by the pipeline.</param>
    /// <param name="outputDirectory">Output directory produced by the build command.</param>
    /// <returns>The configured static site pipeline profile.</returns>
    /// <exception cref="ArgumentException">Thrown when the build command or output directory is empty.</exception>
    public static StaticSitePipelineProfile Create(string buildCommand, string outputDirectory)
    {
        return new StaticSitePipelineProfile(
            NormalizeRequiredText(buildCommand, nameof(buildCommand), BlankBuildCommandMessage),
            NormalizeRequiredText(outputDirectory, nameof(outputDirectory), BlankOutputDirectoryMessage));
    }

    private static string NormalizeRequiredText(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }

        return value.Trim();
    }
}