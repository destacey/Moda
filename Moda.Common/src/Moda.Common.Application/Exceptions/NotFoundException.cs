using System.Net;

namespace Moda.Common.Application.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, null, HttpStatusCode.NotFound)
    {
    }
}