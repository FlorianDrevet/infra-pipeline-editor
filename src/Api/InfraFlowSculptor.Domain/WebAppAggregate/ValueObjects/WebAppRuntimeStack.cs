using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

/// <summary>Runtime stack for an Azure Web App.</summary>
public class WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum value)
    : EnumValueObject<WebAppRuntimeStack.WebAppRuntimeStackEnum>(value)
{
    /// <summary>Available runtime stack types.</summary>
    public enum WebAppRuntimeStackEnum
    {
        /// <summary>.NET runtime stack.</summary>
        DotNet,

        /// <summary>Node.js runtime stack.</summary>
        Node,

        /// <summary>Python runtime stack.</summary>
        Python,

        /// <summary>Java runtime stack.</summary>
        Java,

        /// <summary>PHP runtime stack.</summary>
        Php,
    }
}
