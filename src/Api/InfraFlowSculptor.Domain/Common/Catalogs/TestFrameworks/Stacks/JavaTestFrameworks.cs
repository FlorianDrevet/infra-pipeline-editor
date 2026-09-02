using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;

/// <summary>
/// Test framework definitions for the Java application stack.
/// </summary>
public static class JavaTestFrameworks
{
    /// <summary>JUnit 5 (Jupiter) test framework.</summary>
    public static readonly TestFrameworkDefinition JUnit5 = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Java, "JUnit5"),
        "JUnit 5",
        "mvn test",
        "JUnit",
        "JaCoCo",
        "target/site/jacoco/jacoco.xml");

    /// <summary>JUnit 4 test framework.</summary>
    public static readonly TestFrameworkDefinition JUnit4 = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Java, "JUnit4"),
        "JUnit 4",
        "mvn test",
        "JUnit",
        "JaCoCo",
        "target/site/jacoco/jacoco.xml");

    /// <summary>TestNG test framework.</summary>
    public static readonly TestFrameworkDefinition TestNG = new(
        new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.Java, "TestNG"),
        "TestNG",
        "mvn test",
        "JUnit",
        "JaCoCo",
        "target/site/jacoco/jacoco.xml");

    /// <summary>All Java test framework definitions.</summary>
    public static IReadOnlyList<TestFrameworkDefinition> All { get; } = [JUnit5, JUnit4, TestNG];
}
