using System.Text.Json.Serialization;

namespace SpoMusic.Web.Models;

public record UserProfileViewModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("spotifyId")] string? SpotifyId
);

public record MatchScoreViewModel(
    [property: JsonPropertyName("commonTracks")] int CommonTracks,
    [property: JsonPropertyName("commonArtists")] int CommonArtists,
    [property: JsonPropertyName("recencyScore")] double RecencyScore,
    [property: JsonPropertyName("topSimilarity")] double TopSimilarity,
    [property: JsonPropertyName("total")] double Total
);

public record CommonTrackViewModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("artist")] string Artist,
    [property: JsonPropertyName("album")] string Album,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("spotifyUrl")] string? SpotifyUrl
);

public record MatchItemViewModel(
    [property: JsonPropertyName("user")] UserProfileViewModel User,
    [property: JsonPropertyName("score")] MatchScoreViewModel Score,
    [property: JsonPropertyName("commonTrackDetails")] List<CommonTrackViewModel>? CommonTrackDetails,
    [property: JsonPropertyName("commonArtistNames")] List<string>? CommonArtistNames
);

public record TrackDetailViewModel(
    [property: JsonPropertyName("spotifyId")] string SpotifyId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("artists")] List<string> Artists,
    [property: JsonPropertyName("album")] string Album,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("previewUrl")] string? PreviewUrl,
    [property: JsonPropertyName("url")] string Url
);

public record TopTrackViewModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("rank")] int Rank,
    [property: JsonPropertyName("track")] TrackDetailViewModel Track
);

public record HistoryItemViewModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("playedAt")] DateTime PlayedAt,
    [property: JsonPropertyName("track")] TrackDetailViewModel Track
);

public class ProfileViewModel
{
    public UserProfileViewModel? User { get; set; }
    public List<TopTrackViewModel> TopTracks { get; set; } = new();
    public List<HistoryItemViewModel> RecentHistory { get; set; } = new();
    public string? StatusMessage { get; set; }
}

public class MatchesViewModel
{
    public List<MatchItemViewModel> Matches { get; set; } = new();
    public UserProfileViewModel? CurrentUser { get; set; }
}
