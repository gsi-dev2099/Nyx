using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

/// <summary>
/// Intercepts HTTP calls from the Blazor WASM client that target endpoints
/// not yet exposed by the backend (ApiHub). Returns mock data for those
/// specific endpoints while letting all other calls pass through to the real backend.
/// 
/// This handler is ONLY active on the WASM side. Server-side components (InteractiveServer)
/// do NOT go through this handler.
/// </summary>
public class MockBackendHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Todos los endpoints ahora existen en el ApiHub real, así que dejamos 
        // pasar todas las peticiones hacia el backend vía YARP.
        return await base.SendAsync(request, cancellationToken);
    }

    private HttpResponseMessage CreateJsonResponse<T>(T data)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(data)
        };
        return response;
    }
}
