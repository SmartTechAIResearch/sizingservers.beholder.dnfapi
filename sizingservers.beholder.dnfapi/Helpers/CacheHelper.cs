/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using SizingServers.Log;
using System;
using System.Web;

namespace sizingservers.beholder.dnfapi.Helpers {
    /// <summary>
    /// A helper class for HttpContext.Current.Cache. Uses CacheExpiresInSeconds from appsettings.json.
    /// </summary>
    public static class CacheHelper {
        /// <summary>
        /// The default cache expires in seconds
        /// </summary>
        public const int DEFAULT_CACHE_EXPIRES_IN_SECONDS = 10;

        private static System.Web.Caching.Cache _cache = new System.Web.Caching.Cache();

        /// <summary>
        /// Gets the cache expires in seconds.
        /// </summary>
        /// <value>
        /// The cache expires in seconds.
        /// </value>
        public static int CacheExpiresInSeconds {
            get {
                int cacheExpiresInSeconds = DEFAULT_CACHE_EXPIRES_IN_SECONDS;
                try {
                    cacheExpiresInSeconds = AppSettings.GetValue<int>("CacheExpiresInSeconds");
                }
                catch {
                    Loggers.Log(Level.Warning, "CacheExpiresInSeconds not found in appsettings.json. Reverted to DEFAULT_CACHE_EXPIRES_IN_SECONDS (== 10).");
                }
                return cacheExpiresInSeconds;
            }
        }
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static dynamic Get<T>(string key) {
            try {
                var value = _cache.Get(key);
                if (value != null) {
                    return Convert.ChangeType(value, typeof(T));
                }
            }
            catch {
                //Not in cache.
            }
            return null;
        }
        /// <summary>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// </returns>
        public static bool Contains(string key) { return Get<object>(key) != null; }

        /// <summary>
        /// Only add reference / nullable types! otherwise Get and Contains won't work. Uses CacheExpiresInSeconds from appsettings.json.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void Add(string key, object value) {
            _cache.Add(key, value, null, DateTime.Now.AddSeconds(CacheExpiresInSeconds),
                System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
        }

        /// <summary>
        /// Adds a new object().
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Add(string key) { Add(key, new object()); }
    }
}