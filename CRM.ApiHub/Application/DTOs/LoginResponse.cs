namespace CRM.ApiHub.Application.DTOs;

public record LoginResponse(string Token, string RefreshToken, string Username, string? Role);
