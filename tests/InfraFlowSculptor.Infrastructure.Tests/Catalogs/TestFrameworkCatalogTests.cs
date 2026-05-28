using FluentAssertions;
using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;
using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks.Stacks;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;
using InfraFlowSculptor.Infrastructure.Catalogs;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Catalogs;

public sealed class TestFrameworkCatalogTests
{
    private readonly TestFrameworkCatalog _sut = new();

    [Fact]
    public void GetAll_ReturnsAllDefinitionsAcrossAllStacks()
    {
        // Arrange & Act
        var all = _sut.GetAll();

        // Assert
        all.Should().NotBeEmpty();
        all.Count.Should().Be(
            DotNetTestFrameworks.All.Count
            + NodeJsTestFrameworks.All.Count
            + AngularTestFrameworks.All.Count
            + JavaTestFrameworks.All.Count
            + PythonTestFrameworks.All.Count);
    }

    [Theory]
    [InlineData(ApplicationStack.ApplicationStackEnum.DotNet, 3)]
    [InlineData(ApplicationStack.ApplicationStackEnum.NodeJs, 3)]
    [InlineData(ApplicationStack.ApplicationStackEnum.Angular, 3)]
    [InlineData(ApplicationStack.ApplicationStackEnum.Java, 3)]
    [InlineData(ApplicationStack.ApplicationStackEnum.Python, 2)]
    public void GetFrameworksForStack_ReturnsExpectedCount(
        ApplicationStack.ApplicationStackEnum stack, int expectedCount)
    {
        // Act
        var frameworks = _sut.GetFrameworksForStack(stack);

        // Assert
        frameworks.Should().HaveCount(expectedCount);
        frameworks.Should().OnlyContain(f => f.Key.Stack == stack);
    }

    [Fact]
    public void GetFrameworksForStack_UnsupportedStack_ReturnsEmpty()
    {
        // Act
        var frameworks = _sut.GetFrameworksForStack(ApplicationStack.ApplicationStackEnum.StaticSite);

        // Assert
        frameworks.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ApplicationStack.ApplicationStackEnum.DotNet, "XUnit")]
    [InlineData(ApplicationStack.ApplicationStackEnum.NodeJs, "Jest")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Angular, "Jasmine")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Java, "JUnit5")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Python, "Pytest")]
    public void GetByKey_ExistingKey_ReturnsDefinition(
        ApplicationStack.ApplicationStackEnum stack, string framework)
    {
        // Arrange
        var key = new TestFrameworkKey(stack, framework);

        // Act
        var result = _sut.GetByKey(key);

        // Assert
        result.Should().NotBeNull();
        result!.Key.Should().Be(key);
        result.DisplayName.Should().NotBeNullOrWhiteSpace();
        result.DefaultCommand.Should().NotBeNullOrWhiteSpace();
        result.DefaultResultsFormat.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetByKey_NonExistingKey_ReturnsNull()
    {
        // Arrange
        var key = new TestFrameworkKey(ApplicationStack.ApplicationStackEnum.DotNet, "NonExistent");

        // Act
        var result = _sut.GetByKey(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSupportedStacks_ReturnsAllStacksWithFrameworks()
    {
        // Act
        var stacks = _sut.GetSupportedStacks();

        // Assert
        stacks.Should().Contain(ApplicationStack.ApplicationStackEnum.DotNet);
        stacks.Should().Contain(ApplicationStack.ApplicationStackEnum.NodeJs);
        stacks.Should().Contain(ApplicationStack.ApplicationStackEnum.Angular);
        stacks.Should().Contain(ApplicationStack.ApplicationStackEnum.Java);
        stacks.Should().Contain(ApplicationStack.ApplicationStackEnum.Python);
        stacks.Should().NotContain(ApplicationStack.ApplicationStackEnum.StaticSite);
        stacks.Should().NotContain(ApplicationStack.ApplicationStackEnum.Custom);
        stacks.Should().NotContain(ApplicationStack.ApplicationStackEnum.Unknown);
    }

    [Theory]
    [InlineData(ApplicationStack.ApplicationStackEnum.DotNet, "VSTest")]
    [InlineData(ApplicationStack.ApplicationStackEnum.NodeJs, "JUnit")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Angular, "JUnit")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Java, "JUnit")]
    [InlineData(ApplicationStack.ApplicationStackEnum.Python, "JUnit")]
    public void AllFrameworksForStack_HaveConsistentResultsFormat(
        ApplicationStack.ApplicationStackEnum stack, string expectedFormat)
    {
        // Act
        var frameworks = _sut.GetFrameworksForStack(stack);

        // Assert
        frameworks.Should().OnlyContain(f => f.DefaultResultsFormat == expectedFormat);
    }
}
