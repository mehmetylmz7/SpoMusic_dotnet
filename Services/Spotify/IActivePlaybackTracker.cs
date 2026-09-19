namespace SpoMusic.Api.Services.Spotify;

public record ActiveUserPlayback(
    string UserId,
    string UserName,
    string? UserImage,
    string TrackId,
    string TrackName,
    List<string> Artists,
    string? ImageUrl,
    DateTime LastActiveUtc
);

public interface IActivePlaybackTracker
{
    void RecordActivePlayback(ActiveUserPlayback playback);
    List<ActiveUserPlayback> GetCoListeners(string currentUserId, string trackId);
    void RemovePlayback(string userId);
}