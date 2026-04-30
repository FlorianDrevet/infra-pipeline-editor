using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectMemberTests
{
    [Fact]
    public void Given_NewProject_When_Created_Then_OwnerMemberHasOwnerRole()
    {
        // Arrange
        var ownerId = UserId.CreateUnique();
        var project = Project.Create(new Name("Demo"), description: null, ownerId);

        // Act
        var owner = project.Members.Single();

        // Assert
        owner.UserId.Should().Be(ownerId);
        owner.Role.Value.Should().Be(Role.RoleEnum.Owner);
        owner.ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public void Given_OwnerMember_When_ChangeRoleToReader_Then_RoleIsUpdated()
    {
        // Arrange
        var ownerId = UserId.CreateUnique();
        var project = Project.Create(new Name("Demo"), description: null, ownerId);

        // Act
        project.ChangeRole(ownerId, new Role(Role.RoleEnum.Reader));

        // Assert
        project.Members.Single().Role.Value.Should().Be(Role.RoleEnum.Reader);
    }
}
