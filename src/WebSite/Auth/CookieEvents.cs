using Microsoft.AspNetCore.Authentication.Cookies;

namespace VideoGallery.Website.Auth;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserTokenStore _store;

    public CookieEvents(IUserTokenStore store)
    {
        _store = store;
    }
    
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await _store.GetTokenAsync(context.Principal!, CancellationToken.None);
        if (token.IsError)
        {
            context.RejectPrincipal();
        }

        await base.ValidatePrincipal(context);
    }
}