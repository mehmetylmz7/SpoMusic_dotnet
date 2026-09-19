using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoMusic.Api.Entities;

public class SpotifyToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public string AccessToken { get; set; } = string.Empty;
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public int ExpiresIn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
