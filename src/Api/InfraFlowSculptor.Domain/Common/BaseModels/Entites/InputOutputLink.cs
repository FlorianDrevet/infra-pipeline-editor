using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

public sealed class InputOutputLink : Entity<InputOutputId>
{
    public AzureResource SourceResource { get; private set; } = null!;
    public AzureResourceId SourceResourceId { get; private set; }

    public AzureResource TargetResource { get; private set; } = null!;
    public AzureResourceId TargetResourceId { get; private set; }

    public string OutputType { get; private set; }
    public string InputType { get; private set; }

    private InputOutputLink(InputOutputId id,AzureResource source,
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
            throw new InvalidOperationException("Une ressource ne peut pas se lier à elle-même.");
    }

    public static InputOutputLink Create<TInput, TOutput> (AzureResource source,
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