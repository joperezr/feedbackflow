using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace FeedbackFunctions.Utils;

public static class AuthenticationHelper
{
    private const string AUTH_HEADER_NAME = "X-FeedbackFlow-Auth";
    
    public static bool ValidateAuthHeader(HttpRequestData req, IConfiguration configuration)
    {
        // Check if we're in mock mode (bypass auth)
        var useMocks = configuration.GetValue<bool>("UseMocks");
        if (useMocks)
        {
            return true;
        }
        
        // Get the expected password from configuration
        var expectedPassword = configuration["FeedbackApp:AccessPassword"] 
                              ?? Environment.GetEnvironmentVariable("FEEDBACKAPP_ACCESS_PASSWORD");
        
        if (string.IsNullOrEmpty(expectedPassword))
        {
            // If no password is configured, allow access (development scenario)
            return true;
        }
        
        // Check if the auth header is present and matches
        if (req.Headers.TryGetValues(AUTH_HEADER_NAME, out var headerValues))
        {
            var providedPassword = headerValues.FirstOrDefault();
            return !string.IsNullOrEmpty(providedPassword) && providedPassword == expectedPassword;
        }
        
        return false;
    }
    
    public static async Task<HttpResponseData> CreateUnauthorizedResponseAsync(HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        await response.WriteStringAsync("Unauthorized: Invalid or missing authentication header");
        return response;
    }
}
