namespace SmartLMS.Web.Extensions
{
    public static class RouteDataExtensions
    {
        public static IDictionary<string, string> ToRouteData(this IQueryCollection query)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in query)
            {
                if (string.Equals(pair.Key, "page", StringComparison.OrdinalIgnoreCase))
                    continue;

                dict[pair.Key] = pair.Value.ToString();
            }
            return dict;
        }
    }
}
