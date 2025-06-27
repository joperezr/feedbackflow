using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace FeedbackFunctions.Services;

public class FeedbackFlowApiKeyProvider : IApiKeyProvider
{
    private readonly IConfiguration _configuration;

    public FeedbackFlowApiKeyProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<IApiKey?> ProvideAsync(string key)
    {
        // Check if we're in mock mode (bypass auth)
        var useMocks = _configuration.GetValue<bool>("UseMocks");
        if (useMocks)
        {
            return Task.FromResult<IApiKey?>(new FeedbackFlowApiKey(key, "MockUser", new List<Claim>()));
        }

        // Get the expected password from configuration
        var expectedPassword = _configuration["FeedbackApp:AccessPassword"] 
                              ?? Environment.GetEnvironmentVariable("FEEDBACKAPP_ACCESS_PASSWORD");
        
        if (string.IsNullOrEmpty(expectedPassword))
        {
            // If no password is configured, allow access (development scenario)
            return Task.FromResult<IApiKey?>(new FeedbackFlowApiKey(key, "DevelopmentUser", new List<Claim>()));
        }

        // Validate the provided API key against the configured password
        if (key == expectedPassword)
        {
            return Task.FromResult<IApiKey?>(new FeedbackFlowApiKey(key, "AuthenticatedUser", new List<Claim>
            {
                new(ClaimTypes.Name, "AuthenticatedUser"),
                new(ClaimTypes.Role, "User")
            }));
        }

        // Invalid key
        return Task.FromResult<IApiKey?>(null);
    }
}
