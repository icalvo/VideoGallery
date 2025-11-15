using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace VideoGallery.Website.Auth;
public class UserToken
{
    /// <summary>
    /// The access token
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// The access token type
    /// </summary>
    public string? AccessTokenType { get; set; }

    /// <summary>
    /// The string representation of the JSON web key to use for DPoP.
    /// </summary>
    public string? DPoPJsonWebKey { get; set; }
    
    /// <summary>
    /// The access token expiration
    /// </summary>
    public DateTimeOffset Expiration { get; set; }

    /// <summary>
    /// The scope of the access tokens
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Error (if any) during token request
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Checks for an error
    /// </summary>
    public bool IsError => !string.IsNullOrWhiteSpace(Error);    
    /// <summary>
    /// The refresh token
    /// </summary>
    public string? RefreshToken { get; set; }
}
public interface IUserTokenStore
{
    /// <summary>
    /// Stores tokens
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task StoreTokenAsync(ClaimsPrincipal user, UserToken token);

    /// <summary>
    /// Retrieves tokens from store
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <param name="ct"></param>
    /// <returns>access and refresh token and access token expiration</returns>
    Task<UserToken> GetTokenAsync(ClaimsPrincipal user, CancellationToken ct);

    /// <summary>
    /// Clears the stored tokens for a given user
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <returns></returns>
    Task ClearTokenAsync(ClaimsPrincipal user);
}

public static class AuthStartup
{
    public static void RegisterServices(WebApplicationBuilder builder, IConfiguration config)
    {
        var sc = builder.Services;
        sc.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.EventsType = typeof(CookieEvents);
            })
            .AddOpenIdConnect(options =>
            {
                var oidcConfig = config.GetSection("OpenIDConnectSettings");

                options.Authority = oidcConfig["Authority"];
                options.ClientId = oidcConfig["ClientId"];
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("files.metadata.read");
                options.Scope.Add("files.content.read");
                options.Scope.Add("sharing.read");
                options.Scope.Add("sharing.write");

                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                options.TokenValidationParameters.RoleClaimType = "roles";

                options.ProtocolValidator.RequireNonce = false;
                options.EventsType = typeof(OidcEvents);
            });
        
        var requireAuthPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(requireAuthPolicy);        
        sc.AddMemoryCache();
        sc.AddTransient<IUserTokenStore, ServerSideTokenStore>();
        sc.AddDistributedMemoryCache();
// register events to customize authentication handlers
        sc.AddTransient<CookieEvents>();
        sc.AddTransient<OidcEvents>();
    }
    
    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCookiePolicy();
    }
    
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet(
                "/account/login",
                (string? returnUrl, HttpContext ctx) =>
                {
                    StringValues url = ctx.Request.Query["url"];

                    string redirectUri = "/";

                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        if (MyUrlHelper.IsLocalUrl(url))
                        {
                            redirectUri = returnUrl;
                        }
                    }
            
                    var props = new AuthenticationProperties
                    {
                        RedirectUri = redirectUri
                    };
            
                    return Results.Challenge(props);
                })
            .AllowAnonymous();

        app.MapGet(
                "/accounts/logout",
                () => Results.SignOut(null, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]))
            .AllowAnonymous();
    }
}