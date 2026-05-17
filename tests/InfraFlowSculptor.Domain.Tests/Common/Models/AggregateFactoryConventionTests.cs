using System.Reflection;
using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate;

namespace InfraFlowSculptor.Domain.Tests.Common.Models;

public sealed class AggregateFactoryConventionTests
{
    [Fact]
    public void Given_DomainAggregateRoots_When_InspectingFactoryConventions_Then_ExposeStaticCreateAndNonPublicParameterlessConstructor()
    {
        // Arrange
        var aggregateRootTypes = typeof(Project).Assembly
            .GetTypes()
            .Where(static type =>
                type is { IsClass: true, IsAbstract: false } &&
                type != typeof(AzureResource) &&
                InheritsFromGeneric(type, typeof(AggregateRoot<>)))
            .ToList();

        // Act
        var violations = aggregateRootTypes
            .Select(static type => DescribeViolation(
                type,
                type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static),
                type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null)))
            .Where(static violation => violation is not null)
            .Cast<string>()
            .OrderBy(static violation => violation)
            .ToList();

        // Assert
        violations.Should().BeEmpty(
            "aggregate roots must expose a static Create factory and keep their EF constructor non-public. Missing: {0}",
            string.Join(", ", violations));
    }

    private static bool InheritsFromGeneric(Type type, Type genericTypeDefinition)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return true;
            }
        }

        return false;
    }

    private static string? DescribeViolation(
        Type aggregateRootType,
        MethodInfo? createMethod,
        ConstructorInfo? parameterlessConstructor)
    {
        if (createMethod is null)
        {
            return $"{aggregateRootType.Name}: missing public static Create";
        }

        if (parameterlessConstructor is null)
        {
            return $"{aggregateRootType.Name}: missing parameterless constructor";
        }

        return parameterlessConstructor.IsPublic
            ? $"{aggregateRootType.Name}: parameterless constructor must be non-public"
            : null;
    }
}
