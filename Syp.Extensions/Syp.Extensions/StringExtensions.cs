using System.Collections.Generic;
using System.Linq;

namespace Syp.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string ToString<T>(this IEnumerable<T> t, string seperator)
        {
            return string.Join(seperator, t.Select(tt => tt.ToString()));
        }
    }
}