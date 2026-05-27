using System.Diagnostics.CodeAnalysis;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

/// <summary>
/// Identifies the application stack used to drive application pipeline behavior.
/// This is distinct from Azure hosting runtime stacks such as WebAppRuntimeStack or FunctionAppRuntimeStack.
/// </summary>
public sealed class ApplicationStack : EnumValueObject<ApplicationStack.ApplicationStackEnum>
{
    /// <summary>Initializes a new instance of the <see cref="ApplicationStack"/> class.</summary>
    /// <param name="value">Application stack value.</param>
    public ApplicationStack(ApplicationStackEnum value) : base(value)
    {
    }

    private ApplicationStack() : base()
    {
    }

    /// <summary>Available application pipeline stacks.</summary>
    [SuppressMessage("Naming", "S2344:Enumeration names should not have \"Enum\" suffix", Justification = "ApplicationStackEnum is part of the public domain API for pipeline stack values.")]
    public enum ApplicationStackEnum // NOSONAR S2344
    {
        /// <summary>Application stack has not been selected yet.</summary>
        Unknown,

        /// <summary>.NET application pipeline behavior.</summary>
        DotNet,

        /// <summary>Node.js application pipeline behavior.</summary>
        NodeJs,

        /// <summary>Angular application pipeline behavior.</summary>
        Angular,

        /// <summary>Java application pipeline behavior.</summary>
        Java,

        /// <summary>Python application pipeline behavior.</summary>
        Python,

        /// <summary>PHP application pipeline behavior reserved for a future specialized profile.</summary>
        Php,

        /// <summary>Go application pipeline behavior reserved for a future specialized profile.</summary>
        Go,

        /// <summary>Static site application pipeline behavior.</summary>
        StaticSite,

        /// <summary>Advanced custom application pipeline behavior.</summary>
        Custom,
    }

    /// <summary>Application stack has not been selected yet.</summary>
    public static readonly ApplicationStack Unknown = new(ApplicationStackEnum.Unknown);

    /// <summary>.NET application pipeline behavior.</summary>
    public static readonly ApplicationStack DotNet = new(ApplicationStackEnum.DotNet);

    /// <summary>Node.js application pipeline behavior.</summary>
    public static readonly ApplicationStack NodeJs = new(ApplicationStackEnum.NodeJs);

    /// <summary>Angular application pipeline behavior.</summary>
    public static readonly ApplicationStack Angular = new(ApplicationStackEnum.Angular);

    /// <summary>Java application pipeline behavior.</summary>
    public static readonly ApplicationStack Java = new(ApplicationStackEnum.Java);

    /// <summary>Python application pipeline behavior.</summary>
    public static readonly ApplicationStack Python = new(ApplicationStackEnum.Python);

    /// <summary>PHP application pipeline behavior reserved for a future specialized profile.</summary>
    public static readonly ApplicationStack Php = new(ApplicationStackEnum.Php);

    /// <summary>Go application pipeline behavior reserved for a future specialized profile.</summary>
    public static readonly ApplicationStack Go = new(ApplicationStackEnum.Go);

    /// <summary>Static site application pipeline behavior.</summary>
    public static readonly ApplicationStack StaticSite = new(ApplicationStackEnum.StaticSite);

    /// <summary>Advanced custom application pipeline behavior.</summary>
    public static readonly ApplicationStack Custom = new(ApplicationStackEnum.Custom);
}