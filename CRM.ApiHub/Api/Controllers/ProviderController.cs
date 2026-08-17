using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class ProviderController : ControllerBase
{
    private readonly GetProviderCatalogUseCase _getProviderCatalogUseCase;
    private readonly GetProviderStatusMappingUseCase _getProviderStatusMappingUseCase;
    private readonly LogProviderSyncUseCase _logProviderSyncUseCase;
    private readonly UpdateOrderProviderStatusUseCase _updateOrderProviderStatusUseCase;

    public ProviderController(
        GetProviderCatalogUseCase getProviderCatalogUseCase,
        GetProviderStatusMappingUseCase getProviderStatusMappingUseCase,
        LogProviderSyncUseCase logProviderSyncUseCase,
        UpdateOrderProviderStatusUseCase updateOrderProviderStatusUseCase)
    {
        _getProviderCatalogUseCase = getProviderCatalogUseCase;
        _getProviderStatusMappingUseCase = getProviderStatusMappingUseCase;
        _logProviderSyncUseCase = logProviderSyncUseCase;
        _updateOrderProviderStatusUseCase = updateOrderProviderStatusUseCase;
    }

    [HttpGet("api/providers")]
    public async Task<IActionResult> GetCatalog(CancellationToken ct)
    {
        try
        {
            var catalog = await _getProviderCatalogUseCase.ExecuteAsync(ct);
            return Ok(catalog);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el catálogo de proveedores.", details = ex.Message });
        }
    }

    [HttpGet("api/providers/{id:long}/status-mapping")]
    public async Task<IActionResult> GetStatusMapping(long id, CancellationToken ct)
    {
        try
        {
            var mapping = await _getProviderStatusMappingUseCase.ExecuteAsync(id, ct);
            return Ok(mapping);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el mapeo de estados del proveedor.", details = ex.Message });
        }
    }

    [HttpPost("api/providers/{id:long}/sync-log")]
    public async Task<IActionResult> LogSync(long id, [FromBody] ProviderSyncLogRequestDto dto, CancellationToken ct)
    {
        try
        {
            var success = await _logProviderSyncUseCase.ExecuteAsync(id, dto.IdOrder, dto.StatusCode, dto.Result, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudo registrar la bitácora de sincronización." });
            }

            return Ok(new { message = "Registro de sincronización guardado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al guardar el registro de sincronización.", details = ex.Message });
        }
    }

    [HttpPost("api/providers/{id:long}/update-order-status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderProviderStatusRequestDto dto, CancellationToken ct)
    {
        try
        {
            var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
            long actorUserId = 1; // System default fallback
            if (actorIdClaim != null && long.TryParse(actorIdClaim.Value, out long parsedId))
            {
                actorUserId = parsedId;
            }

            var response = await _updateOrderProviderStatusUseCase.ExecuteAsync(id, dto, actorUserId, ct);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al procesar la homologación del estado de la orden.", details = ex.Message });
        }
    }
}
