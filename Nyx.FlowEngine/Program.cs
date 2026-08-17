using Nyx.FlowEngine.Application;
using Nyx.FlowEngine.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IFlowService, FlowService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Nyx Flow Engine", 
    status = "Healthy", 
    database = "nyx_flow",
    timestamp = DateTime.UtcNow,
    capabilities = new[] { "Layered Checkpoints", "Triple Approval Governance", "Recurrence & Rollback", "SHA-512 Audit Log" },
    version = "1.0.0"
}));

app.MapGet("/api/flow/info", () => Results.Ok(new
{
    engine = "Nyx Stage & Checkpoint Lifecycle Engine",
    owner = "NxFortress Corporación",
    capabilities = new[] { "Dynamic Pipelines", "Internal Checkpoint Steps", "Stage Blockers & Exceptions" }
}));

app.MapControllers();

app.Run();
