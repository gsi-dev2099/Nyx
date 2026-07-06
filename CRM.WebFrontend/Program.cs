using CRM.WebFrontend.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient for calling the backend API
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5068");
});

// Configure native Cookie Authentication for Blazor Server
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NyxCRM.Auth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/not-found";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Configure YARP with Request Transformation to automatically inject the Bearer Token
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var tokenClaim = httpContext.User.FindFirst("access_token");
                if (tokenClaim != null)
                {
                    transformContext.ProxyRequest.Headers.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClaim.Value);
                }
            }
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Minimal API: Post Login Form Endpoint
app.MapPost("/login-endpoint", async (HttpContext httpContext, IHttpClientFactory httpClientFactory) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        return Results.Redirect("/login?error=Faltan credenciales");
    }

    try
    {
        var client = httpClientFactory.CreateClient("BackendApi");
        var payload = JsonSerializer.Serialize(new { username, password });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/auth/login", content);
        if (!response.IsSuccessStatusCode)
        {
            return Results.Redirect("/login?error=Credenciales incorrectas");
        }

        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var jsonStr = Encoding.UTF8.GetString(responseBytes);
        using var doc = JsonDocument.Parse(jsonStr);
        var root = doc.RootElement;
        
        var token = root.GetProperty("token").GetString();
        var refreshToken = root.GetProperty("refreshToken").GetString();

        // Get user details from /api/auth/me to know their name and role
        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var meResponse = await client.SendAsync(meRequest);
        
        string role = "ASESOR"; // default fallback
        string name = username;
        string campaignName = "";

        if (meResponse.IsSuccessStatusCode)
        {
            var meBytes = await meResponse.Content.ReadAsByteArrayAsync();
            var meJson = Encoding.UTF8.GetString(meBytes);
            using var meDoc = JsonDocument.Parse(meJson);
            var meRoot = meDoc.RootElement;
            name = meRoot.GetProperty("nombre").GetString() ?? username;
            role = meRoot.GetProperty("rol").GetString() ?? "ASESOR";
            if (meRoot.TryGetProperty("campanaAsignada", out var cmpProp))
            {
                campaignName = cmpProp.GetString() ?? "";
            }
        }

        // Parse UserID from JWT token claims
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var idUserClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id_user" || c.Type == "sub");
        var idUser = idUserClaim?.Value ?? "0";

        // Establish the Cookie Authentication Ticket using explicit System.Security.Claims.Claim
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, idUser),
            new System.Security.Claims.Claim("id_user", idUser),
            new System.Security.Claims.Claim("username", username),
            new System.Security.Claims.Claim(ClaimTypes.Name, name),
            new System.Security.Claims.Claim(ClaimTypes.Role, role),
            new System.Security.Claims.Claim("access_token", token ?? ""),
            new System.Security.Claims.Claim("refresh_token", refreshToken ?? ""),
            new System.Security.Claims.Claim("campaign", campaignName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Redirect based on role
        if (role.Equals("SUPERVISOR", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Redirect("/supervisor");
        }
        return Results.Redirect("/asesor");
    }
    catch (Exception ex)
    {
        return Results.Redirect($"/login?error=Error del sistema: {Uri.EscapeDataString(ex.Message)}");
    }
});

// Minimal API: Logout Endpoint
app.MapGet("/logout-endpoint", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// Map YARP Proxy Routes
app.MapReverseProxy();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
