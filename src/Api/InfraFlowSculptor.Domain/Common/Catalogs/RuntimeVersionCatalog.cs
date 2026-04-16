using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Catalogs;

/// <summary>
/// Provides the exhaustive list of supported runtime versions for each runtime stack.
/// Versions are ordered from most recent to oldest.
/// </summary>
public static class RuntimeVersionCatalog
{
    /// <summary>Gets the supported runtime versions for the specified Web App runtime stack.</summary>
    public static IReadOnlyCollection<string> GetWebAppVersions(WebAppRuntimeStack.WebAppRuntimeStackEnum stack) => stack switch
    {
        WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet => ["10", "9", "8"],
        WebAppRuntimeStack.WebAppRuntimeStackEnum.Node => ["22-lts", "20-lts"],
        WebAppRuntimeStack.WebAppRuntimeStackEnum.Python => ["3.13", "3.12", "3.11", "3.10"],
        WebAppRuntimeStack.WebAppRuntimeStackEnum.Java => ["21", "17", "11"],
        WebAppRuntimeStack.WebAppRuntimeStackEnum.Php => ["8.4", "8.3", "8.2"],
        _ => []
    };

    /// <summary>Gets the supported runtime versions for the specified Function App runtime stack.</summary>
    public static IReadOnlyCollection<string> GetFunctionAppVersions(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum stack) => stack switch
    {
        FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet => ["10-isolated", "9-isolated", "8-isolated", "8-in-process"],
        FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Node => ["22", "20"],
        FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Python => ["3.12", "3.11", "3.10"],
        FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.Java => ["21", "17", "11"],
        FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.PowerShell => ["7.4", "7.2"],
        _ => []
    };

    /// <summary>Checks whether the given runtime version is valid for the specified Web App runtime stack.</summary>
    public static bool IsValidWebAppVersion(WebAppRuntimeStack.WebAppRuntimeStackEnum stack, string version)
        => GetWebAppVersions(stack).Contains(version);

    /// <summary>Checks whether the given runtime version is valid for the specified Function App runtime stack.</summary>
    public static bool IsValidFunctionAppVersion(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum stack, string version)
        => GetFunctionAppVersions(stack).Contains(version);
}
