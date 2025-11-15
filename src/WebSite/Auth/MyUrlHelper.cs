using System.Diagnostics.CodeAnalysis;

namespace VideoGallery.Website.Auth;

public static class MyUrlHelper
{
    internal static bool IsLocalUrl([NotNullWhen(true)] string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Allows "/" or "/foo" but not "//" or "/\".
        if (url[0] == '/')
        {
            // url is exactly "/"
            if (url.Length == 1)
            {
                return true;
            }

            // url doesn't start with "//" or "/\"
            if (url[1] != '/' && url[1] != '\\')
            {
                return !HasControlCharacter(url.AsSpan(1));
            }

            return false;
        }

        // Allows "~/" or "~/foo" but not "~//" or "~/\".
        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            // url is exactly "~/"
            if (url.Length == 2)
            {
                return true;
            }

            // url doesn't start with "~//" or "~/\"
            if (url[2] != '/' && url[2] != '\\')
            {
                return !HasControlCharacter(url.AsSpan(2));
            }

            return false;
        }

        return false;

        static bool HasControlCharacter(ReadOnlySpan<char> readOnlySpan)
        {
            // URLs may not contain ASCII control characters.
            foreach (var t in readOnlySpan)
            {
                if (char.IsControl(t))
                {
                    return true;
                }
            }

            return false;
        }
    }
}