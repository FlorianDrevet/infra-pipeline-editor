using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;

/// <summary>
/// Test framework definitions for the Angular application stack.
/// </summary>
public static class AngularTestFrameworks
{
    /// <summary>Jasmine test framework (default Angular CLI runner).</summary>
    public static readonly TestFrameworkDefinition Jasmine = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Angular, "Jasmine"),
        "Jasmine (Karma)",
        "npx ng test --watch=false --browsers=ChromeHeadless --code-coverage",
        "JUnit",
        "Cobertura",
        "coverage/cobertura-coverage.xml");

    /// <summary>Jest test framework for Angular (via jest-preset-angular).</summary>
    public static readonly TestFrameworkDefinition Jest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Angular, "Jest"),
        "Jest",
        "npx jest --ci --reporters=default --reporters=jest-junit --coverage --coverageReporters=cobertura",
        "JUnit",
        "Cobertura",
        "coverage/cobertura-coverage.xml");

    /// <summary>Vitest test framework for Angular (via @analogjs/vitest-angular).</summary>
    public static readonly TestFrameworkDefinition Vitest = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Angular, "Vitest"),
        "Vitest",
        "npx vitest run --reporter=junit --outputFile=results.xml --coverage",
        "JUnit",
        "Cobertura",
        "coverage/cobertura-coverage.xml");

    /// <summary>All Angular test framework definitions.</summary>
    public static IReadOnlyList<TestFrameworkDefinition> All { get; } = [Jasmine, Jest, Vitest];
}
