using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoFiles;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.SearchCodeRepoFiles;

public sealed class SearchCodeRepoFilesQueryValidatorTests
{
    private const string BranchProperty = nameof(SearchCodeRepoFilesQuery.Branch);
    private const string FilenamePatternProperty = nameof(SearchCodeRepoFilesQuery.FilenamePattern);

    private readonly SearchCodeRepoFilesQueryValidator _sut = new();

    [Fact]
    public void Given_ValidQuery_When_Validate_Then_Succeeds()
    {
        // Arrange
        var query = new SearchCodeRepoFilesQuery(
            ProjectId.CreateUnique(),
            "main",
            "*.bicep");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidQueryWithNullFilenamePattern_When_Validate_Then_Succeeds()
    {
        // Arrange
        var query = new SearchCodeRepoFilesQuery(
            ProjectId.CreateUnique(),
            "main");

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyBranch_When_Validate_Then_FailsOnBranch(string? branch)
    {
        // Arrange
        var query = new SearchCodeRepoFilesQuery(
            ProjectId.CreateUnique(),
            branch!);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == BranchProperty);
    }

    [Fact]
    public void Given_FilenamePatternTooLong_When_Validate_Then_FailsOnFilenamePattern()
    {
        // Arrange
        var query = new SearchCodeRepoFilesQuery(
            ProjectId.CreateUnique(),
            "main",
            new string('a', 101));

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == FilenamePatternProperty);
    }
}
