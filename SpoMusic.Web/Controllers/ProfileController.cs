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

        var currentlyPlaying = await _api.GetCurrentlyPlayingAsync(token, targetUserId);
        var topTracks = await _api.GetTopTracksAsync(token, targetUserId);
        var history = await _api.GetHistoryAsync(token, targetUserId);

        var vm = new ProfileViewModel
        {
            User = user ?? new UserProfileViewModel(targetUserId, "Demo Dinleyici", "demo@spomusic.app", null, null),
            CurrentlyPlaying = currentlyPlaying,
            TopTracks = topTracks,
            RecentHistory = history,
            StatusMessage = TempData["StatusMessage"]?.ToString()
        };

        return View(vm);
    }

    [HttpGet("profile/current-live")]
    public async Task<IActionResult> GetCurrentLive([FromQuery] string? userId)
    {
        var token = Request.Cookies["spomusic_token"];
        var user = !string.IsNullOrEmpty(token) ? await _api.GetCurrentUserAsync(token) : null;
        var targetUserId = userId ?? user?.Id ?? "user-demo";

        var current = await _api.GetCurrentlyPlayingAsync(token, targetUserId);
        var history = await _api.GetHistoryAsync(token, targetUserId);

        return Json(new
        {
            current = current ?? new CurrentlyPlayingViewModel(false, null, null, new List<string>(), null, null, null, null, 0, 0, null),
            history = history ?? new List<HistoryItemViewModel>()
        });
    }

    [HttpGet("live/listener-sync")]
    public async Task<IActionResult> ListenerSync()
    {
        var token = Request.Cookies["spomusic_token"];
        if (string.IsNullOrEmpty(token))
        {
            return Json(new { isPlaying = false, coListeners = new List<CoListenerViewModel>() });
        }

        var user = await _api.GetCurrentUserAsync(token);
        if (user == null)
        {
            return Json(new { isPlaying = false, coListeners = new List<CoListenerViewModel>() });
        }

        var current = await _api.GetCurrentlyPlayingAsync(token, user.Id);
        return Json(new
        {
            isPlaying = current?.IsPlaying ?? false,
            trackId = current?.TrackId,
            trackName = current?.Name,
            artists = current?.Artists ?? new List<string>(),
            imageUrl = current?.ImageUrl,
            coListeners = current?.CoListeners ?? new List<CoListenerViewModel>()
        });
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
