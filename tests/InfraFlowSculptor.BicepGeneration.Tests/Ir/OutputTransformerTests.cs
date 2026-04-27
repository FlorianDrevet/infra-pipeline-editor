using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class OutputTransformerTests
{
    private static BicepModuleSpec BaseSpec() => new()
    {
        ModuleName = "keyVault",
        ModuleFolderName = "key-vault",
        ResourceTypeName = "Key Vault",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "kv",
            ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2023-07-01",
            Body =
            [
                new BicepPropertyAssignment("name", new BicepReference("name")),
                new BicepPropertyAssignment("location", new BicepReference("location")),
            ],
        },
    };

    [Fact]
    public void Given_Spec_When_WithOutputs_Then_AddsOutputDeclarations()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>
        {
            ("vaultUri", "kv.properties.vaultUri", false),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().ContainSingle()
            .Which.Name.Should().Be("vaultUri");
    }

    [Fact]
    public void Given_SpecWithExistingOutput_When_WithOutputs_Then_SkipsDuplicate()
    {
        var spec = BaseSpec() with
        {
            Outputs =
            [
                new BicepOutput("vaultUri", BicepType.String, new BicepReference("kv.properties.vaultUri")),
            ],
        };
        var outputs = new List<(string, string, bool)>
        {
            ("vaultUri", "kv.properties.vaultUri", false),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().HaveCount(1);
    }

    [Fact]
    public void Given_Spec_When_WithSecureOutput_Then_OutputIsSecure()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>
        {
            ("connectionString", "kv.properties.vaultUri", true),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().ContainSingle()
            .Which.IsSecure.Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyOutputs_When_WithOutputs_Then_ReturnsUnchanged()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>();

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().BeEmpty();
        result.Should().BeSameAs(spec);
    }

    [Fact]
    public void Given_Spec_When_WithMultipleOutputs_Then_AllAdded()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>
        {
            ("vaultUri", "kv.properties.vaultUri", false),
            ("vaultId", "kv.id", false),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().HaveCount(2);
    }

    [Fact]
    public void Given_Spec_When_WithFunctionCallOutput_Then_OutputIsKept()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>
        {
            ("primaryConnectionString", "listKeys(kv.id, '2023-07-01').primaryConnectionString", false),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().ContainSingle()
            .Which.Name.Should().Be("primaryConnectionString");
    }

    [Fact]
    public void Given_Spec_When_WithOutputFromDifferentSymbol_Then_OutputIsSkipped()
    {
        var spec = BaseSpec();
        var outputs = new List<(string, string, bool)>
        {
            ("otherId", "otherResource.id", false),
        };

        var result = spec.WithOutputs(outputs);

        result.Outputs.Should().BeEmpty();
    }
}
