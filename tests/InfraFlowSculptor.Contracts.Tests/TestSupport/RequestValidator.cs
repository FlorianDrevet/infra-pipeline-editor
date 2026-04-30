using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Tests.TestSupport;

internal static class RequestValidator
{
    public static IList<ValidationResult> Validate(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }

    /// <summary>
    /// Returns true when at least one validation result either lists <paramref name="memberName"/>
    /// in its <see cref="ValidationResult.MemberNames"/> or mentions it in the error message.
    /// Custom attributes in this codebase often emit <see cref="ValidationResult"/> instances
    /// with empty <see cref="ValidationResult.MemberNames"/>, so this helper centralises the
    /// fallback to the message text.
    /// </summary>
    public static bool HasErrorForMember(this IEnumerable<ValidationResult> results, string memberName)
    {
        return results.Any(result =>
            result.MemberNames.Contains(memberName)
            || (result.ErrorMessage is not null
                && result.ErrorMessage.Contains(memberName, StringComparison.OrdinalIgnoreCase)));
    }
}

