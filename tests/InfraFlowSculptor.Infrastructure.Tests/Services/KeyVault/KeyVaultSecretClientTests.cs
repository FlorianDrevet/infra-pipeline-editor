using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using InfraFlowSculptor.Infrastructure.Services.KeyVault;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services.KeyVault;

public sealed class KeyVaultSecretClientTests
{
    private readonly SecretClient _secretClient;
    private readonly KeyVaultSecretClient _sut;

    public KeyVaultSecretClientTests()
    {
        _secretClient = Substitute.For<SecretClient>(
            new Uri("https://unit-test.vault.azure.net/"),
            Substitute.For<TokenCredential>());
        var logger = Substitute.For<ILogger<KeyVaultSecretClient>>();
        _sut = new KeyVaultSecretClient(_secretClient, logger);
    }

    [Fact]
    public async Task Given_ForbiddenKeyVaultWrite_When_SetSecretAsync_Then_ReturnsHumanReadableStorageErrorAsync()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(403, "Caller is not authorized to perform action.");
        _secretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<KeyVaultSecret>>(requestFailedException));

        // Act
        var result = await _sut.SetSecretAsync("git-pat-repo-123", "token-value", CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("GitRepository.SecretStorageFailed");
        result.FirstError.Description.Should().Be(
            "Failed to store the authentication token in Key Vault: the application is not allowed to write secrets to the configured Key Vault.");
    }
}