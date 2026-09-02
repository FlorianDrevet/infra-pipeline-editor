using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;

/// <summary>
/// Test framework definitions for the Node.js application stack.
/// </summary>
public static class NodeJsTestFrameworks
{
    /// <summary>Jest test framework.</summary>
    public static readonly TestFrameworkDefinition Jest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.NodeJs, "Jest"),
        "Jest",
        "npx jest --ci --reporters=default --reporters=jest-junit --coverage --coverageReporters=cobertura",
        "JUnit",
        "Cobertura",
        "coverage/cobertura-coverage.xml");

    /// <summary>Vitest test framework.</summary>
    public static readonly TestFrameworkDefinition Vitest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.NodeJs, "Vitest"),
        "Vitest",
        "npx vitest run --reporter=junit --outputFile=results.xml --coverage",
        "JUnit",
        "Cobertura",
        "coverage/cobertura-coverage.xml");

    /// <summary>Mocha test framework.</summary>
    public static readonly TestFrameworkDefinition Mocha = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.NodeJs, "Mocha"),
        "Mocha",
        "npx mocha --reporter mocha-junit-reporter --reporter-options mochaFile=results.xml",
        "JUnit",
        null,
        "");

    /// <summary>All Node.js test framework definitions.</summary>
    public static IReadOnlyList<TestFrameworkDefinition> All { get; } = [Jest, Vitest, Mocha];
}
