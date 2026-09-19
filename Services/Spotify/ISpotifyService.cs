using SpoMusic.Api.DTOs;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Services.Spotify;

public interface ISpotifyService
{
    Task<string> GetUserTokenAsync(string userId);
    Task<string> RefreshAccessTokenAsync(string userId, string refreshToken);
    Task<SyncResponseDto> SyncUserDataAsync(string userId);
    Task<CurrentlyPlayingDto?> GetCurrentlyPlayingAsync(string userId);
    Task<List<ListeningHistory>> GetUserListeningHistoryAsync(string userId);
    Task<List<UserTopTrack>> GetUserTopTracksAsync(string userId);
    Task<object> CreateBlendPlaylistAsync(string currentUserId, string targetUserId);
}
