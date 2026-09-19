using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoMusic.Api.Entities;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? SpotifyId { get; set; }
    public string? Name { get; set; }
    
    [Required]
    public string Email { get; set; } = string.Empty;
    public string? Image { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public SpotifyToken? SpotifyToken { get; set; }
    public ICollection<UserTopTrack> TopTracks { get; set; } = new List<UserTopTrack>();
    public ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    public ICollection<Match> MatchesInitiated { get; set; } = new List<Match>();
    public ICollection<Match> MatchesReceived { get; set; } = new List<Match>();
}
