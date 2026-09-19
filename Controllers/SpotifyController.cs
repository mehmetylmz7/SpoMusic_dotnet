using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpoMusic.Api.DTOs;
using SpoMusic.Api.Services.Spotify;

namespace SpoMusic.Api.Controllers;

[ApiController]
[Route("spotify")]
public class SpotifyController : ControllerBase
{
    private readonly ISpotifyService _spotifyService;
    private readonly ILogger<SpotifyController> _logger;

    public SpotifyController(ISpotifyService spotifyService, ILogger<SpotifyController> logger)
    {
        _spotifyService = spotifyService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("sync")]
    public async Task<IActionResult> SyncUserData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        try
        {
            var result = await _spotifyService.SyncUserDataAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spotify sync error for user {UserId}", userId);
            return Ok(new SyncResponseDto(
                Success: false,
                SyncedTracks: 0,
                Message: ex.Message
            ));
        }
    }

    [HttpGet("history")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHistory([FromQuery] string? userId)
    {
        var targetUserId = !string.IsNullOrEmpty(userId)
            ? userId
            : (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));

        if (string.IsNullOrEmpty(targetUserId))
        {
            return Unauthorized(new { message = "Authentication required or 'userId' parameter needed" });
        }

        var history = await _spotifyService.GetUserListeningHistoryAsync(targetUserId);
        return Ok(history);
    }

    [HttpGet("top")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopTracks([FromQuery] string? userId)
    {
        var targetUserId = !string.IsNullOrEmpty(userId)
            ? userId
            : (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));

        if (string.IsNullOrEmpty(targetUserId))
        {
            return Unauthorized(new { message = "Authentication required or 'userId' parameter needed" });
        }

        var topTracks = await _spotifyService.GetUserTopTracksAsync(targetUserId);
        return Ok(topTracks);
    }

    [Authorize]
    [HttpPost("blend")]
    public async Task<IActionResult> CreateBlend([FromBody] BlendRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        if (string.IsNullOrEmpty(request?.TargetUserId))
        {
            return BadRequest(new { message = "targetUserId is required" });
        }

        var result = await _spotifyService.CreateBlendPlaylistAsync(currentUserId, request.TargetUserId);
        return Ok(result);
    }
}
