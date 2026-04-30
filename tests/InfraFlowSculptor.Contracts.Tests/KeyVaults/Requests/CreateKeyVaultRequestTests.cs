using FluentAssertions;
using InfraFlowSculptor.Contracts.KeyVaults.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.KeyVaults.Requests;

public sealed class CreateKeyVaultRequestTests
{
    private const string ValidLocation = "WestEurope";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateKeyVaultRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "kv-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateKeyVaultRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "kv-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateKeyVaultRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateKeyVaultRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = null!,
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateKeyVaultRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateKeyVaultRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "kv-prod",
            Location = "MoonBase",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateKeyVaultRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EnvironmentEntryWithInvalidSku_When_Validate_Then_ReturnsEntryError()
    {
        // Arrange
        var sut = new CreateKeyVaultRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "kv-prod",
            Location = ValidLocation,
            EnvironmentSettings =
            [
                new KeyVaultEnvironmentConfigEntry
                {
                    EnvironmentName = "dev",
                    Sku = "NotARealSku",
                },
            ],
        };

        // Act — top-level validator does not recurse into the nested entry, so we validate the entry itself.
        var entryResults = RequestValidator.Validate(sut.EnvironmentSettings![0]);

        // Assert
        entryResults.HasErrorForMember(nameof(KeyVaultEnvironmentConfigEntry.Sku)).Should().BeTrue();
    }
}
