using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class SchemaMetadataTests
{
    [Fact]
    public void Application_IsApp() => SchemaMetadata.Application.Should().Be("app");

    [Fact]
    public void Geofencing_IsGeofencing() => SchemaMetadata.Geofencing.Should().Be("geofencing");

    [Fact]
    public void Public_IsPublic() => SchemaMetadata.Public.Should().Be("public");

    [Fact]
    public void Security_IsSecurity() => SchemaMetadata.Security.Should().Be("security");

    [Fact]
    public void Trip_IsTrip() => SchemaMetadata.Trip.Should().Be("trip");
}
