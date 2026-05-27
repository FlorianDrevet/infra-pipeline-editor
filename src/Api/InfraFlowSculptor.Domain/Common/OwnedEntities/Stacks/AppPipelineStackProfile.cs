namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Base type for typed stack-specific application pipeline profiles.</summary>
public abstract class AppPipelineStackProfile
{
    /// <summary>Initializes a new instance of the <see cref="AppPipelineStackProfile"/> class.</summary>
    private protected AppPipelineStackProfile()
    {
    }

    /// <summary>Gets the application stack supported by this profile.</summary>
    public abstract ApplicationStack Stack { get; }
}