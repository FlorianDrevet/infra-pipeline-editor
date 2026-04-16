using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

/// <summary>Runtime stack for an Azure Function App.</summary>
public sealed class FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum value)
    : EnumValueObject<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(value)
{
    /// <summary>Supported Function App runtime stacks.</summary>
    public enum FunctionAppRuntimeStackEnum
    {
        /// <summary>.NET runtime.</summary>
        DotNet,

        /// <summary>Node.js runtime.</summary>
        Node,

        /// <summary>Python runtime.</summary>
        Python,

        /// <summary>Java runtime.</summary>
        Java,

        /// <summary>PowerShell runtime.</summary>
        PowerShell,
    }
}
