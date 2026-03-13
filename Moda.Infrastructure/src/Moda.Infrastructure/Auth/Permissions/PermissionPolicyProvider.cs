using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moda.Infrastructure.Auth.Local;

namespace Moda.Infrastructure.Auth.Permissions;

internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Check if the x-api-key header is present
        var hasApiKeyHeader = httpContext?.Request.Headers.ContainsKey(AuthConstants.ApiKeyHeaderName) == true;

        AuthorizationPolicy policy;
        if (hasApiKeyHeader)
        {
            // If PAT header is present, ONLY use PersonalAccessToken authentication
            // This ensures we don't try JWT authentication when the user is explicitly using a PAT
            policy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("PersonalAccessToken")
                .RequireAuthenticatedUser()
                .Build();
        }
        else
        {
            // If no PAT header, use both Azure AD JWT bearer and Local JWT schemes
            policy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(
                    Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                    Local.ConfigureServices.LocalJwtScheme)
                .RequireAuthenticatedUser()
                .Build();
        }

        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {

        if (policyName.StartsWith(ApplicationClaims.Permission, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        if (policyName.StartsWith("MustHaveAnyPermission:", StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName["MustHaveAnyPermission:".Length..].Split(',');
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new AnyPermissionRequirement(permissions));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);
}