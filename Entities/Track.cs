using System.ComponentModel.DataAnnotations;

namespace SpoMusic.Api.Entities;

public class Track
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string SpotifyId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public List<string> Artists { get; set; } = new();

    [Required]
    public string Album { get; set; } = string.Empty;

    public int DurationMs { get; set; }
    public int Popularity { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    public ICollection<UserTopTrack> UserTopTracks { get; set; } = new List<UserTopTrack>();
}
