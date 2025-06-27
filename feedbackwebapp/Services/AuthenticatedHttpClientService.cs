using FeedbackWebApp.Services.Authentication;

namespace FeedbackWebApp.Services;

public class AuthenticatedHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationService _authService;
    private const string AUTH_HEADER_NAME = "X-FeedbackFlow-Auth";

    public AuthenticatedHttpClientService(HttpClient httpClient, AuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var password = await _authService.GetPasswordAsync();
        if (!string.IsNullOrEmpty(password))
        {
            // Remove existing auth header if present
            _httpClient.DefaultRequestHeaders.Remove(AUTH_HEADER_NAME);
            // Add the password as a custom header
            _httpClient.DefaultRequestHeaders.Add(AUTH_HEADER_NAME, password);
        }
        
        return _httpClient;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        var client = await GetAuthenticatedClientAsync();
        return await client.GetAsync(requestUri);
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
    {
        var client = await GetAuthenticatedClientAsync();
        return await client.PostAsync(requestUri, content);
    }
}
