using System.Net;

namespace Moda.Infrastructure.Middleware;

public class ErrorResult
{
    public List<string>? Messages { get; set; } = [];

    public string? Source { get; set; }
    public string? Exception { get; set; }
    public string? ErrorId { get; set; }
    public string? SupportMessage { get; set; }
    public int StatusCode { get; set; }

    public static ErrorResult Create(string message, string supportMessage, string source, string? exception = null, string? errorId = null)
    {
        return new ErrorResult
        {
            Source = source,
            Exception = exception,
            ErrorId = errorId,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Messages = [message],
            SupportMessage = supportMessage
        };
    }

    public static ErrorResult CreateBadRequest(string supportMessage, string source)
    {
        return new ErrorResult
        {
            Source = source,
            StatusCode = (int)HttpStatusCode.BadRequest,
            SupportMessage = supportMessage
        };
    }

    public static ErrorResult CreateUnknownIdOrKeyTypeBadRequest(string source)
    {
        return new ErrorResult
        {
            Source = source,
            StatusCode = (int)HttpStatusCode.BadRequest,
            SupportMessage = "Unknown id or key type.",
        };
    }
}