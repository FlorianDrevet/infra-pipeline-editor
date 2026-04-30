using FluentAssertions;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.StorageAccounts.Requests;

public sealed class CreateStorageAccountRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidKind = "StorageV2";
    private const string ValidAccessTier = "Hot";
    private const string ValidTlsVersion = "Tls12";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = CreateValidRequest();

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateStorageAccountRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "stgaccount",
            Location = ValidLocation,
            Kind = ValidKind,
            AccessTier = ValidAccessTier,
            MinimumTlsVersion = ValidTlsVersion,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateStorageAccountRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateStorageAccountRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "stgaccount",
            Location = "Atlantis",
            Kind = ValidKind,
            AccessTier = ValidAccessTier,
            MinimumTlsVersion = ValidTlsVersion,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateStorageAccountRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidKind_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateStorageAccountRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "stgaccount",
            Location = ValidLocation,
            Kind = "InvalidKind",
            AccessTier = ValidAccessTier,
            MinimumTlsVersion = ValidTlsVersion,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateStorageAccountRequest.Kind)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateStorageAccountRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = null!,
            Location = ValidLocation,
            Kind = ValidKind,
            AccessTier = ValidAccessTier,
            MinimumTlsVersion = ValidTlsVersion,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateStorageAccountRequest.Name)).Should().BeTrue();
    }

    private static CreateStorageAccountRequest CreateValidRequest()
    {
        return new CreateStorageAccountRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "stgaccount",
            Location = ValidLocation,
            Kind = ValidKind,
            AccessTier = ValidAccessTier,
            MinimumTlsVersion = ValidTlsVersion,
        };
    }
}
