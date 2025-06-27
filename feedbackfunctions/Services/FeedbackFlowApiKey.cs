using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;

namespace FeedbackFunctions.Services;

public class FeedbackFlowApiKey : IApiKey
{
    public FeedbackFlowApiKey(string key, string owner, IReadOnlyCollection<Claim> claims)
    {
        Key = key;
        OwnerName = owner;
        Claims = claims;
    }

    public string Key { get; }
    public string OwnerName { get; }
    public IReadOnlyCollection<Claim> Claims { get; }
}
