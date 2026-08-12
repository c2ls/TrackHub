using Common.Infrastructure;
using FluentAssertions;

namespace Common.Infrastructure.Tests;

public class BaseEventTests
{
    private class TestEvent : BaseEvent { }

    [Fact]
    public void BaseEvent_ImplementsINotification()
    {
        var evt = new TestEvent();
        evt.Should().BeAssignableTo<Common.Mediator.INotification>();
    }
}
