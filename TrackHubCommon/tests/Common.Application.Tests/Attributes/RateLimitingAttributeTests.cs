using Common.Application.Attributes;
using FluentAssertions;

namespace Common.Application.Tests.Attributes;

public class RateLimitingAttributeTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var attr = new RateLimitingAttribute();
        attr.PermitLimit.Should().Be(100);
        attr.WindowSeconds.Should().Be(60);
        attr.SegmentsPerWindow.Should().Be(1);
        attr.QueueLimit.Should().Be(0);
        attr.PartitionKey.Should().Be("user");
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new RateLimitingAttribute
        {
            PermitLimit = 50,
            WindowSeconds = 120,
            SegmentsPerWindow = 4,
            QueueLimit = 10,
            PartitionKey = "client"
        };
        attr.PermitLimit.Should().Be(50);
        attr.WindowSeconds.Should().Be(120);
        attr.SegmentsPerWindow.Should().Be(4);
        attr.QueueLimit.Should().Be(10);
        attr.PartitionKey.Should().Be("client");
    }
}
