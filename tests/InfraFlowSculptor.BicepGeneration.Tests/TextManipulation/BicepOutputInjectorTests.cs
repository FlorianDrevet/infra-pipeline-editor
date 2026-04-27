using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepOutputInjectorTests
{
    private const string MinimalModule = """
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {}
        }
        """;

    // ── Inject ──

    [Fact]
    public void Given_ModuleWithOutputs_When_Inject_Then_AppendsOutputDeclarations()
    {
        // Arrange
        var outputs = new[]
        {
            new ModuleOutputInjection("vaultUri", "kv.properties.vaultUri", IsSecure: false),
        };

        // Act
        var result = BicepOutputInjector.Inject(MinimalModule, outputs);

        // Assert
        result.Should().Contain("output vaultUri string = kv.properties.vaultUri");
    }

    [Fact]
    public void Given_SecureOutput_When_Inject_Then_AddsSecureDecorator()
    {
        // Arrange
        var outputs = new[]
        {
            new ModuleOutputInjection("secretValue", "kv.properties.secretUri", IsSecure: true),
        };

        // Act
        var result = BicepOutputInjector.Inject(MinimalModule, outputs);

        // Assert
        result.Should().Contain("@secure()");
        result.Should().Contain("output secretValue string = kv.properties.secretUri");
    }

    [Fact]
    public void Given_OutputAlreadyExists_When_Inject_Then_SkipsIt()
    {
        // Arrange
        const string bicep = """
            param location string

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              properties: {}
            }

            output vaultUri string = kv.properties.vaultUri
            """;
        var outputs = new[]
        {
            new ModuleOutputInjection("vaultUri", "kv.properties.vaultUri", IsSecure: false),
        };

        // Act
        var result = BicepOutputInjector.Inject(bicep, outputs);

        // Assert — should have exactly one occurrence
        var count = result.Split("output vaultUri").Length - 1;
        count.Should().Be(1);
    }

    [Fact]
    public void Given_OutputRefsUndeclaredSymbol_When_Inject_Then_SkipsIt()
    {
        // Arrange — "redis" is not declared in the module
        var outputs = new[]
        {
            new ModuleOutputInjection("redisHost", "redis.properties.hostName", IsSecure: false),
        };

        // Act
        var result = BicepOutputInjector.Inject(MinimalModule, outputs);

        // Assert
        result.Should().NotContain("output redisHost");
    }

    [Fact]
    public void Given_MultipleOutputs_When_Inject_Then_AppendsAll()
    {
        // Arrange
        var outputs = new[]
        {
            new ModuleOutputInjection("vaultUri", "kv.properties.vaultUri", IsSecure: false),
            new ModuleOutputInjection("secretUri", "kv.properties.secretUri", IsSecure: true),
        };

        // Act
        var result = BicepOutputInjector.Inject(MinimalModule, outputs);

        // Assert
        result.Should().Contain("output vaultUri string = kv.properties.vaultUri");
        result.Should().Contain("output secretUri string = kv.properties.secretUri");
        result.Should().Contain("@secure()");
    }

    [Fact]
    public void Given_EmptyOutputsList_When_Inject_Then_ReturnsOriginalContent()
    {
        // Arrange
        var outputs = Array.Empty<ModuleOutputInjection>();

        // Act
        var result = BicepOutputInjector.Inject(MinimalModule, outputs);

        // Assert — trimmed content should be equivalent
        result.Trim().Should().Be(MinimalModule.Trim());
    }

    // ── ExtractRootSymbol ──

    [Theory]
    [InlineData("kv.properties.vaultUri", "kv")]
    [InlineData("redis.properties.hostName", "redis")]
    [InlineData("sqlServer.properties.fullyQualifiedDomainName", "sqlServer")]
    public void Given_DotAccessExpression_When_ExtractRootSymbol_Then_ReturnsFirstSegment(
        string expression, string expected)
    {
        // Act
        var result = BicepOutputInjector.ExtractRootSymbol(expression);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("'${sqlServer.properties.fqdn}'", "sqlServer")]
    [InlineData("'https://${kv.name}.vault.azure.net/'", "kv")]
    public void Given_InterpolationExpression_When_ExtractRootSymbol_Then_ExtractsSymbol(
        string expression, string expected)
    {
        // Act
        var result = BicepOutputInjector.ExtractRootSymbol(expression);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("'literal-string'")]
    [InlineData("123")]
    public void Given_LiteralOrEmpty_When_ExtractRootSymbol_Then_ReturnsNull(string expression)
    {
        // Act
        var result = BicepOutputInjector.ExtractRootSymbol(expression);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Given_SymbolWithoutDot_When_ExtractRootSymbol_Then_ReturnsNull()
    {
        // Arrange — a bare identifier with no dot access
        const string expression = "kv";

        // Act
        var result = BicepOutputInjector.ExtractRootSymbol(expression);

        // Assert
        result.Should().BeNull();
    }
}
