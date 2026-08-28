using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Infrastructure.Persistence;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cacheService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IDbConnectionFactory connectionFactory,
        IFileStorageService fileStorageService,
        ICacheService cacheService,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<HealthController> logger)
    {
        _connectionFactory = connectionFactory;
        _fileStorageService = fileStorageService;
        _cacheService = cacheService;
        _httpClient = httpClientFactory.CreateClient();
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        bool dbOk = false;
        string dbMsg = "Pending";
        bool fdwOk = false;
        string fdwMsg = "Pending";

        // 1. Check PostgreSQL nyx_crm
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var dbCheck = await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1;", cancellationToken: ct));
            dbOk = dbCheck == 1;
            dbMsg = dbOk ? "Healthy" : "Failed to return scalar 1";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthCheck: Falla al conectar a la base de datos principal.");
            dbMsg = ex.Message;
        }

        // 2. Check FDW to nx_ecosystem
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1 FROM ext_ecosystem.collaborators LIMIT 1;", cancellationToken: ct));
            fdwOk = true;
            fdwMsg = "Healthy";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HealthCheck: Falla al verificar conexión al FDW ext_ecosystem.");
            fdwMsg = ex.Message;
        }

        // 3. Check MinIO Object Storage
        bool minioOk = false;
        string minioMsg = "Pending";
        try
        {
            var bucket = _config["MinioSettings:BucketName"] ?? "nyx-crm-documents";
            await _fileStorageService.EnsureBucketExistsAsync(bucket, ct);
            minioOk = true;
            minioMsg = "Healthy";
        }
        catch (Exception ex)
        {
            minioMsg = ex.Message;
        }

        // 4. Check Redis Cache
        bool redisOk = false;
        string redisMsg = "Pending";
        try
        {
            var testKey = "health:ping";
            await _cacheService.SetAsync(testKey, "pong", TimeSpan.FromSeconds(10), ct);
            var val = await _cacheService.GetAsync<string>(testKey, ct);
            redisOk = val == "pong";
            redisMsg = redisOk ? "Healthy" : "Cache write/read mismatch";
        }
        catch (Exception ex)
        {
            redisMsg = ex.Message;
        }

        // 5. Check Satellite Engines
        var slaUrl = _config["SlaEngineSettings:BaseUrl"] ?? "http://sla_engine_api:5070";
        var flowUrl = _config["FlowEngineSettings:BaseUrl"] ?? "http://flow_engine_api:5072";
        var approvalUrl = _config["ApprovalEngineSettings:BaseUrl"] ?? "http://approval_engine_api:5071";

        var slaHealthy = await CheckEngineHealthAsync($"{slaUrl}/health");
        var flowHealthy = await CheckEngineHealthAsync($"{flowUrl}/health");
        var approvalHealthy = await CheckEngineHealthAsync($"{approvalUrl}/health");

        sw.Stop();

        var overallHealthy = dbOk && minioOk && redisOk;
        var status = overallHealthy 
            ? (fdwOk && slaHealthy && flowHealthy && approvalHealthy ? "Healthy" : "Degraded")
            : "Unhealthy";

        var result = new
        {
            status,
            durationMs = sw.ElapsedMilliseconds,
            timestamp = DateTime.UtcNow,
            checks = new
            {
                database = new { status = dbOk ? "Healthy" : "Unhealthy", message = dbMsg },
                fdwEcosystem = new { status = fdwOk ? "Healthy" : "Degraded", message = fdwMsg },
                minioStorage = new { status = minioOk ? "Healthy" : "Unhealthy", message = minioMsg },
                redisCache = new { status = redisOk ? "Healthy" : "Degraded", message = redisMsg },
                engines = new
                {
                    slaEngine = new { url = slaUrl, isHealthy = slaHealthy },
                    flowEngine = new { url = flowUrl, isHealthy = flowHealthy },
                    approvalEngine = new { url = approvalUrl, isHealthy = approvalHealthy }
                }
            }
        };

        if (status == "Healthy") return Ok(result);
        if (status == "Degraded") return StatusCode(200, result);
        return StatusCode(503, result);
    }

    private async Task<bool> CheckEngineHealthAsync(string healthUrl)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var resp = await _httpClient.GetAsync(healthUrl, cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
