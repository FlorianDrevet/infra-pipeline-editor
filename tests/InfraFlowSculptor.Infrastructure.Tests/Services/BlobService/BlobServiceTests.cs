using System.Reflection;
using Azure.Storage.Blobs;
using FluentAssertions;
using InfraFlowSculptor.Infrastructure.Services.BlobService;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services.BlobService;

public sealed class BlobServiceTests
{
    [Fact]
    public void Given_MissingContainerName_When_ConstructingBlobService_Then_UsesDefaultContainerName()
    {
        // Arrange
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var options = Options.Create(new BlobSettings
        {
            ContainerName = string.Empty,
        });

        // Act
        var sut = new InfraFlowSculptor.Infrastructure.Services.BlobService.BlobService(blobServiceClient, options);

        // Assert
        GetBlobContainerClient(sut).Name.Should().Be(BlobSettings.DefaultContainerName);
    }

    [Fact]
    public void Given_ConfiguredContainerName_When_ConstructingBlobService_Then_UsesConfiguredContainerName()
    {
        // Arrange
        const string configuredContainerName = "custom-output";
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var options = Options.Create(new BlobSettings
        {
            ContainerName = configuredContainerName,
        });

        // Act
        var sut = new InfraFlowSculptor.Infrastructure.Services.BlobService.BlobService(blobServiceClient, options);

        // Assert
        GetBlobContainerClient(sut).Name.Should().Be(configuredContainerName);
    }

    private static BlobContainerClient GetBlobContainerClient(InfraFlowSculptor.Infrastructure.Services.BlobService.BlobService sut)
    {
        var field = typeof(InfraFlowSculptor.Infrastructure.Services.BlobService.BlobService)
            .GetField("_blobContainerClient", BindingFlags.Instance | BindingFlags.NonPublic);

        field.Should().NotBeNull();
        return field!.GetValue(sut).Should().BeOfType<BlobContainerClient>().Subject;
    }
}