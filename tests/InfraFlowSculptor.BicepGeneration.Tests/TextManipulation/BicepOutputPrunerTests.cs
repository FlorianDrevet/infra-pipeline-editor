using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepOutputPrunerTests
{
    // ── CollectUsedOutputsByPath ──

    [Fact]
    public void Given_MainBicepWithModuleOutputRefs_When_CollectUsedOutputsByPath_Then_ReturnsGroupedOutputs()
    {
        // Arrange
        const string mainBicep = """
            module kvModule './modules/KeyVault/kv.module.bicep' = {
              params: {
                location: location
              }
            }

            module redisModule './modules/RedisCache/redis.module.bicep' = {
              params: {
                location: location
              }
            }

            var kvUri = kvModule.outputs.vaultUri
            var redisHost = redisModule.outputs.hostName
            var kvName = kvModule.outputs.name
            """;

        var moduleFiles = new Dictionary<string, string>
        {
            ["modules/KeyVault/kv.module.bicep"] = "// kv module content",
            ["modules/RedisCache/redis.module.bicep"] = "// redis module content",
        };

        // Act
        var result = BicepOutputPruner.CollectUsedOutputsByPath(mainBicep, moduleFiles);

        // Assert
        result.Should().ContainKey("modules/KeyVault/kv.module.bicep");
        result["modules/KeyVault/kv.module.bicep"].Should().Contain("vaultUri");
        result["modules/KeyVault/kv.module.bicep"].Should().Contain("name");
        result.Should().ContainKey("modules/RedisCache/redis.module.bicep");
        result["modules/RedisCache/redis.module.bicep"].Should().Contain("hostName");
    }

    [Fact]
    public void Given_MainBicepWithNoModuleRefs_When_CollectUsedOutputsByPath_Then_ReturnsEmpty()
    {
        // Arrange
        const string mainBicep = """
            param location string
            var name = 'myProject'
            """;
        var moduleFiles = new Dictionary<string, string>();

        // Act
        var result = BicepOutputPruner.CollectUsedOutputsByPath(mainBicep, moduleFiles);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Given_MainBicepWithUndeclaredModule_When_CollectUsedOutputsByPath_Then_SkipsOrphanRef()
    {
        // Arrange — references a module that has no declaration
        const string mainBicep = """
            var value = orphanModule.outputs.something
            """;
        var moduleFiles = new Dictionary<string, string>();

        // Act
        var result = BicepOutputPruner.CollectUsedOutputsByPath(mainBicep, moduleFiles);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Given_DuplicateOutputRefs_When_CollectUsedOutputsByPath_Then_DeduplicatesPerPath()
    {
        // Arrange
        const string mainBicep = """
            module kvModule './modules/KeyVault/kv.module.bicep' = {}

            var a = kvModule.outputs.vaultUri
            var b = kvModule.outputs.vaultUri
            """;
        var moduleFiles = new Dictionary<string, string>
        {
            ["modules/KeyVault/kv.module.bicep"] = "",
        };

        // Act
        var result = BicepOutputPruner.CollectUsedOutputsByPath(mainBicep, moduleFiles);

        // Assert
        result["modules/KeyVault/kv.module.bicep"].Should().HaveCount(1);
        result["modules/KeyVault/kv.module.bicep"].Should().Contain("vaultUri");
    }
}
