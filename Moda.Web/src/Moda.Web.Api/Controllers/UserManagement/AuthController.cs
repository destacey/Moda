using Microsoft.AspNetCore.Authorization;
using Moda.Common.Application.Identity.Tokens;

namespace Moda.Web.Api.Controllers.UserManagement;

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
}
