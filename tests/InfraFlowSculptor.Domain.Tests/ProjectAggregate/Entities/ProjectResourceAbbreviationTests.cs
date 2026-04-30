using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.Entities;

public sealed class ProjectResourceAbbreviationTests
{
    [Fact]
    public void Given_ResourceTypeAndAbbreviation_When_SetResourceAbbreviation_Then_EntryIsAdded()
    {
        // Arrange
        const string resourceType = "StorageAccount";
        const string abbreviation = "st";
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());

        // Act
        var entry = project.SetResourceAbbreviation(resourceType, abbreviation);

        // Assert
        entry.ResourceType.Should().Be(resourceType);
        entry.Abbreviation.Should().Be(abbreviation);
        entry.ProjectId.Should().Be(project.Id);
        project.ResourceAbbreviations.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingAbbreviation_When_SetWithSameResourceType_Then_AbbreviationIsUpdatedAndNoDuplicate()
    {
        // Arrange
        const string resourceType = "StorageAccount";
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());
        var first = project.SetResourceAbbreviation(resourceType, "st");

        // Act
        var second = project.SetResourceAbbreviation(resourceType, "stg");

        // Assert
        second.Should().BeSameAs(first);
        second.Abbreviation.Should().Be("stg");
        project.ResourceAbbreviations.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingAbbreviation_When_RemoveResourceAbbreviation_Then_ReturnsTrueAndRemovesEntry()
    {
        // Arrange
        const string resourceType = "StorageAccount";
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());
        project.SetResourceAbbreviation(resourceType, "st");

        // Act
        var removed = project.RemoveResourceAbbreviation(resourceType);

        // Assert
        removed.Should().BeTrue();
        project.ResourceAbbreviations.Should().BeEmpty();
    }

    [Fact]
    public void Given_NoMatchingAbbreviation_When_RemoveResourceAbbreviation_Then_ReturnsFalse()
    {
        // Arrange
        var project = Project.Create(new Name("Demo"), null, UserId.CreateUnique());

        // Act
        var removed = project.RemoveResourceAbbreviation("StorageAccount");

        // Assert
        removed.Should().BeFalse();
    }
}
