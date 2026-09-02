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
    private readonly ILogger<KeyVaultSecretClient> _logger;
    private readonly SecretClient _secretClient;
    private readonly KeyVaultSecretClient _sut;

    public KeyVaultSecretClientTests()
    {
        _secretClient = Substitute.For<SecretClient>(
            new Uri("https://unit-test.vault.azure.net/"),
            Substitute.For<TokenCredential>());
        _logger = Substitute.For<ILogger<KeyVaultSecretClient>>();
        _sut = new KeyVaultSecretClient(_secretClient, _logger);
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
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Given_KeyVaultReadFailure_When_GetSecretAsync_Then_ReturnsGenericRetrievalErrorAndLogsAsync()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(403, "Caller is not authorized to perform action.");
        _secretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<KeyVaultSecret>>(requestFailedException));

        // Act
        var result = await _sut.GetSecretAsync("git-pat-repo-123", CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("GitRepository.SecretRetrievalFailed");
        result.FirstError.Description.Should().Be("Failed to retrieve the authentication token from Key Vault.");
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Given_GetSecretIsCanceled_When_GetSecretAsync_Then_PropagatesCancellationAsync()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);
        _secretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<Response<KeyVaultSecret>>(cancellationToken));

        // Act
        Func<Task> act = async () => await _sut.GetSecretAsync("git-pat-repo-123", cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _logger.DidNotReceiveWithAnyArgs().Log(default, default, default!, default, default!);
    }
}