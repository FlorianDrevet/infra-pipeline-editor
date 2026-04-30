using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectEnvironmentDefinitionTests
{
    [Fact]
    public void Given_EnvironmentData_When_AddEnvironment_Then_AllPropertiesAreInitialized()
    {
        // Arrange
        var project = CreateValidProject();
        var data = BuildData(order: 0, name: "Development");

        // Act
        var sut = project.AddEnvironment(data);

        // Assert
        sut.ProjectId.Should().Be(project.Id);
        sut.Name.Value.Should().Be("Development");
        sut.ShortName.Value.Should().Be("dev");
        sut.Prefix.Value.Should().Be("p");
        sut.Suffix.Value.Should().Be("s");
        sut.Location.Value.Should().Be(Location.LocationEnum.WestEurope);
        sut.SubscriptionId.Value.Should().NotBeEmpty();
        sut.Order.Value.Should().Be(0);
        sut.RequiresApproval.Value.Should().BeFalse();
        sut.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Given_TagsInData_When_AddEnvironment_Then_TagsAreCopied()
    {
        // Arrange
        var project = CreateValidProject();
        var tags = new[]
        {
            new Tag("env", "prod"),
            new Tag("tier", "backend"),
        };
        var data = BuildData(order: 0, name: "Prod", tags: tags);

        // Act
        var sut = project.AddEnvironment(data);

        // Assert
        sut.Tags.Should().HaveCount(2);
        sut.Tags.Should().Contain(t => t.Name == "env" && t.Value == "prod");
        sut.Tags.Should().Contain(t => t.Name == "tier" && t.Value == "backend");
    }

    [Fact]
    public void Given_ExistingTags_When_SetTags_Then_PreviousTagsAreReplaced()
    {
        // Arrange
        var initial = new[] { new Tag("env", "dev") };
        var project = CreateValidProject();
        var sut = project.AddEnvironment(BuildData(order: 0, name: "Dev", tags: initial));
        var replacement = new[] { new Tag("env", "prod") };

        // Act
        sut.SetTags(replacement);

        // Assert
        sut.Tags.Should().HaveCount(1);
        sut.Tags.Single().Value.Should().Be("prod");
    }

    private static Project CreateValidProject()
        => Project.Create(new Name("Demo"), description: null, UserId.CreateUnique());

    private static EnvironmentDefinitionData BuildData(
        int order,
        string name,
        IEnumerable<Tag>? tags = null)
        => new(
            new Name(name),
            new ShortName("dev"),
            new Prefix("p"),
            new Suffix("s"),
            new Location(Location.LocationEnum.WestEurope),
            new SubscriptionId(Guid.NewGuid()),
            new Order(order),
            new RequiresApproval(false),
            AzureResourceManagerConnection: null,
            Tags: tags ?? []);
}
