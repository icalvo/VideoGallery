namespace VideoGallery.Library;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public string StrJoin(string separator) =>
            string.Join(separator, source);

        public string StrJoin() =>
            string.Join("", source);
        public IEnumerable<T3> WithoutNullsStr<T2, T3>(Func<T, T2?> selector,
            Func<T, T2, T3> selector2) where T2 : struct
        {
            foreach (var item in source)
            {
                var x = selector(item);
                if (x != null)
                {
                    yield return selector2(item, x.Value);
                }
            }
        }

        public IEnumerable<T3> WithoutNullsCls<T2, T3>(Func<T, T2?> selector,
            Func<T, T2, T3> selector2) where T2 : class
        {
            foreach (var item in source)
            {
                var x = selector(item);
                if (x != null)
                {
                    yield return selector2(item, x);
                }
            }
        }
    }

    extension<T>(IEnumerable<T?> source) where T : struct
    {
        public IEnumerable<T> WithoutNullsStr() =>
            source.WithoutNullsStr(x => x, (_, y) => y);
    }
    extension<T>(IEnumerable<T?> source) where T : class
    {
        public IEnumerable<T> WithoutNullsCls() =>
            source.WithoutNullsCls(x => x, (_, y) => y);
    }
}