using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KonbiCloud
{
    public static class Extensions
    {
        /// <summary>
        /// Cranks out a collision resistant hash, relatively quickly.
        /// 
        /// Not suitable for passwords, or sensitive information.
        /// </summary>
        public static string WeakHash(this string value)
        {
            var hasher = SHA1.Create();

            var bytes = !string.IsNullOrEmpty(value) ? Encoding.UTF8.GetBytes(value) : new byte[0];

            return Convert.ToBase64String(hasher.ComputeHash(bytes));
        }
        internal static IList<T> ToIList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }

        public static IList<T> Remove<T>(this IList<T> collection, Func<T, bool> predicate)
        {
            var item = collection.FirstOrDefault(predicate);
            if (item != null)
            {
                collection.Remove(item);
            }

            return collection;
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }
    }
}
