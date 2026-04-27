using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class TagsTransformerTests
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
                new BicepPropertyAssignment("properties", BicepObjectExpression.Empty),
            ],
        },
    };

    [Fact]
    public void Given_Spec_When_WithTags_Then_AddsTagsParam()
    {
        var spec = BaseSpec();

        var result = spec.WithTags();

        result.Parameters.Should().Contain(p => p.Name == "tags");
    }

    [Fact]
    public void Given_Spec_When_WithTags_Then_TagsParamHasObjectTypeAndEmptyDefault()
    {
        var spec = BaseSpec();

        var result = spec.WithTags();

        var tagsParam = result.Parameters.First(p => p.Name == "tags");
        tagsParam.Type.Should().Be(BicepType.Object);
        tagsParam.DefaultValue.Should().Be(BicepObjectExpression.Empty);
    }

    [Fact]
    public void Given_Spec_When_WithTags_Then_AddsTagsPropertyToResource()
    {
        var spec = BaseSpec();

        var result = spec.WithTags();

        result.Resource.Body.Should().Contain(p => p.Key == "tags");
    }

    [Fact]
    public void Given_Spec_When_WithTags_Then_TagsPropertyIsAfterLocation()
    {
        var spec = BaseSpec();

        var result = spec.WithTags();

        var locationIdx = result.Resource.Body.ToList().FindIndex(p => p.Key == "location");
        var tagsIdx = result.Resource.Body.ToList().FindIndex(p => p.Key == "tags");

        tagsIdx.Should().Be(locationIdx + 1);
    }

    [Fact]
    public void Given_SpecWithExistingTags_When_WithTags_Then_DoesNotDuplicate()
    {
        var spec = BaseSpec().WithTags();

        var result = spec.WithTags();

        result.Parameters.Count(p => p.Name == "tags").Should().Be(1);
        result.Resource.Body.Count(p => p.Key == "tags").Should().Be(1);
    }
}
