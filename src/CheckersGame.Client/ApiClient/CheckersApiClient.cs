using CheckersGame.Shared.Dtos;
using System.Net.Http.Json;

namespace CheckersGame.Client.ApiClient;

/// <summary>
/// Thin wrapper around HttpClient for all API calls.
/// </summary>
public class CheckersApiClient
{
    private readonly HttpClient _http;

    public CheckersApiClient(string baseUrl)
    {
        // Accept the self-signed dev certificate used by ASP.NET Core in development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<PlayerDto?> GetPlayerAsync(int id)
    {
        // GetFromJsonAsync throws on 404, so we check the status code manually.
        var resp = await _http.GetAsync($"api/players/{id}");
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;   // player not registered — GameSetupForm shows a friendly message
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PlayerDto>();
    }

    public async Task<StartGameResponse?> StartGameAsync(StartGameRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/games", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<StartGameResponse>();
    }

    public async Task<MakeMoveResponse?> MakeMoveAsync(int gameId, MakeMoveRequest req)
    {
        var resp = await _http.PostAsJsonAsync($"api/games/{gameId}/moves", req);
        return await resp.Content.ReadFromJsonAsync<MakeMoveResponse>();
    }

    public async Task<MakeMoveResponse?> TimeoutAsync(int gameId, int playerId)
    {
        var resp = await _http.PostAsJsonAsync(
            $"api/games/{gameId}/timeout", new TimeoutRequest { PlayerId = playerId });
        return await resp.Content.ReadFromJsonAsync<MakeMoveResponse>();
    }
}
