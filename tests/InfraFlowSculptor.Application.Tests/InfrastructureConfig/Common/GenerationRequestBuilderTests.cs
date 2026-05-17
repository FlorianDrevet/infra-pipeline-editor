using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Common;

public sealed class GenerationRequestBuilderTests
{
    [Fact]
    public void Given_PipelineSpecificInputs_When_BuildForPipeline_Then_PopulatesPipelineFields()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var pipelineVariableGroup = ProjectPipelineVariableGroup.Create(projectId, "shared-secrets");
        var targetResourceId = Guid.NewGuid();
        var config = new InfrastructureConfigReadModel(
            Id: Guid.NewGuid(),
            Name: "dev",
            ProjectId: projectId.Value,
            ResourceGroups:
            [
                new ResourceGroupReadModel(
                    Id: Guid.NewGuid(),
                    Name: "rg-app",
                    Location: "FranceCentral",
                    Resources:
                    [
                        new AzureResourceReadModel(
                            Id: targetResourceId,
                            Name: "ifs-api",
                            Location: "FranceCentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                            Properties: new Dictionary<string, string>(),
                            EnvironmentConfigs: [])
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
            AppSettings:
            [
                new AppSettingReadModel(
                    ResourceId: targetResourceId,
                    ResourceName: "ifs-api",
                    ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                    Name: "JWT_SECRET",
                    EnvironmentValues: null,
                    SourceResourceId: null,
                    SourceResourceName: null,
                    SourceResourceType: null,
                    SourceOutputName: null,
                    IsOutputReference: false,
                    KeyVaultResourceId: null,
                    KeyVaultResourceName: null,
                    SecretName: null,
                    IsKeyVaultReference: false,
                    VariableGroupId: pipelineVariableGroup.Id.Value,
                    PipelineVariableName: "jwt-secret",
                    VariableGroupName: pipelineVariableGroup.GroupName,
                    IsViaVariableGroup: true)
            ],
            CrossConfigReferences: [],
            ProjectTags: new Dictionary<string, string>(),
            ConfigTags: new Dictionary<string, string>(),
            SecureParameterMappings: []);

        // Act
        var result = GenerationRequestBuilder.BuildForPipeline(
            config,
            [pipelineVariableGroup],
            agentPoolName: "private-linux-pool",
            bicepBasePath: "infra/generated",
            generators: []);

        // Assert
        result.PipelineVariableGroups.Should().ContainSingle();
        result.PipelineVariableGroups[0].GroupName.Should().Be("shared-secrets");
        result.PipelineVariableGroups[0].Mappings.Should().ContainSingle();
        result.PipelineVariableGroups[0].Mappings[0].PipelineVariableName.Should().Be("jwt-secret");
        result.PipelineVariableGroups[0].Mappings[0].BicepParameterName.Should().Be("JWT_SECRET");
        result.AgentPoolName.Should().Be("private-linux-pool");
        result.BicepBasePath.Should().Be("infra/generated");
        result.SecureParameterOverrides.Should().BeEmpty();
        result.RoleAssignments.Should().BeEmpty();
        result.AppSettings.Should().BeEmpty();
        result.ExistingResourceReferences.Should().BeEmpty();
    }

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
