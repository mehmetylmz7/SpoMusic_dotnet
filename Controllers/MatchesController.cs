using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpoMusic.Api.Services.Matching;

namespace SpoMusic.Api.Controllers;

[ApiController]
[Route("matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchesController(IMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMatches([FromQuery] string? userId, [FromQuery] int limit = 10)
    {
        if (Request.Headers.Accept.ToString().Contains("text/html"))
        {
            var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "matches", "index.html");
            if (System.IO.File.Exists(htmlPath))
            {
                return PhysicalFile(htmlPath, "text/html");
            }
        }

        var targetUserId = !string.IsNullOrEmpty(userId)
            ? userId
            : (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));

        if (string.IsNullOrEmpty(targetUserId))
        {
            return Unauthorized(new { message = "Authentication required or 'userId' parameter needed" });
        }

        int max = Math.Clamp(limit, 1, 50);
        var matches = await _matchingService.GetMatchesForUserAsync(targetUserId, max);
        return Ok(matches);
    }

    [HttpGet("compare")]
    [AllowAnonymous]
    public async Task<IActionResult> CompareUsers([FromQuery] string? user1, [FromQuery] string? user2)
    {
        var targetUser1 = !string.IsNullOrEmpty(user1)
            ? user1
            : (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));

        if (string.IsNullOrEmpty(targetUser1) || string.IsNullOrEmpty(user2))
        {
            return BadRequest(new { message = "Query parameters 'user1' and 'user2' are required" });
        }

        try
        {
            var comparison = await _matchingService.CompareUsersAsync(targetUser1, user2);
            return Ok(comparison);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
