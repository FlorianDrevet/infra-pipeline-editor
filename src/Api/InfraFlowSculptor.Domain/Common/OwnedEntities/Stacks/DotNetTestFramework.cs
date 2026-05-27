using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the .NET test framework used by application pipeline behavior.</summary>
public sealed class DotNetTestFramework : EnumValueObject<DotNetTestFramework.DotNetTestFrameworkType>
{
    /// <summary>Initializes a new instance of the <see cref="DotNetTestFramework"/> class.</summary>
    /// <param name="value">.NET test framework value.</param>
    public DotNetTestFramework(DotNetTestFrameworkType value) : base(value)
    {
    }

    private DotNetTestFramework() : base()
    {
    }

    /// <summary>Available .NET test frameworks.</summary>
    public enum DotNetTestFrameworkType
    {
        /// <summary>xUnit test framework.</summary>
        XUnit,

        /// <summary>NUnit test framework.</summary>
        NUnit,

        /// <summary>MSTest test framework.</summary>
        MSTest,
    }

    /// <summary>xUnit test framework.</summary>
    public static readonly DotNetTestFramework XUnit = new(DotNetTestFrameworkType.XUnit);

    /// <summary>NUnit test framework.</summary>
    public static readonly DotNetTestFramework NUnit = new(DotNetTestFrameworkType.NUnit);

    /// <summary>MSTest test framework.</summary>
    public static readonly DotNetTestFramework MSTest = new(DotNetTestFrameworkType.MSTest);
}