using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectResourceNamingTemplateTests
{
    [Fact]
    public void Given_ResourceTypeAndTemplate_When_SetResourceNamingTemplate_Then_EntryIsAdded()
    {
        // Arrange
        const string resourceType = "KeyVault";
        var template = new NamingTemplate("{name}-kv");
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());

        // Act
        var entry = project.SetResourceNamingTemplate(resourceType, template);

        // Assert
        entry.ResourceType.Should().Be(resourceType);
        entry.Template.Value.Should().Be("{name}-kv");
        entry.ProjectId.Should().Be(project.Id);
        project.ResourceNamingTemplates.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingTemplate_When_SetWithSameResourceType_Then_TemplateIsUpdatedAndNoDuplicate()
    {
        // Arrange
        const string resourceType = "KeyVault";
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());
        var first = project.SetResourceNamingTemplate(resourceType, new NamingTemplate("{name}"));

        // Act
        var second = project.SetResourceNamingTemplate(resourceType, new NamingTemplate("{prefix}-{name}-kv"));

        // Assert
        second.Should().BeSameAs(first);
        second.Template.Value.Should().Be("{prefix}-{name}-kv");
        project.ResourceNamingTemplates.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingTemplate_When_RemoveResourceNamingTemplate_Then_ReturnsTrueAndRemovesEntry()
    {
        // Arrange
        const string resourceType = "KeyVault";
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());
        project.SetResourceNamingTemplate(resourceType, new NamingTemplate("{name}"));

        // Act
        var removed = project.RemoveResourceNamingTemplate(resourceType);

        // Assert
        removed.Should().BeTrue();
        project.ResourceNamingTemplates.Should().BeEmpty();
    }

    [Fact]
    public void Given_NoMatchingTemplate_When_RemoveResourceNamingTemplate_Then_ReturnsFalse()
    {
        // Arrange
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());

        // Act
        var removed = project.RemoveResourceNamingTemplate("KeyVault");

        // Assert
        removed.Should().BeFalse();
    }
}
