using System.Reflection;
using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class ProjectQueryToolsTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    [Fact]
    public async Task Given_ProjectWithConfigAndResourceGroup_When_GetProjectStructure_Then_ReturnsHierarchicalJsonAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var infraConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var keyVault = KeyVault.Create(
            resourceGroupId,
            new Name("vault-main"),
            new Location(Location.LocationEnum.FranceCentral));
        SetResourceType(keyVault, AzureResourceTypes.KeyVault);

        var project = CreateProjectResult(projectId);
        var config = CreateInfrastructureConfigResult(projectId, infraConfigId);
        var resourceGroup = new ResourceGroupResult(
            resourceGroupId,
            infraConfigId,
            new Location(Location.LocationEnum.FranceCentral),
            new Name("rg-main"),
            [keyVault]);

        _mediator.Send(Arg.Any<GetProjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(project));
        _mediator.Send(Arg.Any<ListProjectConfigsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<List<GetInfrastructureConfigResult>>>(new List<GetInfrastructureConfigResult> { config }));
        _mediator.Send(Arg.Any<ListResourceGroupsByConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<List<ResourceGroupResult>>>(new List<ResourceGroupResult> { resourceGroup }));

        // Act
        var json = await ProjectQueryTools.GetProjectStructure(_mediator, projectId.Value.ToString());

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("projectId").GetString().Should().Be(projectId.Value.ToString());
        root.GetProperty("projectName").GetString().Should().Be("RetailApi");
        root.GetProperty("environmentCount").GetInt32().Should().Be(1);
        root.GetProperty("totalConfigs").GetInt32().Should().Be(1);

        var configJson = root.GetProperty("infrastructureConfigs")[0];
        configJson.GetProperty("infraConfigId").GetString().Should().Be(infraConfigId.Value.ToString());
        configJson.GetProperty("resourceGroupCount").GetInt32().Should().Be(1);
        configJson.GetProperty("resourceCount").GetInt32().Should().Be(1);

        var resourceGroupJson = configJson.GetProperty("resourceGroups")[0];
        resourceGroupJson.GetProperty("resourceGroupId").GetString().Should().Be(resourceGroupId.Value.ToString());
        resourceGroupJson.GetProperty("name").GetString().Should().Be("rg-main");
        resourceGroupJson.GetProperty("location").GetString().Should().Be("FranceCentral");
        resourceGroupJson.GetProperty("resourceCount").GetInt32().Should().Be(1);

        var resourceJson = resourceGroupJson.GetProperty("resources")[0];
        resourceJson.GetProperty("resourceId").GetString().Should().Be(keyVault.Id.Value.ToString());
        resourceJson.GetProperty("resourceType").GetString().Should().Be(AzureResourceTypes.KeyVault);
        resourceJson.GetProperty("name").GetString().Should().Be("vault-main");
    }

    [Fact]
    public async Task Given_ProjectResources_When_ListProjectResources_Then_ReturnsFlatResourceJsonAsync()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var infraConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var keyVault = KeyVault.Create(
            resourceGroupId,
            new Name("vault-main"),
            new Location(Location.LocationEnum.FranceCentral));
        SetResourceType(keyVault, AzureResourceTypes.KeyVault);

        var config = CreateInfrastructureConfigResult(projectId, infraConfigId);
        var resourceGroup = new ResourceGroupResult(
            resourceGroupId,
            infraConfigId,
            new Location(Location.LocationEnum.FranceCentral),
            new Name("rg-main"),
            [keyVault]);

        _mediator.Send(Arg.Any<ListProjectConfigsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<List<GetInfrastructureConfigResult>>>(new List<GetInfrastructureConfigResult> { config }));
        _mediator.Send(Arg.Any<ListResourceGroupsByConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<List<ResourceGroupResult>>>(new List<ResourceGroupResult> { resourceGroup }));

        // Act
        var json = await ProjectQueryTools.ListProjectResources(_mediator, projectId.Value.ToString());

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("projectId").GetString().Should().Be(projectId.Value.ToString());
        root.GetProperty("totalResources").GetInt32().Should().Be(1);

        var resourceJson = root.GetProperty("resources")[0];
        resourceJson.GetProperty("resourceId").GetString().Should().Be(keyVault.Id.Value.ToString());
        resourceJson.GetProperty("resourceType").GetString().Should().Be(AzureResourceTypes.KeyVault);
        resourceJson.GetProperty("name").GetString().Should().Be("vault-main");
        resourceJson.GetProperty("resourceGroupId").GetString().Should().Be(resourceGroupId.Value.ToString());
        resourceJson.GetProperty("resourceGroupName").GetString().Should().Be("rg-main");
        resourceJson.GetProperty("infraConfigId").GetString().Should().Be(infraConfigId.Value.ToString());
        resourceJson.GetProperty("infraConfigName").GetString().Should().Be("main-config");
    }

    private static ProjectResult CreateProjectResult(ProjectId projectId) =>
        new(
            projectId,
            new Name("RetailApi"),
            Description: null,
            Members: [],
            EnvironmentDefinitions:
            [
                new ProjectEnvironmentDefinitionResult(
                    new ProjectEnvironmentDefinitionId(Guid.NewGuid()),
                    new Name("Production"),
                    "prod",
                    "",
                    "prod",
                    Location.ToAzureRegionKey(Location.LocationEnum.FranceCentral),
                    Guid.NewGuid(),
                    0,
                    false,
                    null,
                    []),
            ],
            DefaultNamingTemplate: null,
            ResourceNamingTemplates: [],
            ResourceAbbreviations: [],
            Tags: [],
            LayoutPreset: nameof(LayoutPresetEnum.AllInOne));

    private static GetInfrastructureConfigResult CreateInfrastructureConfigResult(
        ProjectId projectId,
        InfrastructureConfigId infraConfigId) =>
        new(
            infraConfigId,
            new Name("main-config"),
            projectId,
            DefaultNamingTemplate: null,
            UseProjectNamingConventions: true,
            ResourceNamingTemplates: [],
            ResourceAbbreviationOverrides: [],
            Tags: [],
            ResourceGroupCount: 1,
            ResourceCount: 1,
            CrossConfigReferenceCount: 0,
            LayoutMode: null,
            Repositories: null);

    private static void SetResourceType(AzureResource resource, string resourceType)
    {
        var property = typeof(AzureResource).GetProperty(
            nameof(AzureResource.ResourceType),
            BindingFlags.Instance | BindingFlags.Public);

        property.Should().NotBeNull();
        property!.SetValue(resource, new ResourceTypeName(resourceType));
    }
}