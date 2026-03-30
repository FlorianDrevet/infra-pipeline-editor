using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Represents a directed link between two Azure resources where the source
/// resource provides an output consumed as input by the target resource.
/// </summary>
public sealed class InputOutputLink : Entity<InputOutputId>
{
    /// <summary>Navigation to the source resource providing the output.</summary>
    public AzureResource SourceResource { get; private set; } = null!;

    /// <summary>Identifier of the source resource.</summary>
    public AzureResourceId SourceResourceId { get; private set; } = null!;

    /// <summary>Navigation to the target resource consuming the input.</summary>
    public AzureResource TargetResource { get; private set; } = null!;

    /// <summary>Identifier of the target resource.</summary>
    public AzureResourceId TargetResourceId { get; private set; } = null!;

    /// <summary>The output type on the source resource (must be a valid enum member).</summary>
    public string OutputType { get; private set; } = null!;

    /// <summary>The input type on the target resource (must be a valid enum member).</summary>
    public string InputType { get; private set; } = null!;

    private InputOutputLink(InputOutputId id, AzureResource source,
        AzureResource target,
        string outputType,
        string inputType)
        : base(id)
    {
        SourceResource = source;
        SourceResourceId = source.Id;

        TargetResource = target;
        TargetResourceId = target.Id;

        OutputType = outputType;
        InputType = inputType;

        if (SourceResourceId == TargetResourceId)
            throw new InvalidOperationException("A resource cannot link to itself.");
    }

    /// <summary>
    /// Creates a new <see cref="InputOutputLink"/> between two resources after validating
    /// that both the output and input types are defined members of their respective enums.
    /// </summary>
    public static InputOutputLink Create<TInput, TOutput>(AzureResource source,
        AzureResource target,
        string outputType,
        string inputType) where TInput : Enum where TOutput : Enum
    {
        if (!Enum.IsDefined(typeof(TOutput), outputType))
            throw new InvalidOperationException($"OutputType '{outputType}' is not defined in enum '{typeof(TOutput).Name}'.");
        
        if (!Enum.IsDefined(typeof(TInput), inputType))
            throw new InvalidOperationException($"InputType '{inputType}' is not defined in enum '{typeof(TInput).Name}'.");
        
        return new InputOutputLink(InputOutputId.CreateUnique(),source, target, outputType, inputType);
    }

    private InputOutputLink()
    {
    }
}