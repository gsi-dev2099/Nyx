using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CRM.WebFrontend;

public class ServerAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public ServerAuthHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Try HTTP Context first (for initial SSR render)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.User.Identity?.IsAuthenticated == true)
        {
            var tokenClaim = httpContext.User.FindFirst("access_token");
            if (tokenClaim != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenClaim.Value);
            }
        }
        else
        {
            // Fallback for SignalR (InteractiveServer)
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var authStateProvider = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
                var authState = await authStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    var tokenClaim = authState.User.FindFirst("access_token");
                    if (tokenClaim != null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenClaim.Value);
                    }
                }
            }
            catch { /* Ignore if not in a valid scope */ }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
