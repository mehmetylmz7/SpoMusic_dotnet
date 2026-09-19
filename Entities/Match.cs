using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoMusic.Api.Entities;

public class Match
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string User1Id { get; set; } = string.Empty;
    [ForeignKey(nameof(User1Id))]
    public User User1 { get; set; } = null!;

    [Required]
    public string User2Id { get; set; } = string.Empty;
    [ForeignKey(nameof(User2Id))]
    public User User2 { get; set; } = null!;

    public double MatchScore { get; set; }
    public int CommonTracks { get; set; }
    public int CommonArtists { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
