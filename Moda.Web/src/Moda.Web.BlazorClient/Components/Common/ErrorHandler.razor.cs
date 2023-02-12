﻿using Microsoft.AspNetCore.Components;
using Moda.Web.BlazorClient.Infrastructure.Auth;
using MudBlazor;

namespace Moda.Web.BlazorClient.Components.Common;

public partial class ErrorHandler
{
    [Inject]
    public IAuthenticationService AuthService { get; set; } = default!;

    public List<Exception> _receivedExceptions = new();

    protected override async Task OnErrorAsync(Exception exception)
    {
        _receivedExceptions.Add(exception);
        switch (exception)
        {
            case UnauthorizedAccessException:
                await AuthService.LogOutAsync();
                Snackbar.Add("Authentication Failed", Severity.Error);
                break;
        }
    }

    public new void Recover()
    {
        _receivedExceptions.Clear();
        base.Recover();
    }
}