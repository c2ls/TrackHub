using Common.Infrastructure;
using Common.Infrastructure.Interceptors;
using Common.Mediator;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Common.Infrastructure.Tests.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private class TestEvent : BaseEvent { }

    private class TestEntity : BaseEntity
    {
        public int Id { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext() { }

        // Options ctor so a test can attach the real interceptor and exercise the full save pipeline.
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> Entities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
            }
        }
    }

    [Fact]
    public async Task DispatchDomainEvents_WithEvents_PublishesAndClears()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        using var context = new TestDbContext();
        var entity = new TestEntity { Id = 1 };
        var evt1 = new TestEvent();
        var evt2 = new TestEvent();
        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);
        context.Entities.Add(entity);

        interceptor.CollectDomainEvents(context);
        await interceptor.PublishPendingAsync();

        publisher.Verify(p => p.Publish(It.Is<BaseEvent>(e => e == evt1), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.Publish(It.Is<BaseEvent>(e => e == evt2), It.IsAny<CancellationToken>()), Times.Once);
        entity.DomainEvents.Should().BeEmpty();
    }

    // Regression: events were previously collected AFTER the save, by which point EF had detached
    // removed entries — so every domain event raised on a deleted entity was silently dropped.
    [Fact]
    public async Task SaveChanges_DeletedEntity_StillPublishesItsDomainEvents()
    {
        var published = new List<BaseEvent>();
        var publisher = new Mock<IPublisher>();
        publisher.Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => { if (n is BaseEvent e) published.Add(e); })
            .Returns(Task.CompletedTask);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .AddInterceptors(new DispatchDomainEventsInterceptor(publisher.Object))
            .Options;

        using var context = new TestDbContext(options);
        var entity = new TestEntity { Id = 1 };
        context.Entities.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var deletedEvent = new TestEvent();
        entity.AddDomainEvent(deletedEvent);
        context.Entities.Remove(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        published.Should().Contain(deletedEvent);
    }

    [Fact]
    public async Task SaveChanges_WhenSaveFails_DoesNotPublishOnTheNextSave()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        using var context = new TestDbContext();
        var entity = new TestEntity { Id = 1 };
        entity.AddDomainEvent(new TestEvent());
        context.Entities.Add(entity);

        // Collected, then the save fails — the batch must be dropped, not carried forward.
        interceptor.CollectDomainEvents(context);
        interceptor.SaveChangesFailed(new DbContextErrorEventData(null!, null!, context, new InvalidOperationException()));
        await interceptor.PublishPendingAsync();

        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchDomainEvents_NoEvents_DoesNotPublish()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        using var context = new TestDbContext();
        var entity = new TestEntity { Id = 1 };
        context.Entities.Add(entity);

        interceptor.CollectDomainEvents(context);
        await interceptor.PublishPendingAsync();

        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void CollectDomainEvents_NullContext_DoesNotThrow()
    {
        var publisher = new Mock<IPublisher>();
        var interceptor = new DispatchDomainEventsInterceptor(publisher.Object);

        var act = () => interceptor.CollectDomainEvents(null);
        act.Should().NotThrow();
    }
}
