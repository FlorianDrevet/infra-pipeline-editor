using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.CheckResourceNameAvailability;

public sealed class CheckResourceNameAvailabilityQueryValidatorTests
{
    private readonly CheckResourceNameAvailabilityQueryValidator _sut = new();

    [Fact]
    public void Given_ValidQuery_When_Validate_Then_Succeeds()
    {
        var query = new CheckResourceNameAvailabilityQuery(
            ProjectId.CreateUnique(),
            ConfigId: null,
            ResourceType: "StorageAccount",
            Name: "my-resource");

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceType_When_Validate_Then_Fails()
    {
        var query = new CheckResourceNameAvailabilityQuery(
            ProjectId.CreateUnique(),
            ConfigId: null,
            ResourceType: "",
            Name: "my-resource");

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CheckResourceNameAvailabilityQuery.ResourceType));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_Fails()
    {
        var query = new CheckResourceNameAvailabilityQuery(
            ProjectId.CreateUnique(),
            ConfigId: null,
            ResourceType: "StorageAccount",
            Name: "");

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CheckResourceNameAvailabilityQuery.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_Fails()
    {
        var query = new CheckResourceNameAvailabilityQuery(
            ProjectId.CreateUnique(),
            ConfigId: null,
            ResourceType: "StorageAccount",
            Name: new string('a', 101));

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CheckResourceNameAvailabilityQuery.Name));
    }
}
