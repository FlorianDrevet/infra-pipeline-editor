using FluentAssertions;

namespace InfraFlowSculptor.BicepGeneration.Tests.Harness;

public sealed class RepresentativeBicepHarnessTests
{
    [Fact]
    public void Given_RepresentativeHarness_When_GenerateFilesTwice_Then_FileMapIsDeterministic()
    {
        // Arrange
        IReadOnlyDictionary<string, string> firstRun = RepresentativeBicepHarness.GenerateFiles();
        IReadOnlyDictionary<string, string> secondRun = RepresentativeBicepHarness.GenerateFiles();

        // Assert
        firstRun.Should().BeEquivalentTo(secondRun);
    }

    [Fact]
    public void Given_TemporaryOutputDirectory_When_WriteFiles_Then_WritesExpectedBicepTree()
    {
        // Arrange
        var outputDirectory = RepresentativeBicepHarness.ResolveOutputDirectory();
        var preserveOutputDirectory = RepresentativeBicepHarness.ShouldPreserveConfiguredOutputDirectory();

        try
        {
            // Act
            var files = RepresentativeBicepHarness.WriteFiles(outputDirectory);

            // Assert
            files.Should().NotBeEmpty();
            File.Exists(Path.Combine(outputDirectory, "main.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "types.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "functions.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "constants.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "AppServicePlan", "appServicePlan.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "AppServicePlan", "types.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "KeyVault", "keyVault.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "KeyVault", "types.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "KeyVault", "kvSecrets.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "KeyVault", "keyvault.roleassignments.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "UserAssignedIdentity", "userAssignedIdentity.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "WebApp", "webApp.module.bicep")).Should().BeTrue();
            File.Exists(Path.Combine(outputDirectory, "modules", "WebApp", "types.bicep")).Should().BeTrue();
        }
        finally
        {
            if (!preserveOutputDirectory && Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}