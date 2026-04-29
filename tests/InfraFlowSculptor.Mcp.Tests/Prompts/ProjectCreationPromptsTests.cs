using FluentAssertions;
using InfraFlowSculptor.Mcp.Prompts;
using Microsoft.Extensions.AI;

namespace InfraFlowSculptor.Mcp.Tests.Prompts;

public sealed class ProjectCreationPromptsTests
{
    [Fact]
    public void GetProjectCreationGuide_SubscriptionRule_IsOptionalAndNonBlocking()
    {
        // Act
        var message = ProjectCreationPrompts.GetProjectCreationGuide();
        var text = message.Contents.OfType<TextContent>().Single().Text;

        // Assert
        message.Role.Should().Be(ChatRole.User);
        text.Should().Contain("optional when creating environments");
        text.Should().Contain("must never block");
    }
}