using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace VideoGallery.Website.Auth;

public class ServerSideTokenStore : IUserTokenStore
{
    private readonly IMemoryCache _tokens;

    public ServerSideTokenStore(IMemoryCache memoryCache)
    {
        _tokens = memoryCache;
    }

    public Task<UserToken> GetTokenAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        return Task.FromResult(_tokens.Get<UserToken>(sub) ?? new UserToken { Error = "not found" });
    }
    
    public Task StoreTokenAsync(ClaimsPrincipal user, UserToken token)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        _tokens.Set(sub, token);
        
        return Task.CompletedTask;
    }
    
    public Task ClearTokenAsync(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        
        _tokens.Remove(sub);
        return Task.CompletedTask;
    }
}