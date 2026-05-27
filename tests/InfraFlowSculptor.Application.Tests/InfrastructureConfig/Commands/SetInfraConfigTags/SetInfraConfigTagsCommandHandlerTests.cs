using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetInfraConfigTags;

public sealed class SetInfraConfigTagsCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly InfrastructureConfigId _configId;
    private readonly SetInfraConfigTagsCommandHandler _sut;

    public SetInfraConfigTagsCommandHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);
        _configId = _config.Id;

        _sut = new SetInfraConfigTagsCommandHandler(_accessService);
    }

    [Fact]
    public async Task Given_ValidTags_When_Handle_Then_ReturnsUpdatedAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_configId, Arg.Any<CancellationToken>())
            .Returns(_config);
        var tags = new List<(string Name, string Value)> { ("env", "prod"), ("team", "platform") };
        var command = new SetInfraConfigTagsCommand(_configId.Value, tags);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
    }

    [Fact]
    public async Task Given_EmptyTags_When_Handle_Then_ClearsAndReturnsUpdatedAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_configId, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = new SetInfraConfigTagsCommand(_configId.Value, []);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetInfraConfigTagsCommand(_configId.Value, [("env", "prod")]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }
}
