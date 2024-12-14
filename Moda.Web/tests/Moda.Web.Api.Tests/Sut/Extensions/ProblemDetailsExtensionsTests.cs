using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moda.Web.Api.Extensions;
using Moq;

namespace Moda.Web.Api.Tests.Sut.Extensions;

public sealed class ProblemDetailsExtensionsTests
{
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<IHttpActivityFeature> _mockActivityFeature;

    public ProblemDetailsExtensionsTests()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockActivityFeature = new Mock<IHttpActivityFeature>();
        _mockHttpContext.Setup(c => c.Features.Get<IHttpActivityFeature>()).Returns(_mockActivityFeature.Object);
    }

    [Fact]
    public void ForBadRequest_ShouldReturnProblemDetails()
    {
        // Arrange
        var error = "Test error";
        var traceIdentifier = "trace-id";
        var activity = new Activity("TestActivity");
        activity.Start();
        _mockActivityFeature.Setup(f => f.Activity).Returns(activity);
        _mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceIdentifier);
        _mockHttpContext.Setup(c => c.Request.Method).Returns("GET");
        _mockHttpContext.Setup(c => c.Request.Path).Returns("/test");

        // Act
        var result = ProblemDetailsExtensions.ForBadRequest(error, _mockHttpContext.Object);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        result.Title.Should().Be("Bad Request");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be(error);
        result.Instance.Should().Be("GET /test");
        result.Extensions["requestId"].Should().Be(traceIdentifier);
        result.Extensions["traceId"].Should().Be(activity.Id);
        activity.Stop();
    }

    [Fact]
    public void ForUnknownIdOrKeyType_ShouldReturnProblemDetails()
    {
        // Arrange
        var traceIdentifier = "trace-id";
        _mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceIdentifier);
        _mockHttpContext.Setup(c => c.Request.Method).Returns("GET");
        _mockHttpContext.Setup(c => c.Request.Path).Returns("/test");

        // Act
        var result = ProblemDetailsExtensions.ForUnknownIdOrKeyType(_mockHttpContext.Object);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        result.Title.Should().Be("Bad Request");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be("Unknown id or key type.");
        result.Instance.Should().Be("GET /test");
        result.Extensions["requestId"].Should().Be(traceIdentifier);
    }

    [Fact]
    public void ForRouteParamMismatch_ShouldReturnProblemDetails()
    {
        // Arrange
        var traceIdentifier = "trace-id";
        _mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceIdentifier);
        _mockHttpContext.Setup(c => c.Request.Method).Returns("GET");
        _mockHttpContext.Setup(c => c.Request.Path).Returns("/test");

        // Act
        var result = ProblemDetailsExtensions.ForRouteParamMismatch(_mockHttpContext.Object);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        result.Title.Should().Be("Bad Request");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be("The route Id and request Id do not match.");
        result.Instance.Should().Be("GET /test");
        result.Extensions["requestId"].Should().Be(traceIdentifier);
    }

    [Fact]
    public void ForRouteParamMismatch_WithParameters_ShouldReturnProblemDetails()
    {
        // Arrange
        var routeParamName = "userId";
        var requestPropertyName = "userId";
        var traceIdentifier = "trace-id";
        _mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceIdentifier);
        _mockHttpContext.Setup(c => c.Request.Method).Returns("GET");
        _mockHttpContext.Setup(c => c.Request.Path).Returns("/test");

        // Act
        var result = ProblemDetailsExtensions.ForRouteParamMismatch(routeParamName, requestPropertyName, _mockHttpContext.Object);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        result.Title.Should().Be("Bad Request");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be("The route UserId and request userId do not match.");
        result.Instance.Should().Be("GET /test");
        result.Extensions["requestId"].Should().Be(traceIdentifier);
    }
}
