﻿using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Security.Claims;
using Moda.Common.Domain.Authorization;
using Moda.Web.BlazorClient.Infrastructure.ApiClient;

namespace Moda.Web.BlazorClient.Infrastructure.Auth.AzureAd;

internal class AzureAdClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    // Can't work with actual services in the constructor here, have to
    // use IServiceProvider, otherwise the app hangs at startup.
    // The culprit is probably HttpClient, as this class is instantiated
    // at startup while the HttpClient is being (or not even) created.
    private readonly IServiceProvider _services;

    public AzureAdClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, IServiceProvider services)
        : base(accessor) =>
        _services = services;

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var principal = await base.CreateUserAsync(account, options);

        if (principal.Identity?.IsAuthenticated is true)
        {
            var userDetails = await _services.GetRequiredService<IProfileClient>().GetAsync();

            var userIdentity = (ClaimsIdentity)principal.Identity;

            if (!string.IsNullOrWhiteSpace(userDetails.Email) && !userIdentity.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Email, userDetails.Email));
            }

            if (!string.IsNullOrWhiteSpace(userDetails.PhoneNumber) && !userIdentity.HasClaim(c => c.Type == ClaimTypes.MobilePhone))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.MobilePhone, userDetails.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(userDetails.FirstName) && !userIdentity.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Name, userDetails.FirstName));
            }

            if (!string.IsNullOrWhiteSpace(userDetails.LastName) && !userIdentity.HasClaim(c => c.Type == ClaimTypes.Surname))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Surname, userDetails.LastName));
            }

            if (!userIdentity.HasClaim(c => c.Type == ApplicationClaims.Fullname))
            {
                userIdentity.AddClaim(new Claim(ApplicationClaims.Fullname, $"{userDetails.FirstName} {userDetails.LastName}"));
            }

            if (!userIdentity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userDetails.Id.ToString()));
            }

            //    if (!string.IsNullOrWhiteSpace(userDetails.ImageUrl) && !userIdentity.HasClaim(c => c.Type == ApplicationClaims.ImageUrl) && userDetails.ImageUrl is not null)
            //    {
            //        userIdentity.AddClaim(new Claim(ApplicationClaims.ImageUrl, userDetails.ImageUrl));
            //    }

            var permissions = await _services.GetRequiredService<IProfileClient>().GetPermissionsAsync();

            userIdentity.AddClaims(permissions.Select(permission => new Claim(ApplicationClaims.Permission, permission)));
        }

        return principal;
    }
}
