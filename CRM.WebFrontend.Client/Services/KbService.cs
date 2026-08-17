using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

public class KbService : IKbService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public KbService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _authStateProvider = authStateProvider;
    }

    private async Task EnsureTokenAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<KbArticleResponseDto>> SearchAsync(string? query, string? contentType)
    {
        await EnsureTokenAsync();
        var q = new List<string>();
        if (!string.IsNullOrWhiteSpace(query)) q.Add($"query={System.Uri.EscapeDataString(query)}");
        if (!string.IsNullOrWhiteSpace(contentType)) q.Add($"contentType={System.Uri.EscapeDataString(contentType)}");

        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        var response = await _httpClient.GetAsync($"/api/kb/search{qs}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<KbArticleResponseDto>>() ?? new();
        }
        var errorStr = await response.Content.ReadAsStringAsync();
        throw new System.Exception($"API Error {(int)response.StatusCode}: {errorStr}");
    }

    public async Task<KbArticleResponseDto?> GetByIdAsync(long idArticle)
    {
        await EnsureTokenAsync();
        var response = await _httpClient.GetAsync($"/api/kb/{idArticle}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<KbArticleResponseDto>();
        }
        return null;
    }

    public async Task<bool> SubmitFeedbackAsync(long idArticle, bool isHelpful, string? comment)
    {
        await EnsureTokenAsync();
        var dto = new SubmitFeedbackRequestDto { IsHelpful = isHelpful, Comment = comment };
        var response = await _httpClient.PostAsJsonAsync($"/api/kb/{idArticle}/feedback", dto);
        return response.IsSuccessStatusCode;
    }
}
