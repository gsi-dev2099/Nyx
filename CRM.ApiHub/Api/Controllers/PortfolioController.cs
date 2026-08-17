using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Infrastructure.Persistence;
using Dapper;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(IDbConnectionFactory connectionFactory, ILogger<PortfolioController> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPortfolios()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT id_ptflo as Id, name as Name FROM portfolio_service.portfolio ORDER BY name;";
            var portfolios = await connection.QueryAsync(sql);
            return Ok(portfolios);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error querying portfolio_service.portfolio, returning defaults.");
            // Fallback list of portfolios
            var fallback = new[]
            {
                new { Id = 1, Name = "ALARMAS" },
                new { Id = 2, Name = "MONITOREO" },
                new { Id = 3, Name = "TECNOLOGÍA" },
                new { Id = 4, Name = "MANTENIMIENTO" },
                new { Id = 5, Name = "POSTVENTA" }
            };
            return Ok(fallback);
        }
    }
}
