using System.Net.Http.Headers;
using System.Text;
using Ardalis.GuardClauses;
using RestSharp;

namespace Moda.Integrations.AzureDevOps.Extensions;
internal static class RestRequestExtensions
{
    internal static void AddAcceptHeaderWithApiVersion(this RestRequest request, string apiVersion)
    {
        Guard.Against.NullOrWhiteSpace(apiVersion);
        request.AddHeader("Accept", $"application/json;api-version={apiVersion}");
    }

    internal static void AddAuthorizationHeaderForPersonalAccessToken(this RestRequest request, string token)
    {
        Guard.Against.NullOrWhiteSpace(token);
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token)));
        var authorizationHeaderValue = new AuthenticationHeaderValue("Basic", credentials);
        request.AddHeader("Authorization", authorizationHeaderValue.ToString());
    }
}
