using SpoMusic.Api.DTOs;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Services.Auth;

public record StateValidationResult(bool IsValid, string? ReturnUrl, string? RedirectUri);

public interface IAuthService
{
    string GetSpotifyAuthUrl(string? platform = null, string? returnUrl = null, string? redirectUri = null);
    StateValidationResult ValidateState(string state);
    string CreateOneTimeCode(string token, UserProfileDto user);
    AuthResponse ExchangeCode(string code);
    Task<AuthResponse> HandleSpotifyCallbackAsync(string code, string? redirectUri = null);
    Task<UserProfileDto?> GetUserByIdAsync(string userId);
    Task RevokeTokenAsync(string userId);
}
