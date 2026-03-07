namespace Moda.Common.Application.Identity.Tokens;

public sealed record LoginCommand(string UserName, string Password);
