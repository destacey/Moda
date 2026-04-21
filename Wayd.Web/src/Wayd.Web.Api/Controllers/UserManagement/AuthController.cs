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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TokenResponse>> Exchange(ExchangeTokenCommand command, CancellationToken cancellationToken)
    {
        var response = await _tokenService.ExchangeTokenAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet("providers")]
    [OpenApiOperation("List the authentication providers enabled on this deployment.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<AuthProvidersResponse> GetProviders()
    {
        // Anonymous and cheap — the frontend calls this before rendering the
        // login page to decide which provider buttons to show. No auth data
        // leaked: knowing "this deployment accepts Entra tokens" is not sensitive.
        return Ok(_tokenService.GetAuthProviders());
    }
}
