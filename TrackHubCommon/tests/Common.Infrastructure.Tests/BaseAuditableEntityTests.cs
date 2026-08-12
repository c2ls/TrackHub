using Common.Infrastructure;
using FluentAssertions;

namespace Common.Infrastructure.Tests;

public class BaseAuditableEntityTests
{
    private class ConcreteAuditableEntity : BaseAuditableEntity { }

    [Fact]
    public void Properties_CanBeSetAndRead()
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new ConcreteAuditableEntity
        {
            Created = now,
            CreatedBy = "user1",
            LastModified = now.AddMinutes(5),
            LastModifiedBy = "user2"
        };

        entity.Created.Should().Be(now);
        entity.CreatedBy.Should().Be("user1");
        entity.LastModified.Should().Be(now.AddMinutes(5));
        entity.LastModifiedBy.Should().Be("user2");
    }

    [Fact]
    public void InheritsFromBaseEntity()
    {
        var entity = new ConcreteAuditableEntity();
        entity.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void DomainEvents_WorkFromInheritance()
    {
        var entity = new ConcreteAuditableEntity();
        entity.DomainEvents.Should().BeEmpty();
    }
}
