using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.DomainEvents;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Events;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.DomainEvents;

public sealed class ProjectDbContextDomainEventsTests
{
    [Fact]
    public async Task Given_ProjectCreatedDomainEvent_When_SaveChangesAsync_Then_DispatchesAfterPersistenceAndClearsEvents_Async()
    {
        // Arrange
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var project = Project.Create(new Name("alpha"), "primary workload", UserId.CreateUnique());
        var dispatcherObservedPersistedState = false;
        IReadOnlyCollection<IDomainEvent>? dispatchedEvents = null;

        await using var context = new ProjectDbContext(CreateOptions(), dispatcher);
        await context.Projects.AddAsync(project);

        dispatcher.DispatchAsync(Arg.Any<IReadOnlyCollection<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                dispatchedEvents = callInfo.ArgAt<IReadOnlyCollection<IDomainEvent>>(0);
                context.Entry(project).State.Should().Be(EntityState.Unchanged);
                dispatcherObservedPersistedState = true;
                return Task.CompletedTask;
            });

        // Act
        await context.SaveChangesAsync();

        // Assert
        dispatcherObservedPersistedState.Should().BeTrue();
        project.DomainEvents.Should().BeEmpty();
        dispatchedEvents.Should().NotBeNull();
        var domainEvent = dispatchedEvents!.Should().ContainSingle().Which;
        domainEvent.Should().BeOfType<ProjectCreatedDomainEvent>();
        domainEvent.As<ProjectCreatedDomainEvent>().ProjectId.Should().Be(project.Id);
        await dispatcher.Received(1).DispatchAsync(Arg.Any<IReadOnlyCollection<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ContextCreatedWithoutDispatcher_When_SaveChangesAsync_Then_PersistsSafely_Async()
    {
        // Arrange
        var project = Project.Create(new Name("alpha"), "primary workload", UserId.CreateUnique());

        await using var context = new ProjectDbContext(CreateOptions());
        await context.Projects.AddAsync(project);

        // Act
        var writtenEntries = await context.SaveChangesAsync();

        // Assert
        writtenEntries.Should().BeGreaterThan(0);
        project.DomainEvents.Should().BeEmpty();
        (await context.Projects.CountAsync()).Should().Be(1);
    }

    private static DbContextOptions<ProjectDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase($"project-domain-events-{Guid.NewGuid()}")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }
}