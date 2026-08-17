using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CRM.WebFrontend.Client.Providers;

public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _authenticationStateTask;

    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>("UserInfo", out var userInfo) || userInfo is null)
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            _authenticationStateTask = Task.FromResult(new AuthenticationState(user));
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim("id_user", userInfo.UserId),
            new Claim("username", userInfo.Username),
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim(ClaimTypes.Role, userInfo.Role),
            new Claim("campaign", userInfo.Campaign),
            new Claim("access_token", userInfo.Token)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "PersistentAuth");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        _authenticationStateTask = Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
