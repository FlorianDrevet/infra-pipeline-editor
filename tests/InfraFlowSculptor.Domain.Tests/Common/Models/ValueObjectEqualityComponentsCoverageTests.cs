using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.Common.Models;

public sealed class ValueObjectEqualityComponentsCoverageTests
{
    [Fact]
    public void Given_SingleValueObjectDerivative_When_DiscoveringMeaningfulProperties_Then_IncludesInheritedValueProperty()
    {
        // Arrange
        var meaningfulProperties = GetMeaningfulPublicInstanceProperties(typeof(TenantId));

        // Act
        var propertyNames = meaningfulProperties
            .Select(property => property.Name)
            .ToArray();

        // Assert
        propertyNames.Should().Contain(nameof(SingleValueObject<Guid>.Value));
    }

    [Fact]
    public void Given_ValueObjectWithComputedProperty_When_DiscoveringMeaningfulProperties_Then_ExcludesComputedMembers()
    {
        // Arrange
        var meaningfulProperties = GetMeaningfulPublicInstanceProperties(typeof(RepositoryContentKinds));

        // Act
        var propertyNames = meaningfulProperties
            .Select(property => property.Name)
            .ToArray();

        // Assert
        propertyNames.Should().Contain(nameof(RepositoryContentKinds.Value));
        propertyNames.Should().NotContain(nameof(RepositoryContentKinds.Flags));
    }

    [Theory]
    [MemberData(nameof(GetCoveredValueObjectTypes))]
    public void Given_ConcreteValueObjectType_When_A_MeaningfulPublicPropertyChanges_Then_StructuralEqualityChanges(Type valueObjectType)
    {
        // Arrange
        var meaningfulProperties = GetMeaningfulPublicInstanceProperties(valueObjectType).ToArray();

        // Act
        foreach (var property in meaningfulProperties)
        {
            var baseline = CreateValueObject(valueObjectType, meaningfulProperties, propertyToChange: null);
            var changed = CreateValueObject(valueObjectType, meaningfulProperties, propertyToChange: property);

            // Assert
            baseline.Equals(changed).Should().BeFalse($"{valueObjectType.Name}.{property.Name} must participate in structural equality.");
            changed.Equals(baseline).Should().BeFalse($"{valueObjectType.Name}.{property.Name} must participate in structural equality.");
        }
    }

    public static IEnumerable<object[]> GetCoveredValueObjectTypes()
    {
        return typeof(ValueObject).Assembly
            .GetTypes()
            .Where(IsCoveredValueObjectType)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => new object[] { type });
    }

    private static bool IsCoveredValueObjectType(Type type)
    {
        return type.IsClass
            && !type.IsAbstract
            && !type.ContainsGenericParameters
            && !type.IsNested
            && typeof(ValueObject).IsAssignableFrom(type)
            && GetMeaningfulPublicInstanceProperties(type).Count != 0;
    }

    private static IReadOnlyList<PropertyInfo> GetMeaningfulPublicInstanceProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod is not null && !property.GetMethod.IsStatic)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Where(property => FindCompilerGeneratedBackingField(property) is not null || property.GetSetMethod(nonPublic: true) is not null)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static ValueObject CreateValueObject(
        Type valueObjectType,
        IReadOnlyList<PropertyInfo> meaningfulProperties,
        PropertyInfo? propertyToChange)
    {
        var instance = (ValueObject)RuntimeHelpers.GetUninitializedObject(valueObjectType);

        for (var index = 0; index < meaningfulProperties.Count; index++)
        {
            var property = meaningfulProperties[index];
            var variant = property == propertyToChange ? 1 : 0;
            var value = CreateDeterministicValue(property.PropertyType, property.Name, index, variant);

            AssignPropertyValue(instance, property, value);
        }

        return instance;
    }

    private static object CreateDeterministicValue(Type propertyType, string propertyName, int index, int variant)
    {
        var underlyingNullableType = Nullable.GetUnderlyingType(propertyType);
        if (underlyingNullableType is not null)
        {
            return CreateDeterministicValue(underlyingNullableType, propertyName, index, variant);
        }

        if (propertyType == typeof(string))
        {
            return $"{propertyName}-{index}-{variant}";
        }

        if (propertyType == typeof(Guid))
        {
            var bytes = new byte[16];
            bytes[0] = (byte)(index + 1);
            bytes[15] = (byte)(variant + 1);
            return new Guid(bytes);
        }

        if (propertyType == typeof(bool))
        {
            return variant == 1;
        }

        if (propertyType == typeof(int))
        {
            return (index + 1) * 100 + variant;
        }

        if (propertyType == typeof(long))
        {
            return (long)((index + 1) * 1000 + variant);
        }

        if (propertyType == typeof(decimal))
        {
            return (decimal)((index + 1) * 10 + variant);
        }

        if (propertyType == typeof(DateTime))
        {
            return new DateTime(2030, 1, 1, 0, 0, variant, DateTimeKind.Utc).AddDays(index);
        }

        if (propertyType == typeof(DateTimeOffset))
        {
            return new DateTimeOffset(2030, 1, 1, 0, 0, variant, TimeSpan.Zero).AddDays(index);
        }

        if (propertyType.IsEnum)
        {
            return Enum.ToObject(propertyType, variant + index + 1);
        }

        if (IsEnumValueObjectType(propertyType, out var enumType))
        {
            var enumValue = Enum.ToObject(enumType, variant + index + 1);
            return Activator.CreateInstance(propertyType, enumValue)!;
        }

        if (typeof(ValueObject).IsAssignableFrom(propertyType))
        {
            var stringCtor = propertyType.GetConstructor([typeof(string)]);
            if (stringCtor is not null)
            {
                return stringCtor.Invoke([$"10.{index}.{variant}.0/16"]);
            }
        }

        throw new NotSupportedException($"The value-object equality guardrail does not support property type '{propertyType.FullName}'.");
    }

    private static void AssignPropertyValue(ValueObject instance, PropertyInfo property, object value)
    {
        var backingField = FindCompilerGeneratedBackingField(property);
        if (backingField is not null)
        {
            backingField.SetValue(instance, value);
            return;
        }

        var setMethod = property.GetSetMethod(nonPublic: true)
            ?? throw new InvalidOperationException($"No writable path was found for property '{property.DeclaringType?.FullName}.{property.Name}'.");

        setMethod.Invoke(instance, [value]);
    }

    private static FieldInfo? FindCompilerGeneratedBackingField(PropertyInfo property)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        for (var declaringType = property.DeclaringType; declaringType is not null; declaringType = declaringType.BaseType)
        {
            var backingField = declaringType.GetField($"<{property.Name}>k__BackingField", flags);
            if (backingField is not null)
            {
                return backingField;
            }
        }

        return null;
    }

    private static bool IsEnumValueObjectType(Type type, out Type enumType)
    {
        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(EnumValueObject<>))
            {
                enumType = baseType.GetGenericArguments()[0];
                return true;
            }
        }

        enumType = null!;
        return false;
    }
}
