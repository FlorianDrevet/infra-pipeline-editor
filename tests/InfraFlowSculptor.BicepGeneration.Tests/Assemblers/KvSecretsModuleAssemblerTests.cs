using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class KvSecretsModuleAssemblerTests
{
    [Fact]
    public void Given_KvSecretsModule_When_Generate_Then_SecretUrisOutputIsComputedFromInputSecrets()
    {
        // Act
        var result = KvSecretsModuleAssembler.Generate();

        // Assert
        result.Should().Contain("output secretUris object = toObject(secrets, secret => secret.name, secret => '${keyVault.properties.vaultUri}secrets/${secret.name}')");
        result.Should().NotContain("toObject(kvSecrets");
        result.Should().NotContain("kv.properties.secretUri");
        result.Should().NotContain("kv.name");
    }
}
