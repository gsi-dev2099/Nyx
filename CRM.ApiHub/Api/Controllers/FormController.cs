using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces; 
using System.Security.Claims;

namespace CRM.ApiHub.Api.Controllers;

public record UpdateFormStatusRequest(string Status, long ValidatedBy);

[ApiController]
[Route("api/forms")]
[Authorize] 
public class FormController : ControllerBase
{
    private readonly IFormRepository _formRepository;
    private readonly IOrderDataRepository _orderDataRepository;
    private readonly INotificationService _notificationService; 
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IPermissionService _permissionService;

    public FormController(
        IFormRepository formRepository, 
        IOrderDataRepository orderDataRepository, 
        INotificationService notificationService,
        ISalesOrderRepository salesOrderRepository,
        IPermissionService permissionService) 
    {
        _formRepository = formRepository;
        _orderDataRepository = orderDataRepository;
        _notificationService = notificationService; 
        _salesOrderRepository = salesOrderRepository;
        _permissionService = permissionService;
    }

    [HttpGet("campaign/{idCmpg}/stage/{idStage}")]
    public async Task<IActionResult> GetTemplates(long idCmpg, long idStage)
    {
        var templates = await _formRepository.GetTemplatesByCampaignStageAsync(idCmpg, idStage);
        return Ok(templates);
    }

    [HttpGet("campaign/{idCmpg}/stage/{idStage}/fields")]
    public async Task<IActionResult> GetFieldsByCampaignStage(long idCmpg, long idStage)
    {
        var templates = await _formRepository.GetTemplatesByCampaignStageAsync(idCmpg, idStage);
        var template = templates.FirstOrDefault();
        if (template == null)
        {
            return Ok(new List<FormField>());
        }
        var fields = await _formRepository.GetFieldsByTemplateAsync(template.IdForm);
        return Ok(fields);
    }

    [HttpGet("options-catalog")]
    public async Task<IActionResult> GetOptionsCatalog([FromServices] CRM.ApiHub.Infrastructure.Persistence.IDbConnectionFactory connectionFactory)
    {
        using var conn = connectionFactory.CreateConnection();
        const string sql = "SELECT category, option_key AS \"Value\", label AS \"Label\", price_delta AS \"Price\" FROM sales_service.sales_form_option_catalog WHERE is_active = true ORDER BY category, order_index;";
        var options = await Dapper.SqlMapper.QueryAsync(conn, sql);
        return Ok(options);
    }

    [HttpGet("{idForm}/fields")]
    public async Task<IActionResult> GetFields(long idForm)
    {
        var fields = await _formRepository.GetFieldsByTemplateAsync(idForm);
        return Ok(fields);
    }

    [HttpGet("order/{idOrder}/data")]
    public async Task<IActionResult> GetOrderData(long idOrder)
    {
        var data = await _orderDataRepository.GetByOrderAsync(idOrder);
        return Ok(data);
    }

    [HttpPost("order/{idOrder}/template/{idForm}")]
    public async Task<IActionResult> SaveData(long idOrder, long idForm, [FromBody] IEnumerable<OrderData> fields)
    {
        if (fields == null || !fields.Any())
            return BadRequest("La lista de campos no puede estar vacía.");

        // 1. Obtener la orden
        var order = await _salesOrderRepository.GetByIdAsync(idOrder);
        if (order == null)
            return NotFound(new { message = "La orden no existe." });

        // 2. Obtener el ID del usuario logueado
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            return Unauthorized(new { message = "Usuario no autorizado." });

        // 3. Validar custodia (El asesor creador o custodio debe tener la custodia)
        if (order.CustodyUserId.HasValue && order.CustodyUserId.Value != 0 && order.CustodyUserId.Value != userId && order.IdUser != userId)
            return StatusCode(403, new { message = "No tienes la custodia de esta orden para editar sus campos." });

        // 4. Validar si el estado permite la edición
        // Si es el guardado inicial de los datos de la orden recién creada, se permite guardar.
        var existingData = await _orderDataRepository.GetByOrderAsync(idOrder);
        bool isInitialSave = (existingData == null || !existingData.Any());

        if (!isInitialSave)
        {
            var statusId = (int)(order.IdStatus ?? 0);
            var hasPermission = await _permissionService.CanUserActionAsync((int)userId, "sales.order.edit.field", statusId);
            if (!hasPermission)
                return StatusCode(403, new { message = "El estado actual del pedido no permite la edición de campos." });
        }

        await _formRepository.SaveOrderDataAsync(idOrder, idForm, fields);
        return Ok(new { message = "Datos del formulario guardados exitosamente." });
    }

    [HttpPut("data/{idData}/status")]
    public async Task<IActionResult> UpdateStatus(long idData, [FromBody] UpdateFormStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest("El estado es requerido.");

        await _orderDataRepository.UpdateFieldStatusAsync(idData, request.Status, request.ValidatedBy);

        await _notificationService.SendNotificationAsync(
            userId: request.ValidatedBy, 
            title: "Cambio de Estado",
            message: $"El registro {idData} ha cambiado al estado: {request.Status}.",
            module: "Ventas"
        );

        return Ok(new { message = "Estado actualizado correctamente." });
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDefaultForms()
    {
        await _formRepository.SeedDefaultFormsAsync();
        return Ok(new { message = "Formularios de prueba generados exitosamente para las campañas y etapas." });
    }
}