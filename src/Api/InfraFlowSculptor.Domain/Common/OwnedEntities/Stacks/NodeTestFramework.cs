using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Node test framework used by application pipeline behavior.</summary>
public sealed class NodeTestFramework : EnumValueObject<NodeTestFramework.NodeTestFrameworkType>
{
    /// <summary>Initializes a new instance of the <see cref="NodeTestFramework"/> class.</summary>
    /// <param name="value">Node test framework value.</param>
    public NodeTestFramework(NodeTestFrameworkType value) : base(value)
    {
    }

    private NodeTestFramework() : base()
    {
    }

    /// <summary>Available Node test frameworks.</summary>
    public enum NodeTestFrameworkType
    {
        /// <summary>Jest test framework.</summary>
        Jest,

        /// <summary>Vitest test framework.</summary>
        Vitest,

        /// <summary>Mocha test framework.</summary>
        Mocha,
    }

    /// <summary>Jest test framework.</summary>
    public static readonly NodeTestFramework Jest = new(NodeTestFrameworkType.Jest);

    /// <summary>Vitest test framework.</summary>
    public static readonly NodeTestFramework Vitest = new(NodeTestFrameworkType.Vitest);

    /// <summary>Mocha test framework.</summary>
    public static readonly NodeTestFramework Mocha = new(NodeTestFrameworkType.Mocha);
}