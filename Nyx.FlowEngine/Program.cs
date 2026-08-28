using Dapper;
using Nyx.FlowEngine.Application;
using Nyx.FlowEngine.Infrastructure;
using Microsoft.OpenApi.Models;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nyx Cycle & Checkpoint Engine API",
        Version = "v1",
        Description = "Motor Autónomo de Ciclos, Etapas, Checkpoints y Políticas de Actuación"
    });
});

builder.Services.AddScoped<ICycleRepository, CycleRepository>();
builder.Services.AddScoped<ICycleService, CycleService>();
builder.Services.AddHostedService<ScheduledCheckpointWorker>();

var app = builder.Build();

// Auto-sincronización de secuencias PostgreSQL al iniciar
try
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=crm_postgres;Port=5432;Database=nyx_flow;Username=usr_flow;Password=Flow$$Nyx2026!Engine#Key";
    using var conn = new Npgsql.NpgsqlConnection(connStr);
    conn.Open();
    const string syncSql = @"
        DO $$
        BEGIN
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'nyx_flow' AND table_name = 'cycle_definition') THEN
                PERFORM setval(pg_get_serial_sequence('nyx_flow.cycle_definition', 'id_cycle'), COALESCE((SELECT MAX(id_cycle) FROM nyx_flow.cycle_definition), 0) + 1, false);
            END IF;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'nyx_flow' AND table_name = 'cycle_stage') THEN
                PERFORM setval(pg_get_serial_sequence('nyx_flow.cycle_stage', 'id_stage'), COALESCE((SELECT MAX(id_stage) FROM nyx_flow.cycle_stage), 0) + 1, false);
            END IF;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'nyx_flow' AND table_name = 'checkpoint_catalog') THEN
                PERFORM setval(pg_get_serial_sequence('nyx_flow.checkpoint_catalog', 'id_checkpoint'), COALESCE((SELECT MAX(id_checkpoint) FROM nyx_flow.checkpoint_catalog), 0) + 1, false);
            END IF;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'nyx_flow' AND table_name = 'cycle_instance') THEN
                PERFORM setval(pg_get_serial_sequence('nyx_flow.cycle_instance', 'id_instance'), COALESCE((SELECT MAX(id_instance) FROM nyx_flow.cycle_instance), 0) + 1, false);
            END IF;
        END $$;
    ";
    conn.Execute(syncSql);
    Console.WriteLine("[INFO] PostgreSQL sequences synced successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] Sequence sync on startup: {ex.Message}");
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nyx Cycle Engine API v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Nyx Cycle & Checkpoint Engine", 
    status = "Healthy", 
    database = "nyx_flow",
    timestamp = DateTime.UtcNow,
    hierarchy = "CICLOS -> ETAPAS -> CHECKPOINTS -> CANVAS",
    capabilities = new[] { "Cycles Hierarchy", "Dynamic Policy Engine", "Handshake Security", "Scheduled Checkpoint Worker", "SHA-512 Audit" },
    version = "2.0.0"
}));

app.MapControllers();

app.Run();
