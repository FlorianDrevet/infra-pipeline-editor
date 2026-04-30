using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate;

public sealed class ProjectTests
{
    private const string DefaultProjectName = "Demo";

    // ─── Factory ───────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesOwnerMemberAndProperties()
    {
        // Arrange
        var ownerId = UserId.CreateUnique();
        var name = new Name(DefaultProjectName);
        const string description = "A demo project";

        // Act
        var sut = Project.Create(name, description, ownerId);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Should().Be(name);
        sut.Description.Should().Be(description);
        sut.LayoutPreset.Value.Should().Be(LayoutPresetEnum.MultiRepo);
        sut.Members.Should().ContainSingle();
        var owner = sut.Members.Single();
        owner.UserId.Should().Be(ownerId);
        owner.Role.Value.Should().Be(Role.RoleEnum.Owner);
    }

    // ─── Members ───────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewMemberAndRole_When_AddMember_Then_MemberIsAddedWithRole()
    {
        // Arrange
        var sut = CreateValidProject();
        var newUser = UserId.CreateUnique();

        // Act
        sut.AddMember(newUser, new Role(Role.RoleEnum.Contributor));

        // Assert
        sut.Members.Should().HaveCount(2);
        sut.Members.Should().Contain(m => m.UserId == newUser && m.Role.Value == Role.RoleEnum.Contributor);
    }

    [Fact]
    public void Given_ExistingMember_When_ChangeRole_Then_RoleIsUpdated()
    {
        // Arrange
        var sut = CreateValidProject();
        var memberId = UserId.CreateUnique();
        sut.AddMember(memberId, new Role(Role.RoleEnum.Reader));

        // Act
        sut.ChangeRole(memberId, new Role(Role.RoleEnum.Contributor));

        // Assert
        sut.Members.Single(m => m.UserId == memberId).Role.Value.Should().Be(Role.RoleEnum.Contributor);
    }

    [Fact]
    public void Given_UnknownMember_When_ChangeRole_Then_NoMembersAreModified()
    {
        // Arrange
        var sut = CreateValidProject();
        var ownerRoleBefore = sut.Members.Single().Role.Value;

        // Act
        sut.ChangeRole(UserId.CreateUnique(), new Role(Role.RoleEnum.Reader));

        // Assert
        sut.Members.Single().Role.Value.Should().Be(ownerRoleBefore);
    }

    [Fact]
    public void Given_ExistingMember_When_RemoveMember_Then_MemberIsRemoved()
    {
        // Arrange
        var sut = CreateValidProject();
        var memberId = UserId.CreateUnique();
        sut.AddMember(memberId, new Role(Role.RoleEnum.Reader));

        // Act
        sut.RemoveMember(memberId);

        // Assert
        sut.Members.Should().NotContain(m => m.UserId == memberId);
    }

    [Fact]
    public void Given_UnknownMember_When_RemoveMember_Then_MembersAreUnchanged()
    {
        // Arrange
        var sut = CreateValidProject();
        var initialCount = sut.Members.Count;

        // Act
        sut.RemoveMember(UserId.CreateUnique());

        // Assert
        sut.Members.Should().HaveCount(initialCount);
    }

    // ─── Pipeline Variable Groups ──────────────────────────────────────────

    [Fact]
    public void Given_NewGroupName_When_AddPipelineVariableGroup_Then_ReturnsCreatedGroup()
    {
        // Arrange
        var sut = CreateValidProject();
        const string groupName = "MyApp-Secrets";

        // Act
        var result = sut.AddPipelineVariableGroup(groupName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.GroupName.Should().Be(groupName);
        sut.PipelineVariableGroups.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateGroupName_When_AddPipelineVariableGroup_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidProject();
        const string groupName = "MyApp-Secrets";
        sut.AddPipelineVariableGroup(groupName);

        // Act
        var duplicate = sut.AddPipelineVariableGroup(groupName);

        // Assert
        duplicate.IsError.Should().BeTrue();
        duplicate.FirstError.Code.Should().Be("Project.DuplicateVariableGroup");
        sut.PipelineVariableGroups.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateGroupNameInDifferentCase_When_AddPipelineVariableGroup_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidProject();
        sut.AddPipelineVariableGroup("MyApp-Secrets");

        // Act
        var duplicate = sut.AddPipelineVariableGroup("myapp-secrets");

        // Assert
        duplicate.IsError.Should().BeTrue();
        duplicate.FirstError.Code.Should().Be("Project.DuplicateVariableGroup");
    }

    // ─── Default Naming Template ───────────────────────────────────────────

    [Fact]
    public void Given_NamingTemplate_When_SetDefaultNamingTemplate_Then_TemplateIsStored()
    {
        // Arrange
        var sut = CreateValidProject();
        var template = new NamingTemplate("{prefix}-{name}");

        // Act
        sut.SetDefaultNamingTemplate(template);

        // Assert
        sut.DefaultNamingTemplate.Should().Be(template);
    }

    [Fact]
    public void Given_ExistingTemplate_When_SetDefaultNamingTemplateToNull_Then_TemplateIsCleared()
    {
        // Arrange
        var sut = CreateValidProject();
        sut.SetDefaultNamingTemplate(new NamingTemplate("{name}"));

        // Act
        sut.SetDefaultNamingTemplate(null);

        // Assert
        sut.DefaultNamingTemplate.Should().BeNull();
    }

    // ─── Environment Definitions Ordering ──────────────────────────────────

    [Fact]
    public void Given_EmptyProject_When_AddEnvironment_Then_EnvironmentIsAddedAtRequestedOrder()
    {
        // Arrange
        var sut = CreateValidProject();

        // Act
        var env = sut.AddEnvironment(BuildEnvData(order: 0, name: "Dev"));

        // Assert
        sut.EnvironmentDefinitions.Should().ContainSingle();
        env.Order.Value.Should().Be(0);
    }

    [Fact]
    public void Given_ExistingEnvironments_When_AddEnvironmentBeforeThem_Then_ExistingOrdersShiftUp()
    {
        // Arrange
        var sut = CreateValidProject();
        var dev = sut.AddEnvironment(BuildEnvData(order: 0, name: "Dev"));
        var prod = sut.AddEnvironment(BuildEnvData(order: 1, name: "Prod"));

        // Act — insert "QA" at position 1, between Dev and Prod
        var qa = sut.AddEnvironment(BuildEnvData(order: 1, name: "QA"));

        // Assert
        dev.Order.Value.Should().Be(0);
        qa.Order.Value.Should().Be(1);
        prod.Order.Value.Should().Be(2);
    }

    [Fact]
    public void Given_ThreeEnvironments_When_RemoveMiddle_Then_FollowingOrdersShiftDown()
    {
        // Arrange
        var sut = CreateValidProject();
        var dev = sut.AddEnvironment(BuildEnvData(order: 0, name: "Dev"));
        var qa = sut.AddEnvironment(BuildEnvData(order: 1, name: "QA"));
        var prod = sut.AddEnvironment(BuildEnvData(order: 2, name: "Prod"));

        // Act
        var removed = sut.RemoveEnvironment(qa.Id);

        // Assert
        removed.Should().BeTrue();
        dev.Order.Value.Should().Be(0);
        prod.Order.Value.Should().Be(1);
        sut.EnvironmentDefinitions.Should().HaveCount(2);
    }

    [Fact]
    public void Given_UnknownEnvironment_When_RemoveEnvironment_Then_ReturnsFalse()
    {
        // Arrange
        var sut = CreateValidProject();

        // Act
        var removed = sut.RemoveEnvironment(ProjectEnvironmentDefinitionId.CreateUnique());

        // Assert
        removed.Should().BeFalse();
    }

    [Fact]
    public void Given_ThreeEnvironments_When_UpdateMovesLastToFirst_Then_OthersShiftDown()
    {
        // Arrange
        var sut = CreateValidProject();
        var dev = sut.AddEnvironment(BuildEnvData(order: 0, name: "Dev"));
        var qa = sut.AddEnvironment(BuildEnvData(order: 1, name: "QA"));
        var prod = sut.AddEnvironment(BuildEnvData(order: 2, name: "Prod"));

        // Act — move "Prod" from order 2 to order 0
        var updated = sut.UpdateEnvironment(prod.Id, BuildEnvData(order: 0, name: "Prod"));

        // Assert
        updated.Should().NotBeNull();
        prod.Order.Value.Should().Be(0);
        dev.Order.Value.Should().Be(1);
        qa.Order.Value.Should().Be(2);
    }

    [Fact]
    public void Given_ThreeEnvironments_When_UpdateMovesFirstToLast_Then_OthersShiftUp()
    {
        // Arrange
        var sut = CreateValidProject();
        var dev = sut.AddEnvironment(BuildEnvData(order: 0, name: "Dev"));
        var qa = sut.AddEnvironment(BuildEnvData(order: 1, name: "QA"));
        var prod = sut.AddEnvironment(BuildEnvData(order: 2, name: "Prod"));

        // Act — move "Dev" from order 0 to order 2
        sut.UpdateEnvironment(dev.Id, BuildEnvData(order: 2, name: "Dev"));

        // Assert
        qa.Order.Value.Should().Be(0);
        prod.Order.Value.Should().Be(1);
        dev.Order.Value.Should().Be(2);
    }

    [Fact]
    public void Given_UnknownEnvironment_When_UpdateEnvironment_Then_ReturnsNull()
    {
        // Arrange
        var sut = CreateValidProject();

        // Act
        var updated = sut.UpdateEnvironment(
            ProjectEnvironmentDefinitionId.CreateUnique(),
            BuildEnvData(order: 0, name: "X"));

        // Assert
        updated.Should().BeNull();
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private static Project CreateValidProject()
        => Project.Create(new Name(DefaultProjectName), description: null, UserId.CreateUnique());

    private static EnvironmentDefinitionData BuildEnvData(int order, string name)
        => new(
            new Name(name),
            new ShortName(name.ToLowerInvariant()),
            new Prefix("p"),
            new Suffix("s"),
            new Location(Location.LocationEnum.WestEurope),
            new SubscriptionId(Guid.NewGuid()),
            new Order(order),
            new RequiresApproval(false),
            AzureResourceManagerConnection: null,
            Tags: []);
}
