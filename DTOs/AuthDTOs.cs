using System.Text.Json.Serialization;

namespace SpoMusic.Api.DTOs;

public record ExchangeCodeRequest(
    [property: JsonPropertyName("code")] string Code
);

public record SpotifyTokenSummaryDto(
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record UserProfileDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("spotifyId")] string? SpotifyId,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("spotifyToken")] SpotifyTokenSummaryDto? SpotifyToken
);

public record AuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("user")] UserProfileDto User
);
