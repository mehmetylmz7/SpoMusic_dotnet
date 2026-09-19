using System.Text.Json.Serialization;

namespace SpoMusic.Api.DTOs;

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
