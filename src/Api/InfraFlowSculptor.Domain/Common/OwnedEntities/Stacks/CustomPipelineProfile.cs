namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Defines custom application pipeline commands for non-standard stacks.</summary>
/// <remarks>
/// This profile is an advanced escape hatch for non-standard pipelines. Typed emitters should not treat it as an idiomatic stack.
/// </remarks>
public sealed class CustomPipelineProfile : AppPipelineStackProfile
{
    /// <summary>Gets the optional custom test command.</summary>
    public string? CustomTestCommand { get; private set; }

    /// <summary>Gets the optional custom lint command.</summary>
    public string? CustomLintCommand { get; private set; }

    /// <summary>Gets the optional custom build command.</summary>
    public string? CustomBuildCommand { get; private set; }

    private CustomPipelineProfile()
    {
    }

    private CustomPipelineProfile(
        string? customTestCommand,
        string? customLintCommand,
        string? customBuildCommand)
    {
        CustomTestCommand = customTestCommand;
        CustomLintCommand = customLintCommand;
        CustomBuildCommand = customBuildCommand;
    }

    /// <inheritdoc />
    public override ApplicationStack Stack => ApplicationStack.Custom;

    /// <summary>Creates a custom pipeline profile.</summary>
    /// <param name="customTestCommand">Optional custom test command.</param>
    /// <param name="customLintCommand">Optional custom lint command.</param>
    /// <param name="customBuildCommand">Optional custom build command.</param>
    /// <returns>The configured custom pipeline profile.</returns>
    public static CustomPipelineProfile Create(
        string? customTestCommand,
        string? customLintCommand,
        string? customBuildCommand)
    {
        return new CustomPipelineProfile(
            NormalizeOptionalCommand(customTestCommand),
            NormalizeOptionalCommand(customLintCommand),
            NormalizeOptionalCommand(customBuildCommand));
    }

    private static string? NormalizeOptionalCommand(string? command)
    {
        return string.IsNullOrWhiteSpace(command) ? null : command.Trim();
    }
}