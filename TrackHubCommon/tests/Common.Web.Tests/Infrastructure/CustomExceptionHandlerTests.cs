using Common.Application.Exceptions;
using Common.Web.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Common.Web.Tests.Infrastructure;

public class CustomExceptionHandlerTests
{
    private readonly CustomExceptionHandler _handler = new();

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400()
    {
        var context = CreateHttpContext();
        var exception = new ValidationException(
            new[] { new FluentValidation.Results.ValidationFailure("Name", "Required") });

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404()
    {
        var context = CreateHttpContext();
        var exception = new Ardalis.GuardClauses.NotFoundException("item", "123");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = await ReadProblemDetails(context);
        problemDetails!.Status.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401()
    {
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException();

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenAccessException_Returns403()
    {
        var context = CreateHttpContext();
        var exception = new ForbiddenAccessException("Users", "Read");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = await ReadProblemDetails(context);
        problemDetails!.Detail.Should().Be("Insufficient permissions. Required permission: Users.Read.");
    }

    [Fact]
    public async Task TryHandleAsync_TooManyRequestsException_Returns429()
    {
        var context = CreateHttpContext();
        var exception = new TooManyRequestsException("Too many") { RetryAfterSeconds = 30 };

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        context.Response.Headers["Retry-After"].ToString().Should().Be("30");
    }

    [Fact]
    public async Task TryHandleAsync_TooManyRequestsException_NoRetryAfter_DoesNotSetHeader()
    {
        var context = CreateHttpContext();
        var exception = new TooManyRequestsException("Too many");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.Headers.ContainsKey("Retry-After").Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Unknown");

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var problemDetails = await ReadProblemDetails(context);
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("An error occurred while processing your request.");
    }
}
