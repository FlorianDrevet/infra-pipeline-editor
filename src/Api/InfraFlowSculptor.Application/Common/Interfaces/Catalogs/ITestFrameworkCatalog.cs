using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Application.Common.Interfaces.Catalogs;

/// <summary>
/// Provides access to the test framework catalog, filtered by application stack.
/// </summary>
public interface ITestFrameworkCatalog
{
    /// <summary>Returns all test framework definitions for the given application stack.</summary>
    /// <param name="stack">The application stack to filter by.</param>
    /// <returns>The ordered list of test frameworks compatible with the stack.</returns>
    IReadOnlyList<TestFrameworkDefinition> GetFrameworksForStack(ApplicationStack.ApplicationStackEnum stack);

    /// <summary>Returns a specific framework definition by its composite key, or null if not found.</summary>
    /// <param name="key">The composite key (Stack + Framework).</param>
    /// <returns>The matching definition, or null.</returns>
    TestFrameworkDefinition? GetByKey(TestFrameworkKey key);

    /// <summary>Returns all application stacks that have at least one test framework defined.</summary>
    IReadOnlyList<ApplicationStack.ApplicationStackEnum> GetSupportedStacks();

    /// <summary>Returns all test framework definitions across all stacks.</summary>
    IReadOnlyList<TestFrameworkDefinition> GetAll();
}
