using Common.Web.Transformers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Moq;

namespace Common.Web.Tests.Transformers;

public class BearerSecuritySchemeTransformerTests
{
    private static OpenApiDocumentTransformerContext CreateContext() =>
        new()
        {
            DocumentName = "v1",
            ApplicationServices = new Mock<IServiceProvider>().Object,
            DescriptionGroups = []
        };

    [Fact]
    public async Task TransformAsync_WithBearerScheme_AddsSecurityScheme()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(p => p.GetAllSchemesAsync())
            .ReturnsAsync([new AuthenticationScheme("Bearer", "Bearer", typeof(TestHandler))]);

        var transformer = new BearerSecuritySchemeTransformer(schemeProvider.Object);
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new OpenApiOperation()
                    }
                }
            }
        };

        await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

        document.Components.Should().NotBeNull();
        document.Paths["/api/test"]!.Operations![HttpMethod.Get].Security.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WithoutBearerScheme_DoesNotModifyDocument()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(p => p.GetAllSchemesAsync())
            .ReturnsAsync([new AuthenticationScheme("Cookies", "Cookies", typeof(TestHandler))]);

        var transformer = new BearerSecuritySchemeTransformer(schemeProvider.Object);
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new OpenApiOperation()
                    }
                }
            }
        };

        await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

        document.Components.Should().BeNull();
    }

    [Fact]
    public async Task TransformAsync_WithNullOperations_SkipsPath()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(p => p.GetAllSchemesAsync())
            .ReturnsAsync([new AuthenticationScheme("Bearer", "Bearer", typeof(TestHandler))]);

        var transformer = new BearerSecuritySchemeTransformer(schemeProvider.Object);
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/test"] = new OpenApiPathItem()
            }
        };

        await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);
        document.Components.Should().NotBeNull();
    }
}

internal class TestHandler : IAuthenticationHandler
{
    public Task<AuthenticateResult> AuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
    public Task ChallengeAsync(AuthenticationProperties? properties) => Task.CompletedTask;
    public Task ForbidAsync(AuthenticationProperties? properties) => Task.CompletedTask;
    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
}
