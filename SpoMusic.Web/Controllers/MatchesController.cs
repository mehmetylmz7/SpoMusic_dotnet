using Microsoft.AspNetCore.Mvc;
using SpoMusic.Web.Models;
using SpoMusic.Web.Services;

namespace SpoMusic.Web.Controllers;

public class MatchesController : Controller
{
    private readonly ISpoMusicApiClient _api;

    public MatchesController(ISpoMusicApiClient api)
    {
        _api = api;
    }

    [HttpGet("matches")]
    public async Task<IActionResult> Index([FromQuery] string? userId)
    {
        var token = Request.Cookies["spomusic_token"];
        UserProfileViewModel? currentUser = null;

        if (!string.IsNullOrEmpty(token))
        {
            currentUser = await _api.GetCurrentUserAsync(token);
        }

        // Fallback to user-demo if no token or userId specified
        var targetUserId = userId ?? currentUser?.Id ?? "user-demo";
        var matches = await _api.GetMatchesAsync(token, targetUserId);

        var vm = new MatchesViewModel
        {
            Matches = matches,
            CurrentUser = currentUser
        };

        return View(vm);
    }

    [HttpPost("matches/blend")]
    public async Task<IActionResult> CreateBlend([FromBody] BlendRequestModel request)
    {
        var token = Request.Cookies["spomusic_token"];
        if (string.IsNullOrEmpty(token))
        {
            return Json(new { success = false, message = "Blend oluşturmak için giriş yapmalısınız." });
        }

        var message = await _api.CreateBlendAsync(request.TargetUserId, token);
        return Json(new { success = message != null, message = message ?? "Blend oluşturulamadı." });
    }
}

public record BlendRequestModel(string TargetUserId);
