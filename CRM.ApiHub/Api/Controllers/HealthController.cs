using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Infrastructure.Persistence;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IDbConnectionFactory connectionFactory, ILogger<HealthController> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var result = new
        {
            Status = "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = new
            {
                Database = new { Status = "Pending", Message = "" },
                FdwEcosystem = new { Status = "Pending", Message = "" }
            }
        };

        bool dbOk = false;
        bool fdwOk = false;

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Check 1: Base de datos principal (nyx_crm)
            try
            {
                var dbCheck = await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1;", cancellationToken: ct));
                dbOk = dbCheck == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HealthCheck: Falla al conectar a la base de datos principal.");
                result = result with { Checks = result.Checks with { Database = new { Status = "Unhealthy", Message = ex.Message } } };
            }

            // Check 2: Foreign Data Wrapper (FDW) a ecosystem
            try
            {
                var fdwCheck = await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1 FROM ext_ecosystem.collaborators LIMIT 1;", cancellationToken: ct));
                fdwOk = true; // Si la query no lanza excepción, el link funciona, aunque devuelva null (sin registros).
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HealthCheck: Falla al verificar conexión al FDW ext_ecosystem.");
                result = result with { Checks = result.Checks with { FdwEcosystem = new { Status = "Unhealthy", Message = ex.Message } } };
            }

            if (dbOk)
            {
                result = result with { Checks = result.Checks with { Database = new { Status = "Healthy", Message = "OK" } } };
            }
            if (fdwOk)
            {
                result = result with { Checks = result.Checks with { FdwEcosystem = new { Status = "Healthy", Message = "OK" } } };
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "HealthCheck: Error fatal al abrir conexión.");
        }

        if (dbOk && fdwOk)
        {
            result = result with { Status = "Healthy" };
            return Ok(result);
        }
        else if (dbOk && !fdwOk)
        {
            // FDW caído pero BD principal viva. 
            // Podría ser un error 503 o 200 con estado "Degraded". Usaremos 503 para que balanceadores sepan que hay problemas.
            result = result with { Status = "Degraded" };
            return StatusCode(503, result);
        }
        else
        {
            // BD principal caída.
            result = result with { Status = "Unhealthy" };
            return StatusCode(503, result);
        }
    }
}
