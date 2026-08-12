using Common.Infrastructure;
using FluentAssertions;

namespace Common.Infrastructure.Tests;

public class BaseEntityTests
{
    private class TestEvent : BaseEvent { }

    private class ConcreteEntity : BaseEntity { }

    [Fact]
    public void DomainEvents_InitiallyEmpty()
    {
        var entity = new ConcreteEntity();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_AddsEvent()
    {
        var entity = new ConcreteEntity();
        var evt = new TestEvent();
        entity.AddDomainEvent(evt);
        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(evt);
    }

    [Fact]
    public void RemoveDomainEvent_RemovesEvent()
    {
        var entity = new ConcreteEntity();
        var evt = new TestEvent();
        entity.AddDomainEvent(evt);
        entity.RemoveDomainEvent(evt);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ClearsAll()
    {
        var entity = new ConcreteEntity();
        entity.AddDomainEvent(new TestEvent());
        entity.AddDomainEvent(new TestEvent());
        entity.ClearDomainEvents();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddMultipleEvents_AllPresent()
    {
        var entity = new ConcreteEntity();
        var e1 = new TestEvent();
        var e2 = new TestEvent();
        entity.AddDomainEvent(e1);
        entity.AddDomainEvent(e2);
        entity.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void DomainEvents_IsReadOnly()
    {
        var entity = new ConcreteEntity();
        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<BaseEvent>>();
    }
}
