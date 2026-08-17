using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using CRM.WebFrontend.Client.Providers;

namespace CRM.WebFrontend.Providers;

public class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(PersistentComponentState state)
    {
        _state = state;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            return;
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? principal.FindFirst("id_user")?.Value 
                         ?? string.Empty;
            var username = principal.FindFirst("username")?.Value ?? string.Empty;
            var name = principal.Identity.Name ?? string.Empty;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value 
                       ?? principal.FindFirst("rol")?.Value 
                       ?? string.Empty;
            var campaign = principal.FindFirst("campaign")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                var token = principal.FindFirst("access_token")?.Value ?? string.Empty;
                _state.PersistAsJson("UserInfo", new UserInfo
                {
                    UserId = userId,
                    Username = username,
                    Name = name,
                    Role = role,
                    Campaign = campaign,
                    Token = token
                });
            }
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
