using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GuidValidation : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not Guid guid)
        {
            return false;
        }

        return guid != Guid.Empty;
    }
}