using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly MeUseCase _meUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly CRM.ApiHub.Application.Interfaces.IJwtTokenGenerator _tokenGenerator;
    private readonly CRM.ApiHub.Application.Interfaces.IRefreshTokenStore _refreshTokenStore;

    public AuthController(
        LoginUseCase loginUseCase,
        MeUseCase meUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        CRM.ApiHub.Application.Interfaces.IJwtTokenGenerator tokenGenerator,
        CRM.ApiHub.Application.Interfaces.IRefreshTokenStore refreshTokenStore)
    {
        _loginUseCase = loginUseCase;
        _meUseCase = meUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    [HttpPost("login")]
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
        // El claim de ID de usuario puede venir mapeado como NameIdentifier o directamente como "sub"
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
        return Ok();
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
}
