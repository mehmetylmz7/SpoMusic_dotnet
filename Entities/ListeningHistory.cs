using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoMusic.Api.Entities;

public class ListeningHistory
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public string TrackId { get; set; } = string.Empty;
    public Track Track { get; set; } = null!;

    public DateTime PlayedAt { get; set; }
}
