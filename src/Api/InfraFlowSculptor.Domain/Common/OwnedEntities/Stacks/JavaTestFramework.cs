using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Java test framework used by application pipeline behavior.</summary>
public sealed class JavaTestFramework : EnumValueObject<JavaTestFramework.JavaTestFrameworkType>
{
    /// <summary>Initializes a new instance of the <see cref="JavaTestFramework"/> class.</summary>
    /// <param name="value">Java test framework value.</param>
    public JavaTestFramework(JavaTestFrameworkType value) : base(value)
    {
    }

    private JavaTestFramework() : base()
    {
    }

    /// <summary>Available Java test frameworks.</summary>
    public enum JavaTestFrameworkType
    {
        /// <summary>JUnit 5 test framework.</summary>
        JUnit5,

        /// <summary>JUnit 4 test framework.</summary>
        JUnit4,

        /// <summary>TestNG test framework.</summary>
        TestNg,
    }

    /// <summary>JUnit 5 test framework.</summary>
    public static readonly JavaTestFramework JUnit5 = new(JavaTestFrameworkType.JUnit5);

    /// <summary>JUnit 4 test framework.</summary>
    public static readonly JavaTestFramework JUnit4 = new(JavaTestFrameworkType.JUnit4);

    /// <summary>TestNG test framework.</summary>
    public static readonly JavaTestFramework TestNg = new(JavaTestFrameworkType.TestNg);
}