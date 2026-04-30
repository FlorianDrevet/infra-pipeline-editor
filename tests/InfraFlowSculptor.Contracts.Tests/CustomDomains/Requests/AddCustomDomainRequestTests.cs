using FluentAssertions;
using InfraFlowSculptor.Contracts.CustomDomains.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.CustomDomains.Requests;

public sealed class AddCustomDomainRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddCustomDomainRequest
        {
            EnvironmentName = "production",
            DomainName = "api.example.com",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullEnvironmentName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new AddCustomDomainRequest
        {
            EnvironmentName = null!,
            DomainName = "api.example.com",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddCustomDomainRequest.EnvironmentName)).Should().BeTrue();
    }

    [Fact]
    public void Given_EnvironmentNameExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new AddCustomDomainRequest
        {
            EnvironmentName = new string('e', 101),
            DomainName = "api.example.com",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddCustomDomainRequest.EnvironmentName)).Should().BeTrue();
    }

    [Fact]
    public void Given_DomainNameExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new AddCustomDomainRequest
        {
            EnvironmentName = "production",
            DomainName = new string('d', 254),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddCustomDomainRequest.DomainName)).Should().BeTrue();
    }

    [Fact]
    public void Given_BindingTypeExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new AddCustomDomainRequest
        {
            EnvironmentName = "production",
            DomainName = "api.example.com",
            BindingType = new string('b', 21),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddCustomDomainRequest.BindingType)).Should().BeTrue();
    }
}
