using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;

public sealed class SetInfraConfigLayoutModeCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _repository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly SetInfraConfigLayoutModeCommandHandler _sut;

    public SetInfraConfigLayoutModeCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _repository.GetByIdAsync(_config.Id)
            .Returns(_config);

        _sut = new SetInfraConfigLayoutModeCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_InvalidLayoutMode_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new SetInfraConfigLayoutModeCommand(_project.Id, _config.Id, "unsupported");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("InfrastructureConfig.InvalidLayoutMode");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<DomainInfrastructureConfig>());
    }
}