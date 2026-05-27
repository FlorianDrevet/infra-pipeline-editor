using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>Identifies the Java build tool used by application pipeline behavior.</summary>
public sealed class JavaBuildTool : EnumValueObject<JavaBuildTool.JavaBuildToolType>
{
    /// <summary>Initializes a new instance of the <see cref="JavaBuildTool"/> class.</summary>
    /// <param name="value">Java build tool value.</param>
    public JavaBuildTool(JavaBuildToolType value) : base(value)
    {
    }

    private JavaBuildTool() : base()
    {
    }

    /// <summary>Available Java build tools.</summary>
    public enum JavaBuildToolType
    {
        /// <summary>Maven build tool.</summary>
        Maven,

        /// <summary>Gradle build tool.</summary>
        Gradle,
    }

    /// <summary>Maven build tool.</summary>
    public static readonly JavaBuildTool Maven = new(JavaBuildToolType.Maven);

    /// <summary>Gradle build tool.</summary>
    public static readonly JavaBuildTool Gradle = new(JavaBuildToolType.Gradle);
}