using System;

namespace SiteAnalyzer.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime _unixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeSeconds(this DateTime date)
        {
            return Convert.ToInt64((date - _unixEpoch).TotalSeconds);
        }

        public static long ToUnixTimeMilliseconds(this DateTime date)
        {
            return Convert.ToInt64((date - _unixEpoch).TotalMilliseconds);
        }

        public static long GetCurrentUnixTimestampMillis(this DateTime date)
        {
            return (long) (DateTime.UtcNow - _unixEpoch).TotalMilliseconds;
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long) (DateTime.UtcNow - _unixEpoch).TotalSeconds;
        }
    }
}