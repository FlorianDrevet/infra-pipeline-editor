using System.Collections.Frozen;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>
/// Valid application stack identifiers for MCP draft pipeline intent.
/// Maps to <c>ApplicationStack.ApplicationStackEnum</c> values in the domain.
/// </summary>
internal static class DraftApplicationStacks
{
    internal const string DotNet = "DotNet";
    internal const string NodeJs = "NodeJs";
    internal const string Angular = "Angular";
    internal const string Java = "Java";
    internal const string Python = "Python";
    internal const string Php = "Php";
    internal const string Go = "Go";
    internal const string StaticSite = "StaticSite";
    internal const string Custom = "Custom";

    /// <summary>All valid stack identifiers.</summary>
    internal static readonly FrozenSet<string> All = FrozenSet.ToFrozenSet(
    [
        DotNet,
        NodeJs,
        Angular,
        Java,
        Python,
        Php,
        Go,
        StaticSite,
        Custom,
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>Determines whether the given value is a recognized application stack.</summary>
    internal static bool IsValid(string? value) => value is not null && All.Contains(value);
}
