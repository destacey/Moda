using CSharpFunctionalExtensions;

namespace Moda.Web.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToBadRequestObject(this Result result, HttpContext context)
    {
        return result.IsFailure
            ? ProblemDetailsExtensions.ForBadRequest(result.Error, context)
            : throw new InvalidOperationException("Result is successful. Use ToBadRequestObject() for failed results only.");
    }

    public static ProblemDetails ToBadRequestObject<T>(this Result<T> result, HttpContext context)
    {
        return result.IsFailure
            ? ProblemDetailsExtensions.ForBadRequest(result.Error, context)
            : throw new InvalidOperationException("Result is successful. Use ToBadRequestObject() for failed results only.");
    }
}
