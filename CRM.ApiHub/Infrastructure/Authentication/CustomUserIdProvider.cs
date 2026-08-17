using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Extraer id_user del token JWT para asociarlo con SignalR
        return connection.User?.FindFirst("id_user")?.Value 
               ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? connection.User?.FindFirst("sub")?.Value;
    }
}
