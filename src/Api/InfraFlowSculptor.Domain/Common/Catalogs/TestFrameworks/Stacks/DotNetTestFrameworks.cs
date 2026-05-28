using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;

/// <summary>
/// Test framework definitions for the .NET application stack.
/// </summary>
public static class DotNetTestFrameworks
{
    /// <summary>xUnit test framework.</summary>
    public static readonly TestFrameworkDefinition XUnit = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.DotNet, "XUnit"),
        "xUnit",
        "dotnet test --logger \"trx;LogFileName=results.trx\" --collect:\"XPlat Code Coverage\" --results-directory $(Agent.TempDirectory)/TestResults",
        "VSTest",
        "Cobertura",
        "$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml");

    /// <summary>NUnit test framework.</summary>
    public static readonly TestFrameworkDefinition NUnit = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.DotNet, "NUnit"),
        "NUnit",
        "dotnet test --logger \"trx;LogFileName=results.trx\" --collect:\"XPlat Code Coverage\" --results-directory $(Agent.TempDirectory)/TestResults",
        "VSTest",
        "Cobertura",
        "$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml");

    /// <summary>MSTest test framework.</summary>
    public static readonly TestFrameworkDefinition MSTest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.DotNet, "MSTest"),
        "MSTest",
        "dotnet test --logger \"trx;LogFileName=results.trx\" --collect:\"XPlat Code Coverage\" --results-directory $(Agent.TempDirectory)/TestResults",
        "VSTest",
        "Cobertura",
        "$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml");

    /// <summary>All .NET test framework definitions.</summary>
    public static IReadOnlyList<TestFrameworkDefinition> All { get; } = [XUnit, NUnit, MSTest];
}
