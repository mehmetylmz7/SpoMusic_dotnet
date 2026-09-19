using Microsoft.AspNetCore.Mvc;
using SpoMusic.Web.Services;

namespace SpoMusic.Web.Controllers;

public class AuthController : Controller
{
    private readonly ISpoMusicApiClient _api;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISpoMusicApiClient api, ILogger<AuthController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? code, [FromQuery] string? error)
    {
        // Forward query params if user lands on /login with code or error
        if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(error))
        {
            return RedirectToAction("Callback", new { code, error });
        }

        var token = Request.Cookies["spomusic_token"];
        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Matches");
        }
        return View();
    }

    [HttpGet("auth/login-spotify")]
    public IActionResult LoginSpotify()
    {
        // Detect client-accessible public return URL
        var scheme = Request.Scheme;
        var host = Request.Host.Value; // e.g. "localhost:3000" or "192.168.25.243:3000"
        var returnUrl = $"{scheme}://{host}/auth/callback";

        // If client connects via 192.168.25.243, point public API URL to 192.168.25.243:4000
        string? publicApiUrl = null;
        if (Request.Host.Host == "192.168.25.243")
        {
            publicApiUrl = "http://192.168.25.243:4000";
        }

        var authUrl = _api.GetSpotifyAuthUrl(returnUrl: returnUrl, publicApiUrl: publicApiUrl);
        return Redirect(authUrl);
    }

    [HttpGet("auth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
        {
            TempData["Error"] = error ?? "Spotify girişi başarısız oldu.";
            return RedirectToAction("Login");
        }

        var (token, user) = await _api.ExchangeCodeAsync(code);
        if (string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Yetkilendirme kodu doğrulanamadı veya süresi doldu. Lütfen tekrar deneyin.";
            return RedirectToAction("Login");
        }

        Response.Cookies.Append("spomusic_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        if (user != null)
        {
            Response.Cookies.Append("spomusic_username", user.Name ?? user.Email, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            if (!string.IsNullOrEmpty(user.Image))
            {
                Response.Cookies.Append("spomusic_userimage", user.Image, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
        }

        // Asynchronously sync user's latest Spotify listening data
        _ = Task.Run(async () =>
        {
            try
            {
                await _api.SyncUserDataAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background user data sync after login encountered an error");
            }
        });

        return RedirectToAction("Index", "Matches");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("spomusic_token");
        Response.Cookies.Delete("spomusic_username");
        Response.Cookies.Delete("spomusic_userimage");
        return RedirectToAction("Index", "Home");
    }
}
