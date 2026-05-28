using InfraFlowSculptor.Application.Common.Interfaces.Catalogs;
using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;
using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Infrastructure.Catalogs;

/// <summary>
/// In-memory implementation of the test framework catalog.
/// Aggregates all per-stack static definitions into a single queryable source.
/// </summary>
public sealed class TestFrameworkCatalog : ITestFrameworkCatalog
{
    private static readonly IReadOnlyList<TestFrameworkDefinition> AllDefinitions = BuildAll();

    private static readonly Dictionary<ApplicationStack.ApplicationStackEnum, IReadOnlyList<TestFrameworkDefinition>> ByStack =
        AllDefinitions
            .GroupBy(d => d.Key.Stack)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TestFrameworkDefinition>)g.ToList());

    private static readonly Dictionary<(ApplicationStack.ApplicationStackEnum, string), TestFrameworkDefinition> ByKey =
        AllDefinitions.ToDictionary(d => (d.Key.Stack, d.Key.Framework));

    /// <inheritdoc />
    public IReadOnlyList<TestFrameworkDefinition> GetFrameworksForStack(ApplicationStack.ApplicationStackEnum stack)
        => ByStack.TryGetValue(stack, out var list) ? list : [];

    /// <inheritdoc />
    public TestFrameworkDefinition? GetByKey(TestFrameworkKey key)
        => ByKey.TryGetValue((key.Stack, key.Framework), out var def) ? def : null;

    /// <inheritdoc />
    public IReadOnlyList<ApplicationStack.ApplicationStackEnum> GetSupportedStacks()
        => ByStack.Keys.ToList();

    /// <inheritdoc />
    public IReadOnlyList<TestFrameworkDefinition> GetAll() => AllDefinitions;

    private static IReadOnlyList<TestFrameworkDefinition> BuildAll()
    {
        var all = new List<TestFrameworkDefinition>();
        all.AddRange(DotNetTestFrameworks.All);
        all.AddRange(NodeJsTestFrameworks.All);
        all.AddRange(AngularTestFrameworks.All);
        all.AddRange(JavaTestFrameworks.All);
        all.AddRange(PythonTestFrameworks.All);
        return all;
    }
}
