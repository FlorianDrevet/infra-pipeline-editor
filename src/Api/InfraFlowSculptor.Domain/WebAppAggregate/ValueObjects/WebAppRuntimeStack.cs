using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

/// <summary>Runtime stack for an Azure Web App.</summary>
public class WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum value)
    : EnumValueObject<WebAppRuntimeStack.WebAppRuntimeStackEnum>(value)
{
    public enum WebAppRuntimeStackEnum
    {
        DotNet,
        Node,
        Python,
        Java,
        Php,
    }
}
