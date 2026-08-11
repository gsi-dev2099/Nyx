using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace CRM.ApiHub.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IConfiguration _configuration;

    public LoginUseCase(
        IUserRepository userRepository, 
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> ExecuteAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken ct = default)
    {

        // 1. Obtener el usuario por username
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (user == null)
        {
            return null;
        }

        // 2. Verificar la contraseña usando BCrypt (con soporte para hash plano y fallbacks de desarrollo)
        bool isPasswordValid = false;
        try
        {
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (user.PasswordHash.StartsWith("$2"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
                else if (user.PasswordHash == request.Password)
                {
                    isPasswordValid = true;
                }
            }
        }
        catch { }

        if (!isPasswordValid)
        {
            var allowDevFallback = _configuration.GetValue<bool>("AuthSettings:AllowDevFallbackPassword");
            if (allowDevFallback)
            {
                if (request.Password == "password123" || request.Password == "123456" || request.Password == "1221" || request.Password == "72869754" || request.Password == "dayanyy2010")
                {
                    isPasswordValid = true;
                }
            }
            
            if (!isPasswordValid)
            {
                return null;
            }
        }

        // 3. Obtener el rol del usuario
        var userDetail = await _userRepository.GetUserDetailByIdAsync(user.IdUser, ct);
        var role = userDetail?.RoleName;

        // 4. Generar token JWT válido
        var token = _tokenGenerator.GenerateToken(user, role);

        // 5. Generar Refresh Token
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiry = DateTime.UtcNow.AddDays(7);
        _refreshTokenStore.SaveToken(refreshToken, user.IdUser, expiry, ipAddress, userAgent);

        return new LoginResponse(token, refreshToken, user.Username, role);
    }
}
