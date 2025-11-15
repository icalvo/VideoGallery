using System.Collections.Specialized;
using System.Web;

namespace VideoGallery.Website.VideoGrid;

public static class QueryHelpers
{
    extension(string url)
    {
        public string ModifyUri(Action<UriBuilder> modify) => 
            ModifyUri(new Uri(url, UriKind.RelativeOrAbsolute), modify);

        public string ModifyQueryString(Action<NameValueCollection> modify) => 
            ModifyQueryString(new Uri(url, UriKind.RelativeOrAbsolute), modify);
    }

    public static string ModifyUri(this Uri uriTest, Action<UriBuilder> modify)
    {                   
        var uri = uriTest.IsAbsoluteUri ? uriTest : new Uri(new Uri("https://example.com"), uriTest);
        // this gets all the query string key value pairs as a collection

        var ub = new UriBuilder(uri);
        if (!uriTest.IsAbsoluteUri)
        {
            ub.Host = null;
            ub.Scheme = null;
        }

        modify(ub);
        return ub.ToString();
    }

    public static string ModifyQueryString(this Uri url, Action<NameValueCollection> modify)
    {
        return ModifyUri(
            url,
            ub =>
            {
                var newQueryString = HttpUtility.ParseQueryString(ub.Query);
                modify(newQueryString);
                if (newQueryString.Count > 0)
                    ub.Query = $"?{newQueryString}";
            });
    }
}