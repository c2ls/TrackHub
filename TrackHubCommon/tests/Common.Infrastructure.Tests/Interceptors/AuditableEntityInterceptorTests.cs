using Common.Application.Interfaces;
using Common.Infrastructure;
using Common.Infrastructure.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Common.Infrastructure.Tests.Interceptors;

public class AuditableEntityInterceptorTests
{
    private class TestAuditableEntity : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestAuditableEntity> Entities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
    }

    [Fact]
    public void UpdateEntities_AddedEntity_SetsCreatedFields()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns("user-123");
        var fakeTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(t => t.GetUtcNow()).Returns(fakeTime);

        var interceptor = new AuditableEntityInterceptor(user.Object, timeProvider.Object);

        using var context = new TestDbContext();
        var entity = new TestAuditableEntity { Id = 1, Name = "Test" };
        context.Entities.Add(entity);

        interceptor.UpdateEntities(context);

        entity.CreatedBy.Should().Be("user-123");
        entity.Created.Should().Be(fakeTime);
        entity.LastModifiedBy.Should().Be("user-123");
        entity.LastModified.Should().Be(fakeTime);
    }

    [Fact]
    public void UpdateEntities_ModifiedEntity_SetsLastModifiedFields()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns("user-456");
        var fakeTime = new DateTimeOffset(2026, 2, 20, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(t => t.GetUtcNow()).Returns(fakeTime);

        var interceptor = new AuditableEntityInterceptor(user.Object, timeProvider.Object);

        using var context = new TestDbContext();
        var entity = new TestAuditableEntity { Id = 1, Name = "Test", CreatedBy = "original", Created = DateTimeOffset.UtcNow.AddDays(-1) };
        context.Entities.Add(entity);
        context.SaveChanges();

        entity.Name = "Updated";
        context.Entry(entity).State = EntityState.Modified;

        interceptor.UpdateEntities(context);

        entity.LastModifiedBy.Should().Be("user-456");
        entity.LastModified.Should().Be(fakeTime);
    }

    [Fact]
    public void UpdateEntities_NullContext_DoesNotThrow()
    {
        var user = new Mock<IUser>();
        var timeProvider = new Mock<TimeProvider>();
        var interceptor = new AuditableEntityInterceptor(user.Object, timeProvider.Object);

        var act = () => interceptor.UpdateEntities(null);
        act.Should().NotThrow();
    }
}
