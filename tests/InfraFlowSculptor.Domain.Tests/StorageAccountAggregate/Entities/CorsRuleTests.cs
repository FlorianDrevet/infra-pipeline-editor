using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using System.Reflection;

namespace InfraFlowSculptor.Domain.Tests.StorageAccountAggregate.Entities;

public sealed class CorsRuleTests
{
    [Fact]
    public void Given_CorsRuleType_When_InspectingEfConstructor_Then_ParameterlessConstructorIsNonPrivateAndInitializesReadOnlyCollections()
    {
        // Arrange
        var constructor = typeof(CorsRule).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        // Act
        var sut = constructor?.Invoke(null) as CorsRule;

        // Assert
        constructor.Should().NotBeNull();
        constructor!.IsPrivate.Should().BeFalse("the EF materialization constructor should not require a SuppressMessage attribute");
        sut.Should().NotBeNull();
        sut!.AllowedOrigins.Should().BeEmpty();
        sut.AllowedMethods.Should().BeEmpty();
        sut.AllowedHeaders.Should().BeEmpty();
        sut.ExposedHeaders.Should().BeEmpty();
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var storageAccountId = AzureResourceId.CreateUnique();
        var serviceType = new CorsServiceType(CorsServiceType.Service.Blob);
        var origins = new[] { "https://example.com" };
        var methods = new[] { "GET", "POST" };
        var allowedHeaders = new[] { "x-custom" };
        var exposedHeaders = new[] { "x-response" };
        const int maxAge = 3600;

        // Act
        var sut = CorsRule.Create(
            storageAccountId,
            serviceType,
            origins,
            methods,
            allowedHeaders,
            exposedHeaders,
            maxAge);

        // Assert
        sut.StorageAccountId.Should().Be(storageAccountId);
        sut.ServiceType.Should().Be(serviceType);
        sut.AllowedOrigins.Should().BeEquivalentTo(origins);
        sut.AllowedMethods.Should().BeEquivalentTo(methods);
        sut.AllowedHeaders.Should().BeEquivalentTo(allowedHeaders);
        sut.ExposedHeaders.Should().BeEquivalentTo(exposedHeaders);
        sut.MaxAgeInSeconds.Should().Be(maxAge);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_ReplacesAllProperties()
    {
        // Arrange
        var sut = CorsRule.Create(
            AzureResourceId.CreateUnique(),
            new CorsServiceType(CorsServiceType.Service.Blob),
            new[] { "https://old.example.com" },
            new[] { "GET" },
            Array.Empty<string>(),
            Array.Empty<string>(),
            60);
        var newServiceType = new CorsServiceType(CorsServiceType.Service.Table);
        var newOrigins = new[] { "https://new.example.com" };
        var newMethods = new[] { "GET", "PUT" };

        // Act
        sut.Update(newServiceType, newOrigins, newMethods, new[] { "x-h" }, new[] { "x-r" }, 7200);

        // Assert
        sut.ServiceType.Should().Be(newServiceType);
        sut.AllowedOrigins.Should().BeEquivalentTo(newOrigins);
        sut.AllowedMethods.Should().BeEquivalentTo(newMethods);
        sut.MaxAgeInSeconds.Should().Be(7200);
    }

    [Fact]
    public void Given_CreatedRule_When_ExposingCollectionProperties_Then_CollectionsAreReadOnly()
    {
        // Arrange
        var sut = CorsRule.Create(
            AzureResourceId.CreateUnique(),
            new CorsServiceType(CorsServiceType.Service.Blob),
            ["https://example.com"],
            ["GET"],
            ["x-custom"],
            ["x-response"],
            120);

        // Act
        var allowedOrigins = (ICollection<string>)sut.AllowedOrigins;
        var allowedMethods = (ICollection<string>)sut.AllowedMethods;
        var allowedHeaders = (ICollection<string>)sut.AllowedHeaders;
        var exposedHeaders = (ICollection<string>)sut.ExposedHeaders;

        // Assert
        sut.AllowedOrigins.Should().NotBeAssignableTo<List<string>>();
        sut.AllowedMethods.Should().NotBeAssignableTo<List<string>>();
        sut.AllowedHeaders.Should().NotBeAssignableTo<List<string>>();
        sut.ExposedHeaders.Should().NotBeAssignableTo<List<string>>();
        FluentActions.Invoking(() => allowedOrigins.Add("https://other.example.com")).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => allowedMethods.Add("POST")).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => allowedHeaders.Add("x-added")).Should().Throw<NotSupportedException>();
        FluentActions.Invoking(() => exposedHeaders.Add("x-added")).Should().Throw<NotSupportedException>();
    }
}
