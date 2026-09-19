using SpoMusic.Web.Models;

namespace SpoMusic.Web.Services;

public interface ISpoMusicApiClient
{
    string GetSpotifyAuthUrl(string? platform = null, string? returnUrl = null, string? publicApiUrl = null);
    Task<(string? Token, UserProfileViewModel? User)> ExchangeCodeAsync(string code);
    Task<UserProfileViewModel?> GetCurrentUserAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<List<MatchItemViewModel>> GetMatchesAsync(string? token = null, string? userId = null);
    Task<List<TopTrackViewModel>> GetTopTracksAsync(string? token = null, string? userId = null);
    Task<List<HistoryItemViewModel>> GetHistoryAsync(string? token = null, string? userId = null);
    Task<bool> SyncUserDataAsync(string token);
    Task<string?> CreateBlendAsync(string targetUserId, string token);
}
