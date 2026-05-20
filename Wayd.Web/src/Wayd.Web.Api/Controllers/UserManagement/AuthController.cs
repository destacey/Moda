using Microsoft.AspNetCore.Authorization;
using Wayd.Common.Application.Identity;
using Wayd.Common.Application.Identity.Bootstrap;
using Wayd.Common.Application.Identity.Tokens;
using Wayd.Web.Api.Extensions;
using Wayd.Web.Api.Models.UserManagement.Users;

namespace Wayd.Web.Api.Controllers.UserManagement;

[Route("api/auth")]
[ApiVersionNeutral]
[ApiController]
[AllowAnonymous]
public class AuthController(
    ITokenService tokenService,
    IBootstrapTokenService bootstrapTokenService,
    IUserService userService) : ControllerBase
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly IBootstrapTokenService _bootstrapTokenService = bootstrapTokenService;
    private readonly IUserService _userService = userService;

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

    [HttpPost("setup")]
    [OpenApiOperation("Create the first admin user using the one-time bootstrap token.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TokenResponse>> Setup(SetupRequest request, CancellationToken cancellationToken)
    {
        if (!_bootstrapTokenService.IsActive)
            return Conflict(ProblemDetailsExtensions.ForConflict("Setup has already been completed.", HttpContext));

        if (!_bootstrapTokenService.Validate(request.Token))
            return BadRequest(ProblemDetailsExtensions.ForBadRequest("Invalid setup token.", HttpContext));

        // Double-check that no users exist — prevents a race where two concurrent
        // setup requests both pass the token check before one consumes it.
        var userCount = await _userService.GetCountAsync(cancellationToken);
        if (userCount > 0)
            return Conflict(ProblemDetailsExtensions.ForConflict("Setup has already been completed.", HttpContext));

        var createResult = await _userService.CreateAsync(new CreateUserCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            LoginProvider = LoginProviders.Wayd,
            Password = request.Password,
            MustChangePassword = false,
        }, cancellationToken);

        if (createResult.IsFailure)
            return BadRequest(createResult.ToBadRequestObject(HttpContext));

        var userId = createResult.Value;

        await _userService.AssignRolesAsync(
            new AssignUserRolesCommand(userId, [ApplicationRoles.Admin]),
            cancellationToken);

        // Consume the token only after successful user creation so a failed
        // attempt (e.g. validation error, duplicate email) doesn't force the
        // operator to restart the application to get a new token.
        _bootstrapTokenService.Consume();

        var token = await _tokenService.GetTokenAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(token);
    }
}
