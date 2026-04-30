using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandlerTests
{
    private const string ProjectName = "RetailApi";
    private const string ProjectDescription = "Retail APIs";
    private const string ExpectedDefaultTemplate = "{name}-{resourceAbbr}{suffix}";
    private const string ExpectedResourceGroupTemplate = "{resourceAbbr}-{name}{suffix}";
    private const string ExpectedStorageAccountTemplate = "{name}{resourceAbbr}{envShort}";
    private const string ResourceGroupResourceType = "ResourceGroup";
    private const string StorageAccountResourceType = "StorageAccount";

    private readonly IProjectRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly CreateProjectCommandHandler _sut;

    public CreateProjectCommandHandlerTests()
    {
        _repository = Substitute.For<IProjectRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _repository.AddAsync(Arg.Any<Project>())
            .Returns(callInfo => Task.FromResult((Project)callInfo.Args()[0]));
        _sut = new CreateProjectCommandHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_PersistsProjectWithDefaultTemplatesAsync()
    {
        // Arrange
        var command = new CreateProjectCommand(ProjectName, ProjectDescription);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Value.Should().Be(ProjectName);
        result.Value.Description.Should().Be(ProjectDescription);
        result.Value.DefaultNamingTemplate.Should().Be(ExpectedDefaultTemplate);
        result.Value.ResourceNamingTemplates.Should()
            .Contain(t => t.ResourceType == ResourceGroupResourceType && t.Template == ExpectedResourceGroupTemplate);
        result.Value.ResourceNamingTemplates.Should()
            .Contain(t => t.ResourceType == StorageAccountResourceType && t.Template == ExpectedStorageAccountTemplate);

        await _repository.Received(1).AddAsync(Arg.Is<Project>(p =>
            p.Name.Value == ProjectName
            && p.Description == ProjectDescription
            && p.Members.Count == 1));
    }

    [Fact]
    public async Task Given_NullDescription_When_Handle_Then_PersistsProjectWithoutDescriptionAsync()
    {
        // Arrange
        var command = new CreateProjectCommand(ProjectName, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_AddsCallerAsOwnerMemberAsync()
    {
        // Arrange
        var command = new CreateProjectCommand(ProjectName, ProjectDescription);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<Project>(p =>
            p.Members.Count == 1 && p.Members.First().UserId == _userId));
    }
}
