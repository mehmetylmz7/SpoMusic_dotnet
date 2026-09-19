using Microsoft.AspNetCore.Mvc;
using SpoMusic.Web.Models;
using SpoMusic.Web.Services;

namespace SpoMusic.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ISpoMusicApiClient _api;

    public ProfileController(ISpoMusicApiClient api)
    {
        _api = api;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Index([FromQuery] string? userId)
    {
        var token = Request.Cookies["spomusic_token"];
        UserProfileViewModel? user = null;

        if (!string.IsNullOrEmpty(token))
        {
            user = await _api.GetCurrentUserAsync(token);
        }

        var targetUserId = userId ?? user?.Id ?? "user-demo";

        var topTracks = await _api.GetTopTracksAsync(token, targetUserId);
        var history = await _api.GetHistoryAsync(token, targetUserId);

        var vm = new ProfileViewModel
        {
            User = user ?? new UserProfileViewModel(targetUserId, "Demo Dinleyici", "demo@spomusic.app", null, null),
            TopTracks = topTracks,
            RecentHistory = history,
            StatusMessage = TempData["StatusMessage"]?.ToString()
        };

        return View(vm);
    }

    [HttpPost("profile/sync")]
    public async Task<IActionResult> Sync()
    {
        var token = Request.Cookies["spomusic_token"];
        if (string.IsNullOrEmpty(token))
        {
            TempData["StatusMessage"] = "Senkronizasyon için lütfen önce Spotify ile giriş yapın.";
            return RedirectToAction("Index");
        }

        var success = await _api.SyncUserDataAsync(token);
        TempData["StatusMessage"] = success
            ? "Müzik geçmişiniz ve en çok dinlenenler başarıyla senkronize edildi! 🎶"
            : "Senkronizasyon sırasında bir hata oluştu.";

        return RedirectToAction("Index");
    }
}
