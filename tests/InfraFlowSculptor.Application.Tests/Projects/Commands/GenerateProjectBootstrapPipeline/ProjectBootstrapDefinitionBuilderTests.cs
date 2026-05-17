using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using InfraFlowSculptor.PipelineGeneration.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBootstrapPipeline;

public sealed class ProjectBootstrapDefinitionBuilderTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApplicationFolderNameResolver _applicationFolderNameResolver;
    private readonly Project _project;
    private readonly ProjectPipelineVariableGroup _variableGroup;
    private readonly InfrastructureConfigReadModel _config;
    private readonly ProjectBootstrapDefinitionBuilder _sut;

    public ProjectBootstrapDefinitionBuilderTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _applicationFolderNameResolver = Substitute.For<IApplicationFolderNameResolver>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _variableGroup = _project.AddPipelineVariableGroup("shared-{env}").Value;

        _config = new InfrastructureConfigReadModel(
            Id: Guid.NewGuid(),
            Name: "primary config",
            ProjectId: _project.Id.Value,
            ResourceGroups:
            [
                new ResourceGroupReadModel(
                    Id: Guid.NewGuid(),
                    Name: "rg-apps",
                    Location: "francecentral",
                    Resources:
                    [
                        new AzureResourceReadModel(
                            Id: Guid.NewGuid(),
                            Name: "orders-api",
                            Location: "francecentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                            Properties: new Dictionary<string, string>(),
                            EnvironmentConfigs: []),
                        new AzureResourceReadModel(
                            Id: Guid.NewGuid(),
                            Name: "legacy-api",
                            Location: "francecentral",
                            ResourceType: AzureResourceTypes.ArmTypes.WebAppType,
                            Properties: new Dictionary<string, string>(),
                            EnvironmentConfigs: [],
                            IsExisting: true),
                    ])
            ],
            Environments:
            [
                new EnvironmentDefinitionReadModel(
                    Id: Guid.NewGuid(),
                    Name: "Development",
                    ShortName: "dev",
                    Location: "francecentral",
                    Prefix: "d",
                    Suffix: string.Empty,
                    AzureResourceManagerConnection: null,
                    SubscriptionId: null,
                    Tags: new Dictionary<string, string>()),
                new EnvironmentDefinitionReadModel(
                    Id: Guid.NewGuid(),
                    Name: "Production",
                    ShortName: "prod",
                    Location: "francecentral",
                    Prefix: "p",
                    Suffix: string.Empty,
                    AzureResourceManagerConnection: null,
                    SubscriptionId: null,
                    Tags: new Dictionary<string, string>()),
            ],
            NamingContext: new NamingContextReadModel(
                DefaultTemplate: null,
                ResourceTemplates: new Dictionary<string, string>(),
                ResourceAbbreviations: new Dictionary<string, string>()),
            RoleAssignments: [],
            AppSettings:
            [
                new AppSettingReadModel(
                    ResourceId: Guid.NewGuid(),
                    ResourceName: "orders-api",
                    ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                    Name: "Orders__Secret",
                    EnvironmentValues: null,
                    SourceResourceId: null,
                    SourceResourceName: null,
                    SourceResourceType: null,
                    SourceOutputName: null,
                    IsOutputReference: false,
                    KeyVaultResourceId: Guid.NewGuid(),
                    KeyVaultResourceName: "kv-shared",
                    SecretName: "orders-secret",
                    IsKeyVaultReference: true,
                    SecretValueAssignment: "ViaBicepparam",
                    VariableGroupId: _variableGroup.Id.Value,
                    PipelineVariableName: "app-secret",
                    VariableGroupName: "shared-{env}",
                    IsViaVariableGroup: true)
            ],
            CrossConfigReferences: [],
            ProjectTags: new Dictionary<string, string>(),
            ConfigTags: new Dictionary<string, string>(),
            SecureParameterMappings:
            [
                new SecureParameterMappingReadModel(
                    Id: Guid.NewGuid(),
                    ResourceId: Guid.NewGuid(),
                    ResourceName: "orders-api",
                    SecureParameterName: "AcrPassword",
                    VariableGroupId: _variableGroup.Id.Value,
                    VariableGroupName: "shared-{env}",
                    PipelineVariableName: "acr-password")
            ]);

        _sut = new ProjectBootstrapDefinitionBuilder(_projectRepository, _applicationFolderNameResolver);
    }

    [Fact]
    public async Task Given_ProjectBootstrapInputs_When_BuildAsync_Then_ReturnsPipelinesVariableGroupsAndEnvironmentsAsync()
    {
        // Arrange
        _projectRepository.GetPipelineVariableUsagesAsync(Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroupId>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, List<PipelineVariableUsageResult>>
            {
                [_variableGroup.Id.Value] =
                [
                    new PipelineVariableUsageResult(
                        PipelineVariableName: "api-token",
                        AppSettingName: "ConnectionStrings__Api",
                        ResourceName: "orders-api",
                        ResourceType: "ContainerApp",
                        ConfigName: _config.Name),
                ],
            });
        _applicationFolderNameResolver.ResolveAsync(
                Arg.Is<AzureResourceReadModel>(resource => resource.Name == "orders-api"),
                Arg.Any<CancellationToken>())
            .Returns("orders-api");

        // Act
        var result = await _sut.BuildAsync(
            _project,
            [_config],
            infraPipelineBasePath: "infra-pipelines",
            appPipelineBasePath: "app-pipelines",
            CancellationToken.None);

        // Assert
        result.InfraPipelines.Should().BeEquivalentTo(
        [
            new BootstrapPipelineDefinition("primary-config - CI", "/infra-pipelines/.azuredevops/primary-config/ci.pipeline.yml", "\\primary-config"),
            new BootstrapPipelineDefinition("primary-config - PR", "/infra-pipelines/.azuredevops/primary-config/pr.pipeline.yml", "\\primary-config"),
            new BootstrapPipelineDefinition("primary-config - Release", "/infra-pipelines/.azuredevops/primary-config/release.pipeline.yml", "\\primary-config"),
        ]);
        result.AppPipelines.Should().BeEquivalentTo(
        [
            new BootstrapPipelineDefinition("primary config - orders-api - CI", "/app-pipelines/.azuredevops/primary-config/apps/orders-api/ci.app-pipeline.yml", "\\primary-config\\Applications\\orders-api"),
            new BootstrapPipelineDefinition("primary config - orders-api - Release", "/app-pipelines/.azuredevops/primary-config/apps/orders-api/release.app-pipeline.yml", "\\primary-config\\Applications\\orders-api"),
        ]);

        result.VariableGroups.Should().HaveCount(2);
        result.VariableGroups.Should().ContainEquivalentOf(
            new BootstrapVariableGroupDefinition(
                "shared-dev",
                [
                    new BootstrapVariable("acr-password", string.Empty, true),
                    new BootstrapVariable("api-token", string.Empty, false),
                    new BootstrapVariable("app-secret", string.Empty, true),
                ]));
        result.VariableGroups.Should().ContainEquivalentOf(
            new BootstrapVariableGroupDefinition(
                "shared-prod",
                [
                    new BootstrapVariable("acr-password", string.Empty, true),
                    new BootstrapVariable("api-token", string.Empty, false),
                    new BootstrapVariable("app-secret", string.Empty, true),
                ]));
        result.Environments.Should().BeEquivalentTo(
        [
            new BootstrapEnvironmentDefinition("dev", "Development", false),
            new BootstrapEnvironmentDefinition("prod", "Production", false),
        ]);

        await _applicationFolderNameResolver.Received(1)
            .ResolveAsync(Arg.Is<AzureResourceReadModel>(resource => resource.Name == "orders-api"), Arg.Any<CancellationToken>());
        await _applicationFolderNameResolver.DidNotReceive()
            .ResolveAsync(Arg.Is<AzureResourceReadModel>(resource => resource.Name == "legacy-api"), Arg.Any<CancellationToken>());
    }
}
