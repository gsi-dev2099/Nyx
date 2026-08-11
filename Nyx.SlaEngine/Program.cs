var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "NxFortress SLA Engine", 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.MapGet("/api/sla/info", () => Results.Ok(new
{
    engine = "NxFortress SLA Autonomous Engine",
    owner = "NxFortress Corporación",
    capabilities = new[] { "Realtime SLA Clock", "Dynamic Work Shifts", "PL/pgSQL Atomic Precision", "JSONB Multidepartmental Context" }
}));

app.MapControllers();

app.Run();
