using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CRM.ApiHub.Domain.Repositories;
using System.Reflection;

namespace CRM.ApiHub.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;

    public RequiresPermissionAttribute(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Extraer el identificador de usuario desde el JWT (sub, id_user, NameIdentifier, etc.)
        var userClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? context.HttpContext.User.FindFirst("sub")
                        ?? context.HttpContext.User.FindFirst("id_user")
                        ?? context.HttpContext.User.FindFirst("userId");

        if (userClaim == null || !int.TryParse(userClaim.Value, out int userId))
        {
            context.Result = new ObjectResult(new { message = "Usuario no autenticado de forma válida." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        // 2. Extraer el statusId desde los parámetros de la solicitud (Ruta, Query String o JSON body)
        int statusId = 0;
        var routeStatus = context.RouteData.Values["statusId"]?.ToString();
        var queryStatus = context.HttpContext.Request.Query["statusId"].ToString();

        if (!string.IsNullOrEmpty(routeStatus) && int.TryParse(routeStatus, out int parsedRoute))
        {
            statusId = parsedRoute;
        }
        else if (!string.IsNullOrEmpty(queryStatus) && int.TryParse(queryStatus, out int parsedQuery))
        {
            statusId = parsedQuery;
        }
        else if (context.HttpContext.Request.HasJsonContentType())
        {
            try
            {
                context.HttpContext.Request.EnableBuffering();
                // Read body synchronously or asynchronously
                using (var reader = new StreamReader(context.HttpContext.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    // Using Task.Run to do async read synchronously since OnAuthorizationAsync is async
                    var body = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    context.HttpContext.Request.Body.Position = 0; // Rewind for model binder

                    if (!string.IsNullOrEmpty(body))
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("statusId", out var statusProp) && statusProp.TryGetInt32(out int parsedVal))
                        {
                            statusId = parsedVal;
                        }
                    }
                }
            }
            catch {}
        }

        // 3. Validar contra la función de PostgreSQL
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        bool hasPermission = await permissionService.CanUserActionAsync(userId, _permissionKey, statusId);

        // 4. Cortar flujo si no se cuenta con los privilegios del estado de custodia
        if (!hasPermission)
        {
            context.Result = new ObjectResult(new { message = $"Acceso denegado: No cuentas con la custodia requerida para '{_permissionKey}' (Estado: {statusId})." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}