using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Python package manager used by application pipeline behavior.</summary>
public sealed class PythonPackageManager : EnumValueObject<PythonPackageManager.PythonPackageManagerType>
{
    /// <summary>Initializes a new instance of the <see cref="PythonPackageManager"/> class.</summary>
    /// <param name="value">Python package manager value.</param>
    public PythonPackageManager(PythonPackageManagerType value) : base(value)
    {
    }

    private PythonPackageManager() : base()
    {
    }

    /// <summary>Available Python package managers.</summary>
    public enum PythonPackageManagerType
    {
        /// <summary>pip package manager.</summary>
        Pip,

        /// <summary>Poetry package manager.</summary>
        Poetry,

        /// <summary>uv package manager.</summary>
        Uv,
    }

    /// <summary>pip package manager.</summary>
    public static readonly PythonPackageManager Pip = new(PythonPackageManagerType.Pip);

    /// <summary>Poetry package manager.</summary>
    public static readonly PythonPackageManager Poetry = new(PythonPackageManagerType.Poetry);

    /// <summary>uv package manager.</summary>
    public static readonly PythonPackageManager Uv = new(PythonPackageManagerType.Uv);
}