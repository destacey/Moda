using System.Diagnostics;
using System.Net;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moda.Web.Api.Extensions;
using Moq;

namespace Moda.Web.Api.Tests.Sut.Extensions;
public sealed class ResultExtensionsTests
{
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<IHttpActivityFeature> _mockActivityFeature;

    public ResultExtensionsTests()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockActivityFeature = new Mock<IHttpActivityFeature>();
        _mockHttpContext.Setup(c => c.Features.Get<IHttpActivityFeature>()).Returns(_mockActivityFeature.Object);
    }

    [Fact]
    public void ToBadRequestObject_Result_ShouldReturnProblemDetails()
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

        var result = Result.Failure(error);

        // Act
        var problemDetails = result.ToBadRequestObject(_mockHttpContext.Object);

        // Assert
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Be(error);
        problemDetails.Instance.Should().Be("GET /test");
        problemDetails.Extensions["requestId"].Should().Be(traceIdentifier);
        problemDetails.Extensions["traceId"].Should().Be(activity.Id);
        activity.Stop();
    }

    [Fact]
    public void ToBadRequestObject_Result_ShouldThrowInvalidOperationException_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        Action act = () => result.ToBadRequestObject(_mockHttpContext.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result is successful. Use ToBadRequestObject() for failed results only.");
    }

    [Fact]
    public void ToBadRequestObject_ResultT_ShouldReturnProblemDetails()
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

        var result = Result.Failure<string>(error);

        // Act
        var problemDetails = result.ToBadRequestObject(_mockHttpContext.Object);

        // Assert
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Be(error);
        problemDetails.Instance.Should().Be("GET /test");
        problemDetails.Extensions["requestId"].Should().Be(traceIdentifier);
        problemDetails.Extensions["traceId"].Should().Be(activity.Id);
        activity.Stop();
    }

    [Fact]
    public void ToBadRequestObject_ResultT_ShouldThrowInvalidOperationException_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success("Success");

        // Act
        Action act = () => result.ToBadRequestObject(_mockHttpContext.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result is successful. Use ToBadRequestObject() for failed results only.");
    }
}
