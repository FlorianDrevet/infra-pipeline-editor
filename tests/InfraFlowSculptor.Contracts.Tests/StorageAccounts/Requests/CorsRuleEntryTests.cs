using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;

namespace InfraFlowSculptor.Contracts.Tests.StorageAccounts.Requests;

public sealed class CorsRuleEntryTests
{
    [Fact]
    public void Given_WildcardSubdomainOriginAndPrefixedHeader_When_Validate_Then_NoValidationErrorIsReturned()
    {
        // Arrange
        var sut = CreateCorsRuleEntry(
            allowedOrigins: ["https://*.contoso.com"],
            allowedHeaders: ["x-ms-meta*"],
            exposedHeaders: ["ETag"]);

        // Act
        var results = sut.Validate(new ValidationContext(sut)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_OriginContainingAPath_When_Validate_Then_ReturnsOriginValidationError()
    {
        // Arrange
        var sut = CreateCorsRuleEntry(
            allowedOrigins: ["https://contoso.com/path"],
            allowedHeaders: ["x-ms-meta*"],
            exposedHeaders: ["ETag"]);

        // Act
        var results = sut.Validate(new ValidationContext(sut)).ToList();

        // Assert
        var result = results.Should().ContainSingle().Which;
        result.MemberNames.Should().ContainSingle().Which.Should().Be("AllowedOrigins[0]");
    }

    [Fact]
    public void Given_InvalidNestedCorsRule_When_RequestValidate_Then_PrefixesNestedMemberNames()
    {
        // Arrange
        var sut = new TestStorageAccountRequest
        {
            Name = "storage-account",
            Location = "ignored",
            Kind = "ignored",
            AccessTier = "ignored",
            MinimumTlsVersion = "ignored",
            CorsRules =
            [
                CreateCorsRuleEntry(
                    allowedOrigins: ["https://contoso.com"],
                    allowedHeaders: ["bad/header"],
                    exposedHeaders: ["ETag"]),
            ],
        };

        // Act
        var results = sut.Validate(new ValidationContext(sut)).ToList();

        // Assert
        var result = results.Should().ContainSingle().Which;
        var memberName = result.MemberNames.Should().ContainSingle().Which;
        memberName.Should().StartWith("CorsRules[");
        memberName.Should().EndWith(".AllowedHeaders[0]");
    }

    private static CorsRuleEntry CreateCorsRuleEntry(
        List<string> allowedOrigins,
        List<string> allowedHeaders,
        List<string> exposedHeaders)
    {
        return new CorsRuleEntry
        {
            AllowedOrigins = allowedOrigins,
            AllowedMethods = ["GET"],
            AllowedHeaders = allowedHeaders,
            ExposedHeaders = exposedHeaders,
            MaxAgeInSeconds = 60,
        };
    }

    private sealed class TestStorageAccountRequest : StorageAccountRequestBase;
}