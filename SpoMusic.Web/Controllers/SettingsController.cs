using Microsoft.AspNetCore.Mvc;
using SpoMusic.Web.Services;

namespace SpoMusic.Web.Controllers;

public class SettingsController : Controller
{
    private readonly ISpoMusicApiClient _api;

    public SettingsController(ISpoMusicApiClient api)
    {
        _api = api;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Index()
    {
        var token = Request.Cookies["spomusic_token"];
        var user = !string.IsNullOrEmpty(token) ? await _api.GetCurrentUserAsync(token) : null;
        ViewBag.User = user;
        ViewBag.IsConnected = !string.IsNullOrEmpty(token);
        return View();
    }

    [HttpPost("settings/revoke")]
    public async Task<IActionResult> Revoke()
    {
        var token = Request.Cookies["spomusic_token"];
        if (!string.IsNullOrEmpty(token))
        {
            await _api.RevokeTokenAsync(token);
            Response.Cookies.Delete("spomusic_token");
            Response.Cookies.Delete("spomusic_username");
            Response.Cookies.Delete("spomusic_userimage");
        }
        return RedirectToAction("Login", "Auth");
    }
}
