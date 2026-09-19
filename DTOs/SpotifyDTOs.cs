using System.Text.Json.Serialization;

namespace SpoMusic.Api.DTOs;

public record CoListenerDto(
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("trackId")] string TrackId,
    [property: JsonPropertyName("trackName")] string TrackName,
    [property: JsonPropertyName("artists")] List<string> Artists,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("startedTogetherAt")] DateTime StartedTogetherAt
);

public record CurrentlyPlayingDto(
    [property: JsonPropertyName("isPlaying")] bool IsPlaying,
    [property: JsonPropertyName("trackId")] string? TrackId,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("artists")] List<string> Artists,
    [property: JsonPropertyName("album")] string? Album,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("previewUrl")] string? PreviewUrl,
    [property: JsonPropertyName("spotifyUrl")] string? SpotifyUrl,
    [property: JsonPropertyName("progressMs")] int ProgressMs,
    [property: JsonPropertyName("durationMs")] int DurationMs,
    [property: JsonPropertyName("deviceName")] string? DeviceName,
    [property: JsonPropertyName("coListeners")] List<CoListenerDto>? CoListeners = null
);

public record SyncResponseDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("syncedTracks")] int SyncedTracks,
    [property: JsonPropertyName("spotifyError")] string? SpotifyError = null,
    [property: JsonPropertyName("spotifyStatus")] int? SpotifyStatus = null,
    [property: JsonPropertyName("message")] string? Message = null
);

public record BlendRequestDto(
    [property: JsonPropertyName("targetUserId")] string TargetUserId
);

public record BlendResponseDto(
    [property: JsonPropertyName("playlistId")] string PlaylistId,
    [property: JsonPropertyName("playlistUrl")] string PlaylistUrl,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("trackCount")] int TrackCount,
    [property: JsonPropertyName("artists")] List<string> Artists
);