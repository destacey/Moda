namespace Moda.Common.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
    Task<TokenResponse> GetTokenAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
}
