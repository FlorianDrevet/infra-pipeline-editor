using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Python test framework used by application pipeline behavior.</summary>
public sealed class PythonTestFramework : EnumValueObject<PythonTestFramework.PythonTestFrameworkType>
{
    /// <summary>Initializes a new instance of the <see cref="PythonTestFramework"/> class.</summary>
    /// <param name="value">Python test framework value.</param>
    public PythonTestFramework(PythonTestFrameworkType value) : base(value)
    {
    }

    private PythonTestFramework() : base()
    {
    }

    /// <summary>Available Python test frameworks.</summary>
    public enum PythonTestFrameworkType
    {
        /// <summary>pytest test framework.</summary>
        Pytest,

        /// <summary>unittest test framework.</summary>
        Unittest,
    }

    /// <summary>pytest test framework.</summary>
    public static readonly PythonTestFramework Pytest = new(PythonTestFrameworkType.Pytest);

    /// <summary>unittest test framework.</summary>
    public static readonly PythonTestFramework Unittest = new(PythonTestFrameworkType.Unittest);
}