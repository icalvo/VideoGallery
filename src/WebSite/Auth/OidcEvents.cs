using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace VideoGallery.Website.Auth;

public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserTokenStore _store;

    public OidcEvents(IUserTokenStore store)
    {
        _store = store;
    }


    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await _store.StoreTokenAsync(context.Principal!, new UserToken
        {
            AccessToken = context.TokenEndpointResponse.AccessToken,
            AccessTokenType = context.TokenEndpointResponse.TokenType,
            Expiration = exp,
            RefreshToken = context.TokenEndpointResponse.RefreshToken,
            Scope = context.TokenEndpointResponse.Scope
        });
        
        await base.TokenValidated(context);
    }

    // public override Task TicketReceived(TicketReceivedContext context)
    // {
    //     var url = context.Principal?.FindFirst("myurl")?.Value;
    //     context.ReturnUri = url;
    //     return base.TicketReceived(context);
    // }
}