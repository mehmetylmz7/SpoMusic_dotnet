using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpoMusic.Api.DTOs;
using SpoMusic.Api.Services.Auth;

namespace SpoMusic.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IConfiguration config, ILogger<AuthController> logger)
    {
        _authService = authService;
        _config = config;
        _logger = logger;
    }

    [HttpGet("spotify")]
    public IActionResult LoginWithSpotify([FromQuery] string? platform, [FromQuery] string? returnUrl, [FromQuery] string? redirectUri)
    {
        if (string.IsNullOrEmpty(redirectUri))
        {
            redirectUri = _config["Spotify:RedirectUri"] ?? "http://127.0.0.1:4000/auth/spotify/callback";
        }

        var url = _authService.GetSpotifyAuthUrl(platform, returnUrl, redirectUri);
        return Redirect(url);
    }

    [HttpGet("spotify/callback")]
    public async Task<IActionResult> SpotifyCallback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? state)
    {
        var isMobile = state?.StartsWith("mobile_") ?? false;
        var defaultFrontendUrl = _config["Client:FrontendUrl"] ?? "http://localhost:3000/auth/callback";
        var mobileDeepLink = _config["Client:MobileDeepLink"] ?? "spomusic://login";

        var stateValidation = !string.IsNullOrEmpty(state) 
            ? _authService.ValidateState(state) 
            : new StateValidationResult(false, null, null);

        // Determine destination frontend URL
        string frontendUrl;
        if (isMobile)
        {
            frontendUrl = mobileDeepLink;
        }
        else if (!string.IsNullOrEmpty(stateValidation.ReturnUrl))
        {
            frontendUrl = stateValidation.ReturnUrl;
        }
        else
        {
            frontendUrl = defaultFrontendUrl;
        }

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
        {
            var errParam = Uri.EscapeDataString(error ?? "Spotify authorization failed");
            var sep = frontendUrl.Contains('?') ? "&" : "?";
            return Redirect($"{frontendUrl}{sep}error={errParam}");
        }

        // CSRF state validation
        if (!stateValidation.IsValid)
        {
            var errParam = Uri.EscapeDataString("Geçersiz ya da süresi dolmuş oturum. Lütfen tekrar deneyin.");
            var sep = frontendUrl.Contains('?') ? "&" : "?";
            return Redirect($"{frontendUrl}{sep}error={errParam}");
        }

        try
        {
            var result = await _authService.HandleSpotifyCallbackAsync(code, stateValidation.RedirectUri);
            var oneTimeCode = _authService.CreateOneTimeCode(result.Token, result.User);
            var sep = frontendUrl.Contains('?') ? "&" : "?";
            return Redirect($"{frontendUrl}{sep}code={oneTimeCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spotify callback handling failed");
            var errParam = Uri.EscapeDataString(ex.Message ?? "Login failed");
            var sep = frontendUrl.Contains('?') ? "&" : "?";
            return Redirect($"{frontendUrl}{sep}error={errParam}");
        }
    }

    [HttpPost("exchange")]
    public IActionResult ExchangeCode([FromBody] ExchangeCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Code))
        {
            return BadRequest(new { message = "Authorization code is required" });
        }

        try
        {
            var response = _authService.ExchangeCode(request.Code);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeAccess()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        await _authService.RevokeTokenAsync(userId);
        return Ok(new { success = true, message = "Spotify token revoked successfully" });
    }
}
