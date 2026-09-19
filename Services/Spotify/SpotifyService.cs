using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SpoMusic.Api.Data;
using SpoMusic.Api.DTOs;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Services.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly ILogger<SpotifyService> _logger;
    private readonly IActivePlaybackTracker _playbackTracker;

    public SpotifyService(AppDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<SpotifyService> logger, IActivePlaybackTracker playbackTracker)
    {
        _db = db;
        _config = config;
        _http = httpClientFactory.CreateClient("SpotifyApi");
        _logger = logger;
        _playbackTracker = playbackTracker;
    }

    public async Task<string> GetUserTokenAsync(string userId)
    {
        var token = await _db.SpotifyTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        if (token == null)
        {
            throw new UnauthorizedAccessException("No Spotify token found for user. Please login first.");
        }

        // Refresh if expiring within 60 seconds
        if ((token.ExpiresAt - DateTime.UtcNow).TotalSeconds < 60)
        {
            return await RefreshAccessTokenAsync(userId, token.RefreshToken);
        }

        return token.AccessToken;
    }

    public async Task<string> RefreshAccessTokenAsync(string userId, string refreshToken)
    {
        var clientId = _config["Spotify:ClientId"] ?? "";
        var clientSecret = _config["Spotify:ClientSecret"] ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        });

        var res = await _http.SendAsync(request);
        var body = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException($"Failed to refresh Spotify access token: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;
        var newRefreshToken = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

        var token = await _db.SpotifyTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        if (token != null)
        {
            token.AccessToken = accessToken;
            token.ExpiresIn = expiresIn;
            token.ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            if (!string.IsNullOrEmpty(newRefreshToken))
            {
                token.RefreshToken = newRefreshToken;
            }
            await _db.SaveChangesAsync();
        }

        return accessToken;
    }

    public async Task<CurrentlyPlayingDto?> GetCurrentlyPlayingAsync(string userId)
    {
        try
        {
            var accessToken = await GetUserTokenAsync(userId);
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/currently-playing");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var res = await _http.SendAsync(req);
            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new CurrentlyPlayingDto(false, null, null, new List<string>(), null, null, null, null, 0, 0, null);
            }

            if (!res.IsSuccessStatusCode)
            {
                return new CurrentlyPlayingDto(false, null, null, new List<string>(), null, null, null, null, 0, 0, null);
            }

            var body = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return new CurrentlyPlayingDto(false, null, null, new List<string>(), null, null, null, null, 0, 0, null);
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var isPlaying = root.TryGetProperty("is_playing", out var ip) && ip.GetBoolean();
            var progressMs = root.TryGetProperty("progress_ms", out var pms) ? pms.GetInt32() : 0;

            if (!root.TryGetProperty("item", out var item) || item.ValueKind != JsonValueKind.Object)
            {
                return new CurrentlyPlayingDto(isPlaying, null, null, new List<string>(), null, null, null, null, progressMs, 0, null);
            }

            var trackId = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            var name = item.TryGetProperty("name", out var nEl) ? nEl.GetString() : "Bilinmeyen Parça";
            var durationMs = item.TryGetProperty("duration_ms", out var dmEl) ? dmEl.GetInt32() : 0;

            var artists = new List<string>();
            if (item.TryGetProperty("artists", out var artistsEl))
            {
                foreach (var a in artistsEl.EnumerateArray())
                {
                    if (a.TryGetProperty("name", out var an) && an.GetString() != null)
                    {
                        artists.Add(an.GetString()!);
                    }
                }
            }

            string? albumName = null;
            string? imageUrl = null;
            if (item.TryGetProperty("album", out var albumEl))
            {
                if (albumEl.TryGetProperty("name", out var abn))
                {
                    albumName = abn.GetString();
                }
                if (albumEl.TryGetProperty("images", out var imgEl) && imgEl.GetArrayLength() > 0)
                {
                    imageUrl = imgEl[0].GetProperty("url").GetString();
                }
            }

            string? spotifyUrl = null;
            if (item.TryGetProperty("external_urls", out var extUrls) && extUrls.TryGetProperty("spotify", out var su))
            {
                spotifyUrl = su.GetString();
            }

            string? previewUrl = null;
            if (item.TryGetProperty("preview_url", out var pu))
            {
                previewUrl = pu.GetString();
            }

            string? deviceName = null;
            if (root.TryGetProperty("device", out var devEl) && devEl.TryGetProperty("name", out var dn))
            {
                deviceName = dn.GetString();
            }

            // Anında Son Dinlenenlere Kaydetme (Şarkı açıldığı anda bitmesini beklemeden geçmişe ekler)
            if (isPlaying && !string.IsNullOrEmpty(trackId))
            {
                try
                {
                    var lastHistory = await _db.ListeningHistories
                        .Where(lh => lh.UserId == userId)
                        .OrderByDescending(lh => lh.PlayedAt)
                        .FirstOrDefaultAsync();

                    if (lastHistory == null || lastHistory.TrackId != trackId)
                    {
                        var trackExists = await _db.Tracks.AnyAsync(t => t.SpotifyId == trackId);
                        if (!trackExists)
                        {
                            _db.Tracks.Add(new Track
                            {
                                SpotifyId = trackId,
                                Name = name ?? "Bilinmeyen Parça",
                                Artists = artists,
                                Album = albumName ?? "",
                                DurationMs = durationMs,
                                ImageUrl = imageUrl,
                                Url = spotifyUrl ?? "",
                                PreviewUrl = previewUrl,
                                CreatedAt = DateTime.UtcNow
                            });
                            await _db.SaveChangesAsync();
                        }

                        _db.ListeningHistories.Add(new ListeningHistory
                        {
                            UserId = userId,
                            TrackId = trackId,
                            PlayedAt = DateTime.UtcNow
                        });
                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Auto-recording listening history failed for user {UserId}", userId);
                }
            }

            List<CoListenerDto> coListeners = new();
            if (isPlaying && !string.IsNullOrEmpty(trackId))
            {
                try
                {
                    var user = await _db.Users.FindAsync(userId);
                    _playbackTracker.RecordActivePlayback(new ActiveUserPlayback(
                        UserId: userId,
                        UserName: user?.Name ?? "Müziksever",
                        UserImage: user?.Image,
                        TrackId: trackId,
                        TrackName: name ?? "Bilinmeyen Parça",
                        Artists: artists,
                        ImageUrl: imageUrl,
                        LastActiveUtc: DateTime.UtcNow
                    ));

                    coListeners = _playbackTracker.GetCoListeners(userId, trackId)
                        .Select(c => new CoListenerDto(
                            UserId: c.UserId,
                            Name: c.UserName,
                            Image: c.UserImage,
                            TrackId: c.TrackId,
                            TrackName: c.TrackName,
                            Artists: c.Artists,
                            ImageUrl: c.ImageUrl,
                            StartedTogetherAt: c.LastActiveUtc
                        ))
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record active playback or co-listeners for user {UserId}", userId);
                }
            }
            else
            {
                _playbackTracker.RemovePlayback(userId);
            }

            return new CurrentlyPlayingDto(
                IsPlaying: isPlaying,
                TrackId: trackId,
                Name: name,
                Artists: artists,
                Album: albumName,
                ImageUrl: imageUrl,
                PreviewUrl: previewUrl,
                SpotifyUrl: spotifyUrl,
                ProgressMs: progressMs,
                DurationMs: durationMs,
                DeviceName: deviceName,
                CoListeners: coListeners
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetCurrentlyPlayingAsync encountered an error for user {UserId}", userId);
            return new CurrentlyPlayingDto(false, null, null, new List<string>(), null, null, null, null, 0, 0, null);
        }
    }

    public async Task<SyncResponseDto> SyncUserDataAsync(string userId)
    {
        var accessToken = await GetUserTokenAsync(userId);
        int syncedCount = 0;

        // 1. Fetch recent plays (Son dinlenenler)
        using var recentReq = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/recently-played?limit=50");
        recentReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var recentRes = await _http.SendAsync(recentReq);

        if (recentRes.IsSuccessStatusCode)
        {
            var body = await recentRes.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                syncedCount += await StoreRecentlyPlayedAsync(userId, items);
            }
        }

        // 2. Fetch top tracks: Check medium_term first, and if sparse, enrich with long_term
        var topItems = new List<JsonElement>();
        var seenIds = new HashSet<string>();

        using var topReq = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/top/tracks?limit=20&time_range=medium_term");
        topReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var topRes = await _http.SendAsync(topReq);

        if (topRes.IsSuccessStatusCode)
        {
            var body = await topRes.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                foreach (var el in items.EnumerateArray())
                {
                    if (el.TryGetProperty("id", out var idEl) && seenIds.Add(idEl.GetString()!))
                    {
                        topItems.Add(el.Clone());
                    }
                }
            }
        }

        // If medium_term returned fewer than 5 tracks, enrich with long_term (all-time top tracks)
        if (topItems.Count < 5)
        {
            using var longReq = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/top/tracks?limit=20&time_range=long_term");
            longReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var longRes = await _http.SendAsync(longReq);

            if (longRes.IsSuccessStatusCode)
            {
                var body = await longRes.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    foreach (var el in items.EnumerateArray())
                    {
                        if (el.TryGetProperty("id", out var idEl) && seenIds.Add(idEl.GetString()!))
                        {
                            topItems.Add(el.Clone());
                        }
                    }
                }
            }
        }

        if (topItems.Count > 0)
        {
            syncedCount += await StoreTopTrackElementsAsync(userId, topItems);
        }

        return new SyncResponseDto(
            Success: true,
            SyncedTracks: syncedCount
        );
    }

    private async Task<int> StoreRecentlyPlayedAsync(string userId, JsonElement items)
    {
        var trackDict = new Dictionary<string, Track>();
        var historyList = new List<ListeningHistory>();

        foreach (var item in items.EnumerateArray())
        {
            var trackEl = item.TryGetProperty("track", out var t) ? t : item;
            if (!trackEl.TryGetProperty("id", out var idEl) || string.IsNullOrEmpty(idEl.GetString())) continue;

            var spotifyId = idEl.GetString()!;
            if (!trackDict.ContainsKey(spotifyId))
            {
                trackDict[spotifyId] = ParseTrack(trackEl);
            }

            var playedAt = item.TryGetProperty("played_at", out var pa) && pa.TryGetDateTime(out var dt)
                ? dt.ToUniversalTime()
                : DateTime.UtcNow;

            historyList.Add(new ListeningHistory
            {
                UserId = userId,
                TrackId = spotifyId,
                PlayedAt = playedAt
            });
        }

        await UpsertTracksAsync(trackDict.Values);

        foreach (var h in historyList)
        {
            var exists = await _db.ListeningHistories
                .AnyAsync(lh => lh.UserId == h.UserId && lh.TrackId == h.TrackId && lh.PlayedAt == h.PlayedAt);
            if (!exists)
            {
                _db.ListeningHistories.Add(h);
            }
        }

        await _db.SaveChangesAsync();
        return historyList.Count;
    }

    private async Task<int> StoreTopTrackElementsAsync(string userId, List<JsonElement> items)
    {
        var trackDict = new Dictionary<string, Track>();
        var topTrackItems = new List<(string SpotifyId, int Rank, double Score)>();

        int total = items.Count;
        int index = 0;

        foreach (var trackEl in items)
        {
            if (!trackEl.TryGetProperty("id", out var idEl) || string.IsNullOrEmpty(idEl.GetString())) continue;

            var spotifyId = idEl.GetString()!;
            if (!trackDict.ContainsKey(spotifyId))
            {
                trackDict[spotifyId] = ParseTrack(trackEl);
            }

            topTrackItems.Add((spotifyId, index + 1, (double)(total - index) / total));
            index++;
        }

        await UpsertTracksAsync(trackDict.Values);

        // Remove old top tracks for clean re-sync
        var existing = await _db.UserTopTracks.Where(ut => ut.UserId == userId).ToListAsync();
        _db.UserTopTracks.RemoveRange(existing);

        foreach (var tti in topTrackItems)
        {
            _db.UserTopTracks.Add(new UserTopTrack
            {
                UserId = userId,
                TrackId = tti.SpotifyId,
                Rank = tti.Rank,
                Score = tti.Score,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return topTrackItems.Count;
    }

    private async Task UpsertTracksAsync(IEnumerable<Track> tracks)
    {
        var list = tracks.ToList();
        var spotifyIds = list.Select(t => t.SpotifyId).ToList();

        var existingIds = await _db.Tracks
            .Where(t => spotifyIds.Contains(t.SpotifyId))
            .Select(t => t.SpotifyId)
            .ToListAsync();

        var newTracks = list.Where(t => !existingIds.Contains(t.SpotifyId)).ToList();
        if (newTracks.Count > 0)
        {
            _db.Tracks.AddRange(newTracks);
            await _db.SaveChangesAsync();
        }
    }

    private static Track ParseTrack(JsonElement el)
    {
        var artists = new List<string>();
        if (el.TryGetProperty("artists", out var artistsEl))
        {
            foreach (var a in artistsEl.EnumerateArray())
            {
                if (a.TryGetProperty("name", out var n) && n.GetString() != null)
                {
                    artists.Add(n.GetString()!);
                }
            }
        }

        string albumName = "";
        string? imageUrl = null;
        if (el.TryGetProperty("album", out var albumEl))
        {
            if (albumEl.TryGetProperty("name", out var an) && an.GetString() != null)
            {
                albumName = an.GetString()!;
            }
            if (albumEl.TryGetProperty("images", out var imagesEl) && imagesEl.GetArrayLength() > 0)
            {
                imageUrl = imagesEl[0].GetProperty("url").GetString();
            }
        }

        string spotifyUrl = "";
        if (el.TryGetProperty("external_urls", out var extUrls) && extUrls.TryGetProperty("spotify", out var spUrl))
        {
            spotifyUrl = spUrl.GetString() ?? "";
        }

        return new Track
        {
            SpotifyId = el.GetProperty("id").GetString()!,
            Name = el.TryGetProperty("name", out var tn) ? tn.GetString() ?? "Unknown Track" : "Unknown Track",
            Artists = artists,
            Album = albumName,
            DurationMs = el.TryGetProperty("duration_ms", out var dm) ? dm.GetInt32() : 0,
            Popularity = el.TryGetProperty("popularity", out var pop) ? pop.GetInt32() : 0,
            Url = spotifyUrl,
            ImageUrl = imageUrl,
            PreviewUrl = el.TryGetProperty("preview_url", out var pu) ? pu.GetString() : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<List<ListeningHistory>> GetUserListeningHistoryAsync(string userId)
    {
        return await _db.ListeningHistories
            .Where(lh => lh.UserId == userId)
            .Include(lh => lh.Track)
            .OrderByDescending(lh => lh.PlayedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<List<UserTopTrack>> GetUserTopTracksAsync(string userId)
    {
        return await _db.UserTopTracks
            .Where(ut => ut.UserId == userId)
            .Include(ut => ut.Track)
            .OrderBy(ut => ut.Rank)
            .Take(20)
            .ToListAsync();
    }

    public async Task<object> CreateBlendPlaylistAsync(string currentUserId, string targetUserId)
    {
        var users = await _db.Users
            .Where(u => u.Id == currentUserId || u.Id == targetUserId)
            .Include(u => u.TopTracks).ThenInclude(tt => tt.Track)
            .ToListAsync();

        var u1 = users.FirstOrDefault(u => u.Id == currentUserId);
        var u2 = users.FirstOrDefault(u => u.Id == targetUserId);

        var name1 = u1?.Name ?? "Sen";
        var name2 = u2?.Name ?? "Arkadaşın";
        var playlistName = $"SpoMusic Blend: {name1} & {name2}";

        var tracks1 = u1?.TopTracks.Select(t => t.Track).Where(t => t != null).ToList() ?? new List<Track>();
        var tracks2 = u2?.TopTracks.Select(t => t.Track).Where(t => t != null).ToList() ?? new List<Track>();

        var trackDict = new Dictionary<string, Track>();
        foreach (var t in tracks1) trackDict[t.SpotifyId] = t;
        foreach (var t in tracks2) trackDict[t.SpotifyId] = t;

        var combinedTracks = trackDict.Values.Take(20).ToList();

        return new
        {
            success = true,
            playlistName,
            trackCount = combinedTracks.Count,
            tracks = combinedTracks,
            message = $"Blend çalma listesi \"{playlistName}\" hazırlandı!"
        };
    }
}
