using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SpoMusic.Web.Models;

namespace SpoMusic.Web.Services;

public class SpoMusicApiClient : ISpoMusicApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _publicUrl;
    private readonly ILogger<SpoMusicApiClient> _logger;

    public SpoMusicApiClient(HttpClient http, IConfiguration config, ILogger<SpoMusicApiClient> logger)
    {
        _http = http;
        _baseUrl = config["SpoMusicApi:BaseUrl"] ?? "http://localhost:4000";
        _publicUrl = config["SpoMusicApi:PublicUrl"] ?? "http://localhost:4000";
        _logger = logger;
    }

    public string GetSpotifyAuthUrl(string? platform = null, string? returnUrl = null, string? publicApiUrl = null)
    {
        var targetPublic = !string.IsNullOrWhiteSpace(publicApiUrl) ? publicApiUrl : _publicUrl;
        var url = $"{targetPublic.TrimEnd('/')}/auth/spotify";
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(platform))
        {
            queryParams.Add($"platform={Uri.EscapeDataString(platform)}");
        }
        if (!string.IsNullOrEmpty(returnUrl))
        {
            queryParams.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        return url;
    }

    public async Task<(string? Token, UserProfileViewModel? User)> ExchangeCodeAsync(string code)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(new { code }), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync($"{_baseUrl}/auth/exchange", content);

            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("ExchangeCode failed [{Status}]: {Body}", res.StatusCode, errBody);
                return (null, null);
            }

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            var userEl = doc.RootElement.GetProperty("user");
            var user = JsonSerializer.Deserialize<UserProfileViewModel>(userEl.GetRawText());

            return (token, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExchangeCode failed");
            return (null, null);
        }
    }

    public async Task<UserProfileViewModel?> GetCurrentUserAsync(string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/auth/me");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.SendAsync(req);

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileViewModel>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrentUser failed");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/revoke");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RevokeToken failed");
            return false;
        }
    }

    public async Task<List<MatchItemViewModel>> GetMatchesAsync(string? token = null, string? userId = null)
    {
        try
        {
            var url = $"{_baseUrl}/matches";
            if (!string.IsNullOrEmpty(userId))
            {
                url += $"?userId={Uri.EscapeDataString(userId)}";
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<MatchItemViewModel>();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MatchItemViewModel>>(json) ?? new List<MatchItemViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMatches failed");
            return new List<MatchItemViewModel>();
        }
    }

    public async Task<List<TopTrackViewModel>> GetTopTracksAsync(string? token = null, string? userId = null)
    {
        try
        {
            var url = $"{_baseUrl}/spotify/top";
            if (!string.IsNullOrEmpty(userId))
            {
                url += $"?userId={Uri.EscapeDataString(userId)}";
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<TopTrackViewModel>();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TopTrackViewModel>>(json) ?? new List<TopTrackViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTopTracks failed");
            return new List<TopTrackViewModel>();
        }
    }

    public async Task<List<HistoryItemViewModel>> GetHistoryAsync(string? token = null, string? userId = null)
    {
        try
        {
            var url = $"{_baseUrl}/spotify/history";
            if (!string.IsNullOrEmpty(userId))
            {
                url += $"?userId={Uri.EscapeDataString(userId)}";
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<HistoryItemViewModel>();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<HistoryItemViewModel>>(json) ?? new List<HistoryItemViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistory failed");
            return new List<HistoryItemViewModel>();
        }
    }

    public async Task<bool> SyncUserDataAsync(string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/spotify/sync");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SyncUserData failed");
            return false;
        }
    }

    public async Task<string?> CreateBlendAsync(string targetUserId, string token)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/spotify/blend");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonSerializer.Serialize(new { targetUserId }), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Blend çalma listesi oluşturuldu!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateBlend failed");
            return null;
        }
    }
}
