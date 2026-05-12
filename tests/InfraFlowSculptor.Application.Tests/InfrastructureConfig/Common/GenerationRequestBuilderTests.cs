using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Common;

public sealed class GenerationRequestBuilderTests
{
    [Fact]
    public void Given_LocalExistingResource_When_Build_Then_PreservesExistingResourceIdInGenerationRequest()
    {
        // Arrange
        var containerRegistryId = Guid.NewGuid();
        var config = new InfrastructureConfigReadModel(
            Id: Guid.NewGuid(),
            Name: "dev",
            ProjectId: Guid.NewGuid(),
            ResourceGroups:
            [
                new ResourceGroupReadModel(
                    Id: Guid.NewGuid(),
                    Name: "ifs",
                    Location: "FranceCentral",
                    Resources:
                    [
                        new AzureResourceReadModel(
                            Id: Guid.NewGuid(),
                            Name: "ifs-api",
                            Location: "FranceCentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                            Properties: new Dictionary<string, string>
                            {
                                ["containerRegistryId"] = containerRegistryId.ToString(),
                            },
                            EnvironmentConfigs: []),
                        new AzureResourceReadModel(
                            Id: containerRegistryId,
                            Name: "ifs",
                            Location: "FranceCentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerRegistryType,
                            Properties: new Dictionary<string, string>(),
                            EnvironmentConfigs: [],
                            AssignedUserAssignedIdentityName: null,
                            IsExisting: true),
                    ])
            ],
            Environments:
            [
                new EnvironmentDefinitionReadModel(
                    Id: Guid.NewGuid(),
                    Name: "dev",
                    ShortName: "dev",
                    Location: "FranceCentral",
                    Prefix: string.Empty,
                    Suffix: string.Empty,
                    AzureResourceManagerConnection: null,
                    SubscriptionId: null,
                    Tags: new Dictionary<string, string>())
            ],
            NamingContext: new NamingContextReadModel(
                DefaultTemplate: null,
                ResourceTemplates: new Dictionary<string, string>(),
                ResourceAbbreviations: new Dictionary<string, string>()),
            RoleAssignments: [],
            AppSettings: [],
            CrossConfigReferences: [],
            ProjectTags: new Dictionary<string, string>(),
            ConfigTags: new Dictionary<string, string>());

        // Act
        var result = GenerationRequestBuilder.Build(config);

        // Assert
        result.ExistingResourceReferences.Should().ContainSingle(reference =>
            reference.ResourceName == "ifs"
            && reference.ResourceType == AzureResourceTypes.ArmTypes.ContainerRegistryType);
    }
}