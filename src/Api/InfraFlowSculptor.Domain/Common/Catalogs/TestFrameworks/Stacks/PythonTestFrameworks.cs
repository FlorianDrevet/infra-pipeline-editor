using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;

/// <summary>
/// Test framework definitions for the Python application stack.
/// </summary>
public static class PythonTestFrameworks
{
    /// <summary>pytest test framework.</summary>
    public static readonly TestFrameworkDefinition Pytest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Python, "Pytest"),
        "pytest",
        "python -m pytest --junitxml=results.xml --cov --cov-report=xml",
        "JUnit",
        "Cobertura",
        "coverage.xml");

    /// <summary>unittest test framework (standard library).</summary>
    public static readonly TestFrameworkDefinition Unittest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Python, "Unittest"),
        "unittest",
        "python -m pytest --junitxml=results.xml",
        "JUnit",
        null,
        "");

    /// <summary>All Python test framework definitions.</summary>
    public static IReadOnlyList<TestFrameworkDefinition> All { get; } = [Pytest, Unittest];
}
