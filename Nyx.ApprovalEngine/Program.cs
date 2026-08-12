using Nyx.ApprovalEngine.Application;
using Nyx.ApprovalEngine.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Nyx Approval Engine", 
    status = "Healthy", 
    database = "nyx_approval",
    timestamp = DateTime.UtcNow,
    compliance = new[] { "ISO 9001:2015 Document Control", "ISO 27001:2022 Segregation of Duties", "SOX Auditable" },
    version = "1.0.0"
}));

app.MapGet("/api/approval/info", () => Results.Ok(new
{
    engine = "Nyx ISO-Compliant Approval Engine",
    owner = "NxFortress Corporación",
    capabilities = new[] { "Multi-Step Chains", "Conditional Routing", "Temporary Delegations", "SHA-512 Audit Log" }
}));

app.MapControllers();

app.Run();
