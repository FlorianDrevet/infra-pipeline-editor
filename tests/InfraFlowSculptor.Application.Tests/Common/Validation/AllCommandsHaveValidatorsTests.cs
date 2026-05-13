using FluentAssertions;
using FluentValidation;
using InfraFlowSculptor.Application.Common.Interfaces;
using System.Reflection;

namespace InfraFlowSculptor.Application.Tests.Common.Validation;

public sealed class AllCommandsHaveValidatorsTests
{
    [Fact]
    public void Given_ApplicationCommands_When_ScanningValidators_Then_EachCommandHasValidator()
    {
        // Arrange
        var applicationAssembly = typeof(ICommand<>).Assembly;
        var commandTypes = applicationAssembly
            .GetTypes()
            .Where(IsConcreteCommandType)
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        // Act
        var missingValidators = commandTypes
            .Where(commandType => !HasValidator(applicationAssembly, commandType))
            .Select(static commandType => commandType.FullName)
            .ToArray();

        // Assert
        missingValidators.Should().BeEmpty(
            "every application command must be covered by a FluentValidation validator. Missing validators: {0}",
            string.Join(", ", missingValidators));
    }

    private static bool IsConcreteCommandType(Type type)
    {
        return type is { IsAbstract: false, IsInterface: false }
            && type.GetInterfaces().Any(
                static @interface => @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    private static bool HasValidator(Assembly applicationAssembly, Type commandType)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(commandType);

        return applicationAssembly
            .GetTypes()
            .Any(type => type is { IsAbstract: false, IsInterface: false } && validatorType.IsAssignableFrom(type));
    }
}