using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectPipelineVariableGroupTests
{
    [Fact]
    public void Given_ProjectIdAndGroupName_When_Create_Then_ExposesValuesAndGeneratesId()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        const string groupName = "MyApp-Secrets";

        // Act
        var sut = ProjectPipelineVariableGroup.Create(projectId, groupName);

        // Assert
        sut.ProjectId.Should().Be(projectId);
        sut.GroupName.Should().Be(groupName);
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Given_TwoCreateCalls_When_Compared_Then_IdsAreUnique()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();

        // Act
        var first = ProjectPipelineVariableGroup.Create(projectId, "G1");
        var second = ProjectPipelineVariableGroup.Create(projectId, "G2");

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
