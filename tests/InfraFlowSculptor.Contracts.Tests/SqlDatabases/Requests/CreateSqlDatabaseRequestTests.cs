using FluentAssertions;
using InfraFlowSculptor.Contracts.SqlDatabases.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.SqlDatabases.Requests;

public sealed class CreateSqlDatabaseRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidCollation = "SQL_Latin1_General_CP1_CI_AS";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidSqlServerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

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
        var sut = new CreateSqlDatabaseRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            SqlServerId = ValidSqlServerId,
            Collation = ValidCollation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlDatabaseRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateSqlDatabaseRequest
        {
            Name = "sqldb-prod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
            SqlServerId = ValidSqlServerId,
            Collation = ValidCollation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlDatabaseRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(location: "InvalidRegion");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlDatabaseRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(resourceGroupId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlDatabaseRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptySqlServerId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(sqlServerId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateSqlDatabaseRequest.SqlServerId)).Should().BeTrue();
    }

    private static CreateSqlDatabaseRequest CreateValidRequest() => CreateRequest();

    private static CreateSqlDatabaseRequest CreateRequest(
        string? name = null,
        string? location = null,
        Guid? resourceGroupId = null,
        Guid? sqlServerId = null,
        string? collation = null)
    {
        return new CreateSqlDatabaseRequest
        {
            Name = name ?? "sqldb-prod",
            Location = location ?? ValidLocation,
            ResourceGroupId = resourceGroupId ?? ValidResourceGroupId,
            SqlServerId = sqlServerId ?? ValidSqlServerId,
            Collation = collation ?? ValidCollation,
        };
    }
}
