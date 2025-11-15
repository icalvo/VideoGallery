using System.Diagnostics;
using System.Net;
using Dropbox.Api;

namespace VideoGallery.Library.Infrastructure;

public class ConsoleDropboxClientFactory(string appKey) : IDropboxClientFactory
{
    private static readonly string TokenDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".videos");
    private static readonly string TokenFile = Path.Combine(TokenDirectory, "dropboxtoken.txt");

    public async Task<DropboxClient> Build(CancellationToken ct = default)
    {
        if (!File.Exists(TokenFile))
            return await GetByAuth();

        var existingRefreshToken = await File.ReadAllTextAsync(TokenFile, ct);
        var dropboxClient = new DropboxClient(existingRefreshToken, appKey);
        try
        {
            if (await dropboxClient.RefreshAccessToken(null))
            {
                return dropboxClient;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode >= HttpStatusCode.InternalServerError)
            {
                throw;
            }
        }

        return await GetByAuth(existingRefreshToken);

        async Task<DropboxClient> GetByAuth(string? existingRefreshToken = null)
        {
            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);

            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                oauthResponseType: OAuthResponseType.Code,
                clientId: appKey,
                redirectUri: (string?)null,
                state: null,
                tokenAccessType: TokenAccessType.Offline,
                scopeList: null,
                includeGrantedScopes: IncludeGrantedScopes.None,
                codeChallenge: codeChallenge);
            var code = GetAuthCodeAsync(authorizeUri);
            OAuth2Response tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                code: code,
                appKey: appKey,
                codeVerifier: codeVerifier,
                redirectUri: null);
            var client = new DropboxClient(
                appKey: appKey,
                oauth2AccessToken: tokenResult.AccessToken,
                oauth2RefreshToken: tokenResult.RefreshToken,
                oauth2AccessTokenExpiresAt: tokenResult.ExpiresAt ?? default(DateTime));
            if (existingRefreshToken != tokenResult.RefreshToken)
            {
                Directory.CreateDirectory(TokenDirectory);
                await File.WriteAllTextAsync(TokenFile, tokenResult.RefreshToken, ct);
            }

            return client;
        }
    }

    private static string? GetAuthCodeAsync(Uri authorizeUri)
    {
        Process.Start(new ProcessStartInfo { FileName = authorizeUri.ToString(), UseShellExecute = true });
        Console.Write("Dropbox auth code: ");
        var code = Console.ReadLine();
        return code;
    }
}