using Nyx.SlaEngine.Application;
using Nyx.SlaEngine.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ISlaRepository, SlaRepository>();
builder.Services.AddScoped<ISlaService, SlaService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Nyx SLA Engine", 
    status = "Healthy", 
    database = "nyx_sla",
    timestamp = DateTime.UtcNow,
    version = "2.0.0"
}));

app.MapGet("/api/sla/info", () => Results.Ok(new
{
    engine = "Nyx SLA Autonomous Engine v2",
    owner = "NxFortress Corporación",
    capabilities = new[] { "Isolated PostgreSQL DB (nyx_sla)", "Realtime SLA Clock", "Multi-Scope Policies", "Immutable Audit Log" }
}));

app.MapControllers();

app.Run();
