using Dropbox.Api;
using VideoGallery.Library.Infrastructure;
using VideoGallery.Website.Auth;

namespace VideoGallery.Website.Shared;

public class WebDropboxClientFactory(IHttpContextAccessor contextAccesor, IUserTokenStore tokenStore) : IDropboxClientFactory
{
    public async Task<DropboxClient> Build(CancellationToken ct)
    {
        var context = contextAccesor.HttpContext ?? throw new Exception("HttpContext not accessible");
        var accessToken = await tokenStore.GetTokenAsync(context.User, ct);
        return new DropboxClient(accessToken.AccessToken);
    }
}