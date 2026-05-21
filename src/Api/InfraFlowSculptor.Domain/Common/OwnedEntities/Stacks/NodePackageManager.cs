using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Node package manager used by application pipeline behavior.</summary>
public sealed class NodePackageManager : EnumValueObject<NodePackageManager.NodePackageManagerType>
{
    /// <summary>Initializes a new instance of the <see cref="NodePackageManager"/> class.</summary>
    /// <param name="value">Node package manager value.</param>
    public NodePackageManager(NodePackageManagerType value) : base(value)
    {
    }

    private NodePackageManager() : base()
    {
    }

    /// <summary>Available Node package managers.</summary>
    public enum NodePackageManagerType
    {
        /// <summary>npm package manager.</summary>
        Npm,

        /// <summary>Yarn package manager.</summary>
        Yarn,

        /// <summary>pnpm package manager.</summary>
        Pnpm,
    }

    /// <summary>npm package manager.</summary>
    public static readonly NodePackageManager Npm = new(NodePackageManagerType.Npm);

    /// <summary>Yarn package manager.</summary>
    public static readonly NodePackageManager Yarn = new(NodePackageManagerType.Yarn);

    /// <summary>pnpm package manager.</summary>
    public static readonly NodePackageManager Pnpm = new(NodePackageManagerType.Pnpm);
}