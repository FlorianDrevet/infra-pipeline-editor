using FluentAssertions;
using InfraFlowSculptor.Contracts.SqlServers.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.SqlServers.Requests;

public sealed class CreateSqlServerRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidVersion = "V12";
    private const string ValidAdminLogin = "sqladmin";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");

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
    public void Given_NullName_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateSqlServerRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            Version = ValidVersion,
            AdministratorLogin = ValidAdminLogin,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlServerRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateSqlServerRequest
        {
            Name = "sql-prod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
            Version = ValidVersion,
            AdministratorLogin = ValidAdminLogin,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlServerRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(location: "InvalidRegion");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlServerRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(resourceGroupId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlServerRequest.ResourceGroupId)).Should().BeTrue();
    }

    private static CreateSqlServerRequest CreateValidRequest() => CreateRequest();

    private static CreateSqlServerRequest CreateRequest(
        string? name = null,
        string? location = null,
        Guid? resourceGroupId = null,
        string? version = null,
        string? administratorLogin = null)
    {
        return new CreateSqlServerRequest
        {
            Name = name ?? "sql-prod",
            Location = location ?? ValidLocation,
            ResourceGroupId = resourceGroupId ?? ValidResourceGroupId,
            Version = version ?? ValidVersion,
            AdministratorLogin = administratorLogin ?? ValidAdminLogin,
        };
    }
}
