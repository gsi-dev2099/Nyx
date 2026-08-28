using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        IUserRepository userRepository, 
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        ILogger<LoginUseCase> logger)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _logger = logger;
    }

    public async Task<LoginResponse?> ExecuteAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        // 1. Obtener el usuario por username
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (user == null)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' no existe.", request.Username);
            return null;
        }

        // 2. Verificar la contraseña usando exclusivamente BCrypt seguro
        bool isPasswordValid = false;
        try
        {
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (user.PasswordHash.StartsWith("$2"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar hash BCrypt para el usuario '{Username}'.", request.Username);
        }

        if (!isPasswordValid)
        {
            _logger.LogWarning("Intento de login fallido: credenciales incorrectas para el usuario '{Username}'.", request.Username);
            return null;
        }

        // 3. Obtener el rol y detalles del usuario
        var userDetail = await _userRepository.GetUserDetailByIdAsync(user.IdUser, ct);
        var role = userDetail?.RoleName ?? "ASESOR";

        // 4. Generar token JWT firmado
        var token = _tokenGenerator.GenerateToken(user, role);

        // 5. Generar Refresh Token seguro
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiry = DateTime.UtcNow.AddDays(7);
        _refreshTokenStore.SaveToken(refreshToken, user.IdUser, expiry, ipAddress, userAgent);

        _logger.LogInformation("Login exitoso para usuario '{Username}' con rol '{Role}'.", user.Username, role);
        return new LoginResponse(token, refreshToken, user.Username, role);
    }
}
