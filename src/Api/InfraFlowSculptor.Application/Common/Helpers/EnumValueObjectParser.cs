using ErrorOr;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Helpers;

internal static class EnumValueObjectParser
{
    public static ErrorOr<TValueObject> Parse<TEnum, TValueObject>(
        string raw,
        Func<TEnum, TValueObject> factory,
        Func<string, Error> invalidError)
        where TEnum : struct, Enum
        where TValueObject : EnumValueObject<TEnum>
    {
        if (!Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed))
            return invalidError(raw);

        return factory(parsed);
    }

    public static ErrorOr<TValueObject?> ParseOrNull<TEnum, TValueObject>(
        string? raw,
        Func<TEnum, TValueObject> factory,
        Func<string, Error> invalidError)
        where TEnum : struct, Enum
        where TValueObject : EnumValueObject<TEnum>
    {
        if (raw is null)
            return (TValueObject?)null;

        if (!Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed))
            return invalidError(raw);

        return factory(parsed);
    }
}
