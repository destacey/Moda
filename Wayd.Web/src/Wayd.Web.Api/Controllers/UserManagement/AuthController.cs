using Microsoft.AspNetCore.Authorization;
using Wayd.Common.Application.Identity.Tokens;

namespace Wayd.Web.Api.Controllers.UserManagement;

[Route("api/auth")]
[ApiVersionNeutral]
[ApiController]
[AllowAnonymous]
public class AuthController(ITokenService tokenService) : ControllerBase
{
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("login")]
    [OpenApiOperation("Authenticate with username and password.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var response = await _tokenService.GetTokenAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [OpenApiOperation("Refresh an expired JWT token.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var response = await _tokenService.RefreshTokenAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("exchange")]
    [OpenApiOperation("Exchange an external identity-provider token (e.g., Microsoft Entra ID) for a Wayd JWT.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Exchange(ExchangeTokenCommand command, CancellationToken cancellationToken)
    {
        var response = await _tokenService.ExchangeTokenAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet("providers")]
    [OpenApiOperation("List the authentication providers enabled on this deployment.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthProvidersResponse>> GetProviders(CancellationToken cancellationToken)
    {
        // Anonymous and cheap — the frontend calls this before constructing any
        // OIDC client. The response contains only public OIDC client metadata
        // (Authority, ClientId, Scopes) per provider; AllowedTenantIds and any
        // future secrets are deliberately not exposed here.
        var response = await _tokenService.GetAuthProviders(cancellationToken);
        return Ok(response);
    }
}
