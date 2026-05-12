using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore.Models;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class SecureParameterOverrideHelperTests
{
    [Fact]
    public void Given_SecureParameterWithoutCustomMapping_When_DerivingOverrides_Then_UsesCanonicalResourceIdentifier()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            Name = "web-api",
            Type = "Microsoft.Web/sites",
        };

        var generator = Substitute.For<IResourceTypeBicepSpecGenerator>();
        generator.ResourceType.Returns(resource.Type);
        generator.Generate(resource).Returns(new GeneratedTypeModule
        {
            ModuleName = "webApp",
            SecureParameters = ["clientSecret"],
        });

        var variableGroups = new List<PipelineVariableGroupDefinition>();

        // Act
        var result = SecureParameterOverrideHelper.DeriveSecureParameterOverrides(
            [resource],
            [generator],
            secureParameterMappings: null,
            variableGroups);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("webAppWebApiClientSecret");
        variableGroups.Should().BeEmpty();
    }
}