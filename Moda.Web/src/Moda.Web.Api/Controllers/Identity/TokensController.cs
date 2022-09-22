using Moda.Core.Application.Identity.Tokens;

namespace Moda.Web.Api.Controllers.Identity;

public sealed class TokensController : VersionNeutralApiController
{
    private readonly ITokenService _tokenService;

    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    [AllowAnonymous]
    [OpenApiOperation("Request an access token using credentials.", "")]
    public Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
    {
        return _tokenService.GetTokenAsync(request, GetIpAddress(), cancellationToken);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [OpenApiOperation("Request an access token using a refresh token.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Search))]
    public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
    {
        return _tokenService.RefreshTokenAsync(request, GetIpAddress());
    }

    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
}