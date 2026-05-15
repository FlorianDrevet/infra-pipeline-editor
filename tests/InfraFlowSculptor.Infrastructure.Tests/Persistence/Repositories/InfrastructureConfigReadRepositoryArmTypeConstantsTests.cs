using FluentAssertions;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class InfrastructureConfigReadRepositoryArmTypeConstantsTests
{
    [Fact]
    public void Given_MapResource_When_CheckingTargetedArmTypeMappings_Then_UsesAzureResourceTypeConstants()
    {
        // Arrange
        var repositoryRootPath = GetRepositoryRootPath();
        var repositoryFilePath = Path.Combine(
            repositoryRootPath,
            "src",
            "Api",
            "InfraFlowSculptor.Infrastructure",
            "Persistence",
            "Repositories",
            "InfrastructureConfigReadRepository.cs");

        // Act
        var fileContent = File.ReadAllText(repositoryFilePath);

        // Assert
        foreach (var forbiddenLiteral in GetForbiddenArmTypeLiterals())
        {
            fileContent.Should().NotContain(forbiddenLiteral);
        }

        foreach (var requiredConstant in GetRequiredArmTypeConstants())
        {
            fileContent.Should().Contain(requiredConstant);
        }
    }

    private static IReadOnlyList<string> GetForbiddenArmTypeLiterals()
    {
        return
        [
            "\"Microsoft.KeyVault/vaults\"",
            "\"Microsoft.App/containerApps\"",
            "\"Microsoft.DocumentDB/databaseAccounts\"",
            "\"Microsoft.ServiceBus/namespaces\"",
            "\"Microsoft.EventHub/namespaces\"",
        ];
    }

    private static IReadOnlyList<string> GetRequiredArmTypeConstants()
    {
        return
        [
            "AzureResourceTypes.ArmTypes.KeyVaultType",
            "AzureResourceTypes.ArmTypes.ContainerAppType",
            "AzureResourceTypes.ArmTypes.CosmosDbType",
            "AzureResourceTypes.ArmTypes.ServiceBusNamespaceType",
            "AzureResourceTypes.ArmTypes.EventHubNamespaceType",
        ];
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
}