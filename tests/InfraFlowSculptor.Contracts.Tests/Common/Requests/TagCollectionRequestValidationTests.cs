using FluentAssertions;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Common.Requests;

public sealed class TagCollectionRequestValidationTests
{
    public static IEnumerable<object[]> RequestsWithTooManyTags()
    {
        yield return [new SetProjectTagsRequest { Tags = CreateTags(TagRequestConstraints.MaxTagCount + 1) }, nameof(SetProjectTagsRequest.Tags)];
        yield return [new SetInfraConfigTagsRequest { Tags = CreateTags(TagRequestConstraints.MaxTagCount + 1) }, nameof(SetInfraConfigTagsRequest.Tags)];
        yield return
        [
            new AddProjectEnvironmentRequest
            {
                Name = "Production",
                ShortName = "prod",
                Location = "westeurope",
                Tags = CreateTags(TagRequestConstraints.MaxTagCount + 1),
            },
            nameof(AddProjectEnvironmentRequest.Tags),
        ];
        yield return
        [
            new UpdateProjectEnvironmentRequest
            {
                Name = "Production",
                ShortName = "prod",
                Location = "westeurope",
                SubscriptionId = Guid.NewGuid(),
                Tags = CreateTags(TagRequestConstraints.MaxTagCount + 1),
            },
            nameof(UpdateProjectEnvironmentRequest.Tags),
        ];
    }

    public static IEnumerable<object[]> RequestsWithAllowedTags()
    {
        yield return [new SetProjectTagsRequest { Tags = CreateTags(TagRequestConstraints.MaxTagCount) }];
        yield return [new SetInfraConfigTagsRequest { Tags = CreateTags(TagRequestConstraints.MaxTagCount) }];
        yield return
        [
            new AddProjectEnvironmentRequest
            {
                Name = "Production",
                ShortName = "prod",
                Location = "westeurope",
                Tags = CreateTags(TagRequestConstraints.MaxTagCount),
            },
        ];
        yield return
        [
            new UpdateProjectEnvironmentRequest
            {
                Name = "Production",
                ShortName = "prod",
                Location = "westeurope",
                SubscriptionId = Guid.NewGuid(),
                Tags = CreateTags(TagRequestConstraints.MaxTagCount),
            },
        ];
    }

    [Theory]
    [MemberData(nameof(RequestsWithTooManyTags))]
    public void Given_TooManyTags_When_Validate_Then_ReturnsCollectionCountError(object sut, string memberName)
    {
        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(memberName).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(RequestsWithAllowedTags))]
    public void Given_MaxAllowedTags_When_Validate_Then_NoCollectionCountError(object sut)
    {
        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    private static IReadOnlyList<TagRequest> CreateTags(int count)
    {
        return Enumerable.Range(1, count)
            .Select(index => new TagRequest
            {
                Name = $"tag-{index}",
                Value = $"value-{index}",
            })
            .ToArray();
    }
}