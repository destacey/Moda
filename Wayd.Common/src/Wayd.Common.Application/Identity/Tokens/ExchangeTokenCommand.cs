namespace Wayd.Common.Application.Identity.Tokens;

/// <summary>
/// Exchanges an external identity-provider token for a Wayd JWT. The frontend
/// obtains <paramref name="IdToken"/> from an OIDC provider (e.g., Microsoft Entra
/// via MSAL) and hands it to <c>/api/auth/exchange</c>; the server validates it
/// and mints a Wayd JWT carrying permission claims.
/// </summary>
/// <param name="Provider">Provider name — e.g., <c>"MicrosoftEntraId"</c>.</param>
/// <param name="IdToken">The id token issued by the external provider.</param>
public sealed record ExchangeTokenCommand(string Provider, string IdToken);
