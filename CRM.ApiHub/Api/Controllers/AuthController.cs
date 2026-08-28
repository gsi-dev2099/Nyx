using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Application.UseCases.Auth;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly MeUseCase _meUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        LoginUseCase loginUseCase,
        MeUseCase meUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        ILogger<AuthController> logger)
    {
        _loginUseCase = loginUseCase;
        _meUseCase = meUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginLimit")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _loginUseCase.ExecuteAsync(request, GetClientIpAddress(), GetUserAgent());

        if (response == null)
        {
            return Unauthorized(new { message = "Nombre de usuario o contraseña incorrectos." });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
        {
            return Unauthorized(new { message = "Usuario no autorizado o token inválido." });
        }

        var userDetail = await _meUseCase.ExecuteAsync(userId);
        if (userDetail == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(new
        {
            nombre = userDetail.Username,
            rol = userDetail.RoleName,
            campanaAsignada = userDetail.CampaignName
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _refreshTokenUseCase.ExecuteAsync(request, GetClientIpAddress(), GetUserAgent());
        if (response == null)
        {
            return Unauthorized(new { message = "Token de refresco inválido o expirado." });
        }
        return Ok(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest request)
    {
        if (request != null && !string.IsNullOrEmpty(request.RefreshToken))
        {
            _refreshTokenStore.RevokeToken(request.RefreshToken);
        }
        return Ok(new { message = "Sesión cerrada exitosamente." });
    }

    private string GetClientIpAddress()
    {
        string ipAddress = Request.Headers["X-Forwarded-For"].ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        }
        return ipAddress;
    }

    private string GetUserAgent()
    {
        return Request.Headers["User-Agent"].ToString();
    }

    [Authorize]
    [HttpGet("check-permission")]
    public async Task<IActionResult> CheckPermission(
        [FromQuery] string permissionKey, 
        [FromQuery] long statusId, 
        [FromServices] IPermissionService permissionService)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        bool hasPermission = await permissionService.CanUserActionAsync(userId, permissionKey, (int)statusId);
        return Ok(new { hasPermission });
    }
}
