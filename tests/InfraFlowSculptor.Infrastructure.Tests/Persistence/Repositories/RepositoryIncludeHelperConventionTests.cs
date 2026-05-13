using FluentAssertions;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class RepositoryIncludeHelperConventionTests
{
    [Fact]
    public void Given_TargetedRepositories_When_CheckingIncludeConsolidation_Then_DuplicatedIncludeChainsAreCentralizedInLocalHelpers()
    {
        // Arrange
        var repositoryRootPath = GetRepositoryRootPath();
        var repositoriesDirectoryPath = Path.Combine(repositoryRootPath, "src", "Api", "InfraFlowSculptor.Infrastructure", "Persistence", "Repositories");

        // Act
        var violations = GetExpectations()
            .SelectMany(expectation => GetViolations(repositoriesDirectoryPath, expectation))
            .ToArray();

        // Assert
        violations.Should().BeEmpty("DB-007 requires duplicated Include chains to be centralized behind local repository helpers for the targeted repositories.");
    }

    private static IReadOnlyList<RepositoryIncludeExpectation> GetExpectations()
    {
        return
        [
            new RepositoryIncludeExpectation(
                "WebAppRepository.cs",
                ["private static IQueryable<WebApp> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "SqlServerRepository.cs",
                ["private static IQueryable<SqlServer> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "SqlDatabaseRepository.cs",
                ["private static IQueryable<SqlDatabase> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "FunctionAppRepository.cs",
                ["private static IQueryable<FunctionApp> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "AppServicePlanRepository.cs",
                ["private static IQueryable<AppServicePlan> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "ApplicationInsightsRepository.cs",
                ["private static IQueryable<ApplicationInsights> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "ContainerAppRepository.cs",
                ["private static IQueryable<ContainerApp> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "ContainerAppEnvironmentRepository.cs",
                ["private static IQueryable<ContainerAppEnvironment> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "ContainerRegistryRepository.cs",
                ["private static IQueryable<ContainerRegistry> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "CosmosDbRepository.cs",
                ["private static IQueryable<CosmosDb> WithSubResources"],
                [".Include(c => c.DependsOn)", ".Include(c => c.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "KeyVaultRepository.cs",
                ["private static IQueryable<KeyVault> WithSubResources"],
                [".Include(kv => kv.DependsOn)", ".Include(kv => kv.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "LogAnalyticsWorkspaceRepository.cs",
                ["private static IQueryable<LogAnalyticsWorkspace> WithSubResources"],
                [".Include(x => x.DependsOn)", ".Include(x => x.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "RedisCacheRepository.cs",
                ["private static IQueryable<RedisCache> WithSubResources"],
                [".Include(rc => rc.DependsOn)", ".Include(rc => rc.EnvironmentSettings)"]),
            new RepositoryIncludeExpectation(
                "EventHubNamespaceRepository.cs",
                ["private static IQueryable<EventHubNamespace> WithSubResources"],
                [
                    ".Include(eh => eh.DependsOn)",
                    ".Include(eh => eh.EnvironmentSettings)",
                    ".Include(eh => eh.EventHubs)",
                    ".Include(eh => eh.ConsumerGroups)",
                ]),
            new RepositoryIncludeExpectation(
                "ServiceBusNamespaceRepository.cs",
                ["private static IQueryable<ServiceBusNamespace> WithSubResources"],
                [
                    ".Include(sb => sb.DependsOn)",
                    ".Include(sb => sb.EnvironmentSettings)",
                    ".Include(sb => sb.Queues)",
                    ".Include(sb => sb.TopicSubscriptions)",
                ]),
            new RepositoryIncludeExpectation(
                "AppConfigurationRepository.cs",
                [
                    "private static IQueryable<AppConfiguration> WithSubResources",
                    "private static IQueryable<AppConfiguration> WithConfigurationKeys",
                ],
                [
                    ".Include(ac => ac.DependsOn)",
                    ".Include(ac => ac.EnvironmentSettings)",
                    ".Include(ac => ac.ConfigurationKeys)",
                    ".ThenInclude(ck => ck.EnvironmentValues)",
                ]),
        ];
    }

    private static IEnumerable<string> GetViolations(string repositoriesDirectoryPath, RepositoryIncludeExpectation expectation)
    {
        var filePath = Path.Combine(repositoriesDirectoryPath, expectation.FileName);
        if (!File.Exists(filePath))
        {
            yield return $"Missing targeted repository file '{expectation.FileName}'.";
            yield break;
        }

        var fileContent = File.ReadAllText(filePath);

        foreach (var requiredHelperSignature in expectation.RequiredHelperSignatures)
        {
            if (!fileContent.Contains(requiredHelperSignature, StringComparison.Ordinal))
            {
                yield return $"Repository '{expectation.FileName}' must define local helper '{requiredHelperSignature}'.";
            }
        }

        foreach (var includeSnippet in expectation.UniqueIncludeSnippets)
        {
            var occurrenceCount = CountOccurrences(fileContent, includeSnippet);
            if (occurrenceCount != 1)
            {
                yield return $"Repository '{expectation.FileName}' should contain include snippet '{includeSnippet}' exactly once, but found {occurrenceCount}.";
            }
        }
    }

    private static int CountOccurrences(string fileContent, string snippet)
    {
        var count = 0;
        var searchIndex = 0;

        while (true)
        {
            var matchIndex = fileContent.IndexOf(snippet, searchIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            searchIndex = matchIndex + snippet.Length;
        }
    }

    private static string GetRepositoryRootPath()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var solutionFilePath = Path.Combine(currentDirectory.FullName, "InfraFlowSculptor.slnx");
            if (File.Exists(solutionFilePath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root from the test execution directory.");
    }

    private sealed record RepositoryIncludeExpectation(
        string FileName,
        IReadOnlyList<string> RequiredHelperSignatures,
        IReadOnlyList<string> UniqueIncludeSnippets);
}