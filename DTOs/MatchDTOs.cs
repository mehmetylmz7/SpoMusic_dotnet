using System.Text.Json.Serialization;

namespace SpoMusic.Api.DTOs;

public record MatchScoreDto(
    [property: JsonPropertyName("commonTracks")] int CommonTracks,
    [property: JsonPropertyName("commonArtists")] int CommonArtists,
    [property: JsonPropertyName("recencyScore")] double RecencyScore,
    [property: JsonPropertyName("topSimilarity")] double TopSimilarity,
    [property: JsonPropertyName("total")] double Total
);

public record UserSummaryDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("email")] string Email
);

public record CommonTrackDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("artist")] string Artist,
    [property: JsonPropertyName("album")] string Album,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("spotifyUrl")] string? SpotifyUrl
);

public record MatchResultItemDto(
    [property: JsonPropertyName("user")] UserSummaryDto User,
    [property: JsonPropertyName("score")] MatchScoreDto Score,
    [property: JsonPropertyName("commonTrackDetails")] List<CommonTrackDto>? CommonTrackDetails = null,
    [property: JsonPropertyName("commonArtistNames")] List<string>? CommonArtistNames = null
);

public record CompareUsersResponseDto(
    [property: JsonPropertyName("user1")] UserSummaryDto User1,
    [property: JsonPropertyName("user2")] UserSummaryDto User2,
    [property: JsonPropertyName("score")] MatchScoreDto Score,
    [property: JsonPropertyName("commonTrackDetails")] List<CommonTrackDto> CommonTrackDetails,
    [property: JsonPropertyName("commonArtistNames")] List<string> CommonArtistNames
);
