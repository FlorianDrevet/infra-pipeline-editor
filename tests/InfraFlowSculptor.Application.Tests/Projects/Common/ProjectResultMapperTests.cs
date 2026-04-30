using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Common;

public sealed class ProjectResultMapperTests
{
    [Fact]
    public void Given_ProjectWithNestedData_When_ToProjectResult_Then_MapsNestedResults()
    {
        // Arrange
        var project = Project.Create(new Name("RetailApi"), "Retail APIs", UserId.CreateUnique());
        var subscriptionId = Guid.NewGuid();

        project.SetDefaultNamingTemplate(new NamingTemplate("{name}-{resourceAbbr}{suffix}"));
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        project.SetResourceNamingTemplate("StorageAccount", new NamingTemplate("{name}{resourceAbbr}{envShort}"));
        project.SetResourceAbbreviation("StorageAccount", "st");
        project.SetTags([new Tag("costCenter", "platform")]);
        project.SetAgentPoolName("pool-shared");
        project.AddEnvironment(
            new EnvironmentDefinitionData(
                new Name("Development"),
                new ShortName("dev"),
                new Prefix("rg"),
                new Suffix("01"),
                new Location(Location.LocationEnum.FranceCentral),
                new SubscriptionId(subscriptionId),
                new Order(1),
                new RequiresApproval(true),
                AzureResourceManagerConnection: "arm-dev",
                Tags: [new Tag("environment", "dev")]));

        var repositoryAlias = RepositoryAlias.Create("main").Value;
        var repositoryKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        project.AddRepository(repositoryAlias, null, null, null, repositoryKinds);

        // Act
        var result = ProjectResultMapper.ToProjectResult(project);

        // Assert
        result.Id.Should().Be(project.Id);
        result.Name.Should().Be(project.Name);
        result.Description.Should().Be("Retail APIs");
        result.DefaultNamingTemplate.Should().Be("{name}-{resourceAbbr}{suffix}");
        result.LayoutPreset.Should().Be(nameof(LayoutPresetEnum.AllInOne));
        result.AgentPoolName.Should().Be("pool-shared");
        result.Tags.Should().ContainSingle(tag => tag.Name == "costCenter" && tag.Value == "platform");

        var environment = result.EnvironmentDefinitions.Should().ContainSingle().Which;
        environment.Name.Should().Be(project.EnvironmentDefinitions.Single().Name);
        environment.ShortName.Should().Be("dev");
        environment.Prefix.Should().Be("rg");
        environment.Suffix.Should().Be("01");
        environment.Location.Should().Be(nameof(Location.LocationEnum.FranceCentral));
        environment.SubscriptionId.Should().Be(subscriptionId);
        environment.Order.Should().Be(1);
        environment.RequiresApproval.Should().BeTrue();
        environment.AzureResourceManagerConnection.Should().Be("arm-dev");
        environment.Tags.Should().ContainSingle(tag => tag.Name == "environment" && tag.Value == "dev");

        var repository = result.Repositories.Should().ContainSingle().Which;
        repository.Alias.Should().Be("main");
        repository.ProviderType.Should().BeNull();
        repository.RepositoryUrl.Should().BeNull();
        repository.DefaultBranch.Should().BeNull();
        repository.IsConfigured.Should().BeFalse();
        repository.ContentKinds.Should().BeEquivalentTo(
            [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);
    }

    [Fact]
    public void Given_ProjectMemberWithoutLoadedUser_When_ToProjectResult_Then_UsesSafeFallbackValues()
    {
        // Arrange
        var project = Project.Create(new Name("RetailApi"), null, UserId.CreateUnique());

        // Act
        var result = ProjectResultMapper.ToProjectResult(project);

        // Assert
        var owner = result.Members.Should().ContainSingle().Which;
        owner.Role.Should().Be(nameof(Role.RoleEnum.Owner));
        owner.EntraId.Should().Be(Guid.Empty);
        owner.FirstName.Should().BeEmpty();
        owner.LastName.Should().BeEmpty();
    }
}