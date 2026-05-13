using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Responses;
using Xunit.Sdk;

namespace InfraFlowSculptor.Contracts.Tests.Responses;

public sealed class ContractsResponseShapeSnapshotTests
{
    private static readonly IReadOnlyDictionary<Type, string> TypeAliases = new Dictionary<Type, string>
    {
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(Guid)] = "System.Guid",
        [typeof(int)] = "int",
        [typeof(long)] = "long",
        [typeof(object)] = "object",
        [typeof(short)] = "short",
        [typeof(string)] = "string",
        [typeof(DateTime)] = "System.DateTime",
        [typeof(DateTimeOffset)] = "System.DateTimeOffset",
    };

    [Fact]
    public void Given_ContractsResponseTypes_When_BuildingPublicShapeSnapshot_Then_MatchesApprovedSchema()
    {
        // Arrange
        var snapshot = BuildSnapshot();
        var approvedSnapshotPath = GetApprovedSnapshotPath();
        var receivedSnapshotPath = GetReceivedSnapshotPath();

        // Act
        if (!File.Exists(approvedSnapshotPath))
        {
            File.WriteAllText(receivedSnapshotPath, snapshot);
            throw new XunitException($"Approved snapshot is missing. Review '{receivedSnapshotPath}' and promote it to '{approvedSnapshotPath}'.");
        }

        var approvedSnapshot = File.ReadAllText(approvedSnapshotPath);
        var normalizedSnapshot = NormalizeSnapshot(snapshot);
        var normalizedApprovedSnapshot = NormalizeSnapshot(approvedSnapshot);

        if (!string.Equals(normalizedSnapshot, normalizedApprovedSnapshot, StringComparison.Ordinal))
        {
            File.WriteAllText(receivedSnapshotPath, snapshot);
            normalizedSnapshot.Should().Be(normalizedApprovedSnapshot, "the public Contracts response DTO shape should stay intentional and reviewable.");
        }

        if (File.Exists(receivedSnapshotPath))
        {
            File.Delete(receivedSnapshotPath);
        }
    }

    private static string BuildSnapshot()
    {
        var nullabilityContext = new NullabilityInfoContext();
        var responseTypes = typeof(ProjectResponse).Assembly
            .GetTypes()
            .Where(IsResponseType)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        var builder = new StringBuilder();
        builder.AppendLine("Contracts response DTO public shape snapshot");
        builder.AppendLine($"Count: {responseTypes.Length}");
        builder.AppendLine();

        foreach (var responseType in responseTypes)
        {
            builder.AppendLine($"Namespace: {responseType.Namespace}");
            builder.AppendLine($"Type: {responseType.Name}");
            builder.AppendLine("Constructors:");

            var constructors = responseType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(constructor => constructor.GetParameters().Length)
                .ThenBy(constructor => BuildConstructorSignature(constructor, nullabilityContext), StringComparer.Ordinal)
                .ToArray();

            if (constructors.Length == 0)
            {
                builder.AppendLine("  (none)");
            }
            else
            {
                foreach (var constructor in constructors)
                {
                    builder.AppendLine($"  {BuildConstructorSignature(constructor, nullabilityContext)}");
                }
            }

            builder.AppendLine("Properties:");

            var properties = responseType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .ToArray();

            if (properties.Length == 0)
            {
                builder.AppendLine("  (none)");
            }
            else
            {
                foreach (var property in properties)
                {
                    var nullabilityInfo = nullabilityContext.Create(property);
                    builder.AppendLine($"  {property.Name}: {FormatTypeDisplayName(property.PropertyType, nullabilityInfo)}");
                }
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static bool IsResponseType(Type type)
    {
        return type.IsPublic
            && !type.IsAbstract
            && !type.IsNested
            && !type.ContainsGenericParameters
            && type.Namespace is not null
            && type.Namespace.StartsWith("InfraFlowSculptor.Contracts.", StringComparison.Ordinal)
            && type.Namespace.EndsWith(".Responses", StringComparison.Ordinal);
    }

    private static string BuildConstructorSignature(ConstructorInfo constructor, NullabilityInfoContext nullabilityContext)
    {
        var parameters = constructor
            .GetParameters()
            .Select(parameter =>
            {
                var nullabilityInfo = nullabilityContext.Create(parameter);
                return $"{FormatTypeDisplayName(parameter.ParameterType, nullabilityInfo)} {parameter.Name}";
            });

        return $"{constructor.DeclaringType!.Name}({string.Join(", ", parameters)})";
    }

    private static string FormatTypeDisplayName(Type type, NullabilityInfo? nullabilityInfo)
    {
        var underlyingNullableType = Nullable.GetUnderlyingType(type);
        if (underlyingNullableType is not null)
        {
            var genericArgumentNullability = nullabilityInfo?.GenericTypeArguments.FirstOrDefault();
            return $"{FormatTypeDisplayName(underlyingNullableType, genericArgumentNullability)}?";
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()
                ?? throw new InvalidOperationException($"Array type '{type.FullName}' did not expose an element type.");

            return $"{FormatTypeDisplayName(elementType, nullabilityInfo?.ElementType)}[]";
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericTypeName = GetTypeNameWithoutGenericArity(genericTypeDefinition);
            var genericArguments = type.GetGenericArguments();
            var genericArgumentNullability = nullabilityInfo?.GenericTypeArguments ?? [];
            var formattedArguments = genericArguments
                .Select((argumentType, index) => FormatTypeDisplayName(argumentType, genericArgumentNullability.ElementAtOrDefault(index)))
                .ToArray();

            return AppendNullableSuffixIfNeeded(
                $"{genericTypeName}<{string.Join(", ", formattedArguments)}>",
                type,
                nullabilityInfo);
        }

        var resolvedTypeName = TypeAliases.TryGetValue(type, out var alias)
            ? alias
            : type.FullName?.Replace('+', '.') ?? type.Name;

        return AppendNullableSuffixIfNeeded(resolvedTypeName, type, nullabilityInfo);
    }

    private static string AppendNullableSuffixIfNeeded(string typeName, Type type, NullabilityInfo? nullabilityInfo)
    {
        if (!type.IsValueType && nullabilityInfo?.ReadState == NullabilityState.Nullable)
        {
            return $"{typeName}?";
        }

        return typeName;
    }

    private static string GetTypeNameWithoutGenericArity(Type type)
    {
        var fullName = type.FullName?.Replace('+', '.') ?? type.Name;
        var genericTickIndex = fullName.IndexOf('`', StringComparison.Ordinal);
        return genericTickIndex >= 0
            ? fullName[..genericTickIndex]
            : fullName;
    }

    private static string NormalizeSnapshot(string content)
    {
        return content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .TrimEnd('\n');
    }

    private static string GetApprovedSnapshotPath([CallerFilePath] string sourceFilePath = "")
    {
        return Path.ChangeExtension(sourceFilePath, ".verified.txt");
    }

    private static string GetReceivedSnapshotPath([CallerFilePath] string sourceFilePath = "")
    {
        return Path.ChangeExtension(sourceFilePath, ".received.txt");
    }
}