using FluentAssertions;

namespace InfraFlowSculptor.GenerationCore.Tests.Architecture;

public sealed class GenerationCoreBoundaryTests
{
    private const string RootNamespace = "InfraFlowSculptor.GenerationCore";
    private const string ModelsNamespacePrefix = RootNamespace + ".Models";
    private const string ErrorsNamespacePrefix = RootNamespace + ".Errors";
    private static readonly HashSet<string> ApprovedRootHelperTypeNames = new(StringComparer.Ordinal)
    {
        nameof(AzureResourceDefaults),
        nameof(AzureResourceTypes),
        nameof(BicepIdentifierNormalizer),
        nameof(KeyVaultSecretNameRules),
        nameof(PathSanitizer),
    };
    private static readonly string[] ForbiddenImplementationTypeSuffixes =
    [
        "Assembler",
        "Engine",
        "Generator",
    ];
    private static readonly string[] ForbiddenImplementationNamespaceSegments =
    [
        "Assemblers",
        "Engines",
        "Generators",
    ];

    [Fact]
    public void Given_GenerationCoreAssembly_When_InspectingTopLevelTypes_Then_ContainsOnlyContractsModelsErrorsAndApprovedStatelessHelpers()
    {
        // Arrange
        var generationCoreAssembly = typeof(IGenerationResult).Assembly;

        // Act
        var violatingTypes = generationCoreAssembly
            .GetTypes()
            .Where(IsUserDefinedTopLevelType)
            .Where(type => !IsApprovedBoundaryType(type))
            .Select(static type => type.FullName)
            .OrderBy(static fullName => fullName, StringComparer.Ordinal)
            .ToArray();

        // Assert
        violatingTypes.Should().BeEmpty(
            "GenerationCore is intentionally limited to shared contracts, models, errors, and approved stateless helpers. Offending types: {0}",
            string.Join(", ", violatingTypes));
    }

    [Fact]
    public void Given_GenerationCoreAssembly_When_InspectingTopLevelTypes_Then_DoesNotContainGeneratorAssemblerOrEngineImplementations()
    {
        // Arrange
        var generationCoreAssembly = typeof(IGenerationResult).Assembly;

        // Act
        var forbiddenImplementationTypes = generationCoreAssembly
            .GetTypes()
            .Where(IsUserDefinedTopLevelType)
            .Where(static type => type is { IsInterface: false, IsEnum: false })
            .Where(IsForbiddenImplementationType)
            .Select(static type => type.FullName)
            .OrderBy(static fullName => fullName, StringComparer.Ordinal)
            .ToArray();

        // Assert
        forbiddenImplementationTypes.Should().BeEmpty(
            "ARCH-003 is satisfied by keeping GenerationCore as a contracts-and-helpers assembly; generators, assemblers, and engines belong in dedicated generation assemblies. Offending types: {0}",
            string.Join(", ", forbiddenImplementationTypes));
    }

    private static bool IsInNamespace(Type type, string namespacePrefix)
    {
        return type.Namespace?.StartsWith(namespacePrefix, StringComparison.Ordinal) == true;
    }

    private static bool IsApprovedBoundaryType(Type type)
    {
        return type.IsInterface
            || type.IsEnum
            || IsInNamespace(type, ModelsNamespacePrefix)
            || IsInNamespace(type, ErrorsNamespacePrefix)
            || IsApprovedRootHelperType(type);
    }

    private static bool IsApprovedRootHelperType(Type type)
    {
        return string.Equals(type.Namespace, RootNamespace, StringComparison.Ordinal)
            && ApprovedRootHelperTypeNames.Contains(type.Name);
    }

    private static bool IsForbiddenImplementationType(Type type)
    {
        return ForbiddenImplementationTypeSuffixes.Any(suffix => type.Name.EndsWith(suffix, StringComparison.Ordinal))
            || type.Namespace?.Split('.').Any(segment => ForbiddenImplementationNamespaceSegments.Contains(segment, StringComparer.Ordinal)) == true;
    }

    private static bool IsUserDefinedTopLevelType(Type type)
    {
        return type.DeclaringType is null
            && type.Namespace?.StartsWith(RootNamespace, StringComparison.Ordinal) == true;
    }
}
