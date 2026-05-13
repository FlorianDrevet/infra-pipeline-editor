using FluentAssertions;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Tests.Common.Models;

public sealed class AggregateRootDomainEventsTests
{
    [Fact]
    public void Given_RaisedDomainEvent_When_AccessingDomainEvents_Then_ContainsEvent()
    {
        // Arrange
        var sut = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestDomainEvent(sut.Id);

        // Act
        sut.Raise(domainEvent);

        // Assert
        sut.DomainEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void Given_RaisedDomainEvents_When_ClearDomainEvents_Then_RemovesAllEvents()
    {
        // Arrange
        var sut = new TestAggregateRoot(Guid.NewGuid());
        sut.Raise(new TestDomainEvent(sut.Id));

        // Act
        sut.ClearDomainEvents();

        // Assert
        sut.DomainEvents.Should().BeEmpty();
    }

    private sealed class TestAggregateRoot(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Raise(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent(Guid AggregateId) : IDomainEvent;
}