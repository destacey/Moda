namespace Moda.Infrastructure;

public static class ServiceEndpoints
{
    public const string AlivenessEndpointPath = "/alive";
    public const string HealthEndpointPath = "/api/health";
    public const string StartupEndpointPath = "/startup";
}

public static class AuthConstants
{
    public const string ApiKeyHeaderName = "x-api-key";
}
