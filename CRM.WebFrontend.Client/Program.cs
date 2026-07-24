using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using CRM.WebFrontend.Client.Providers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register named HttpClient pointing to the host (WebFrontend) for API requests
builder.Services.AddTransient<CRM.WebFrontend.Client.Services.MockBackendHandler>();
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<CRM.WebFrontend.Client.Services.MockBackendHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped<CRM.WebFrontend.Client.Services.NotificationService>();
builder.Services.AddScoped<CRM.WebFrontend.Client.Services.IKbService, CRM.WebFrontend.Client.Services.KbService>();
builder.Services.AddScoped<CRM.WebFrontend.Client.Services.ICommissionService, CRM.WebFrontend.Client.Services.CommissionService>();
builder.Services.AddScoped<CRM.WebFrontend.Client.Services.IActivationService, CRM.WebFrontend.Client.Services.ActivationService>();
builder.Services.AddScoped<CRM.WebFrontend.Client.Services.IMaintenanceService, CRM.WebFrontend.Client.Services.MaintenanceService>();

await builder.Build().RunAsync();
