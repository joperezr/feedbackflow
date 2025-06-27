using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace FeedbackWebApp.Services.Authentication;

public class AuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var isAuthenticated = httpContext?.User?.Identity?.IsAuthenticated ?? false;
        return Task.FromResult(isAuthenticated);
    }

    public Task<string?> GetPasswordAsync()
    {
        // For API calls, we still need the password from configuration
        // Check if we're in mock mode (bypass auth)
        var useMocks = _configuration.GetValue<bool>("FeedbackApi:UseMocks");
        if (useMocks)
        {
            return Task.FromResult<string?>("mock");
        }

        // Get the expected password from configuration for API calls
        var expectedPassword = _configuration["FeedbackApp:AccessPassword"];
        return Task.FromResult<string?>(expectedPassword);
    }

    public async Task SetAuthenticatedAsync(bool authenticated, string? password = null)
    {
        // This method is now handled by the AuthController
        // We keep it for backward compatibility but it's a no-op
        await Task.CompletedTask;
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}