using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpoMusic.Api.Data;
using SpoMusic.Api.DTOs;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly ILogger<AuthService> _logger;

    private record StateEntry(string? ReturnUrl, string? RedirectUri, DateTime ExpiresAt);
    private static readonly ConcurrentDictionary<string, (string Token, UserProfileDto User, DateTime ExpiresAt)> _authCodes = new();
    private static readonly ConcurrentDictionary<string, StateEntry> _stateStore = new();

    public AuthService(AppDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
    {
        _db = db;
        _config = config;
        _http = httpClientFactory.CreateClient("SpotifyAuth");
        _logger = logger;
    }

    public string GetSpotifyAuthUrl(string? platform = null, string? returnUrl = null, string? redirectUri = null)
    {
        var clientId = _config["Spotify:ClientId"] ?? "";
        var effectiveRedirectUri = redirectUri ?? _config["Spotify:RedirectUri"] ?? "http://localhost:4000/auth/spotify/callback";
        var scopes = Uri.EscapeDataString("user-read-email user-read-private user-top-read user-read-recently-played");

        var rawState = Guid.NewGuid().ToString("N");
        var state = string.Equals(platform, "mobile", StringComparison.OrdinalIgnoreCase)
            ? $"mobile_{rawState}"
            : rawState;

        _stateStore[state] = new StateEntry(returnUrl, effectiveRedirectUri, DateTime.UtcNow.AddMinutes(10));

        return $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&scope={scopes}&redirect_uri={Uri.EscapeDataString(effectiveRedirectUri)}&state={state}&show_dialog=true";
    }

    public StateValidationResult ValidateState(string state)
    {
        if (_stateStore.TryRemove(state, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return new StateValidationResult(true, entry.ReturnUrl, entry.RedirectUri);
            }
        }
        return new StateValidationResult(false, null, null);
    }

    public string CreateOneTimeCode(string token, UserProfileDto user)
    {
        var code = Guid.NewGuid().ToString("N");
        _authCodes[code] = (token, user, DateTime.UtcNow.AddSeconds(60));
        return code;
    }

    public AuthResponse ExchangeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Authorization code is required");
        }

        if (!_authCodes.TryRemove(code, out var entry) || entry.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired authorization code");
        }

        return new AuthResponse(entry.Token, entry.User);
    }

    public async Task<AuthResponse> HandleSpotifyCallbackAsync(string code, string? redirectUri = null)
    {
        var clientId = _config["Spotify:ClientId"] ?? "";
        var clientSecret = _config["Spotify:ClientSecret"] ?? "";
        var effectiveRedirectUri = redirectUri ?? _config["Spotify:RedirectUri"] ?? "http://localhost:4000/auth/spotify/callback";

        // 1. Exchange code for access token
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = effectiveRedirectUri
        });

        var tokenRes = await _http.SendAsync(tokenRequest);
        var tokenBody = await tokenRes.Content.ReadAsStringAsync();

        if (!tokenRes.IsSuccessStatusCode)
        {
            _logger.LogError("Spotify token exchange failed [{Status}]: {Body}", tokenRes.StatusCode, tokenBody);
            throw new UnauthorizedAccessException($"Spotify token exchange failed: {tokenBody}");
        }

        using var tokenDoc = JsonDocument.Parse(tokenBody);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString()!;
        var refreshToken = tokenDoc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? "" : "";
        var expiresIn = tokenDoc.RootElement.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

        // 2. Fetch user profile from Spotify
        using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var profileRes = await _http.SendAsync(profileRequest);
        var profileBody = await profileRes.Content.ReadAsStringAsync();

        if (!profileRes.IsSuccessStatusCode)
        {
            _logger.LogError("Spotify profile fetch failed [{Status}]: {Body}", profileRes.StatusCode, profileBody);
            throw new UnauthorizedAccessException($"Failed to fetch Spotify profile: {profileBody}");
        }

        using var profileDoc = JsonDocument.Parse(profileBody);
        var root = profileDoc.RootElement;
        var spotifyId = root.GetProperty("id").GetString()!;
        var email = root.TryGetProperty("email", out var em) && em.GetString() != null
            ? em.GetString()!
            : $"{spotifyId}@spotify.user";
        var name = root.TryGetProperty("display_name", out var dn) ? dn.GetString() ?? spotifyId : spotifyId;

        string? imageUrl = null;
        if (root.TryGetProperty("images", out var images) && images.GetArrayLength() > 0)
        {
            imageUrl = images[0].GetProperty("url").GetString();
        }

        // 3. Upsert User in DB
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Name = name,
                Image = imageUrl,
                SpotifyId = spotifyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
        }
        else
        {
            user.Name = name ?? user.Name;
            user.Image = imageUrl ?? user.Image;
            user.SpotifyId = spotifyId ?? user.SpotifyId;
            user.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        // 4. Upsert SpotifyToken in DB
        var token = await _db.SpotifyTokens.FirstOrDefaultAsync(st => st.UserId == user.Id);
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

        if (token == null)
        {
            _db.SpotifyTokens.Add(new SpotifyToken
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            token.AccessToken = accessToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                token.RefreshToken = refreshToken;
            }
            token.ExpiresIn = expiresIn;
            token.ExpiresAt = expiresAt;
        }
        await _db.SaveChangesAsync();

        // 5. Generate JWT
        var jwtToken = GenerateJwt(user);
        var userDto = new UserProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.Image,
            user.SpotifyId,
            user.CreatedAt,
            user.UpdatedAt,
            new SpotifyTokenSummaryDto(expiresAt, DateTime.UtcNow)
        );

        return new AuthResponse(jwtToken, userDto);
    }

    public async Task<UserProfileDto?> GetUserByIdAsync(string userId)
    {
        var user = await _db.Users
            .Include(u => u.SpotifyToken)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        SpotifyTokenSummaryDto? tokenSummary = null;
        if (user.SpotifyToken != null)
        {
            tokenSummary = new SpotifyTokenSummaryDto(user.SpotifyToken.ExpiresAt, user.SpotifyToken.CreatedAt);
        }

        return new UserProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.Image,
            user.SpotifyId,
            user.CreatedAt,
            user.UpdatedAt,
            tokenSummary
        );
    }

    public async Task RevokeTokenAsync(string userId)
    {
        var tokens = await _db.SpotifyTokens.Where(t => t.UserId == userId).ToListAsync();
        if (tokens.Count > 0)
        {
            _db.SpotifyTokens.RemoveRange(tokens);
            await _db.SaveChangesAsync();
        }
    }

    private string GenerateJwt(User user)
    {
        var secret = _config["Jwt:Secret"] ?? "spomusic-super-secret-production-key";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "SpoMusic.Api",
            audience: _config["Jwt:Audience"] ?? "SpoMusic.Clients",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
