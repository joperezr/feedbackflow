using Microsoft.JSInterop;

namespace FeedbackWebApp.Services.Authentication;

public class AuthenticationService
{
    private readonly IJSRuntime _jsRuntime;
    private const string AUTH_KEY = "feedbackflow_auth";
    private const string PASSWORD_KEY = "feedbackflow_password";
    private bool? _isAuthenticated;
    private string? _cachedPassword;

    public AuthenticationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (_isAuthenticated.HasValue)
            return _isAuthenticated.Value;

        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AUTH_KEY);
        _isAuthenticated = !string.IsNullOrEmpty(token);
        return _isAuthenticated.Value;
    }

    public async Task<string?> GetPasswordAsync()
    {
        if (_cachedPassword != null)
            return _cachedPassword;
            
        _cachedPassword = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PASSWORD_KEY);
        return _cachedPassword;
    }

    public async Task SetAuthenticatedAsync(bool authenticated, string? password = null)
    {
        if (authenticated)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AUTH_KEY, "authenticated");
            if (!string.IsNullOrEmpty(password))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PASSWORD_KEY, password);
                _cachedPassword = password;
            }
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AUTH_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", PASSWORD_KEY);
            _cachedPassword = null;
        }
        _isAuthenticated = authenticated;
    }

    public async Task LogoutAsync()
    {
        await SetAuthenticatedAsync(false);
    }
}