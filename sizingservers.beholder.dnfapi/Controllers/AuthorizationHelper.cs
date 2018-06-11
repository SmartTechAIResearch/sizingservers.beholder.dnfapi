/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

namespace sizingservers.beholder.dnfapi.Controllers {
    public static class AuthorizationHelper {
        /// <summary>
        /// Some sort of hack to get if authorization should be enabled or not (appsettings.json).
        /// </summary>
        public static bool Authorization { get; set; }

        public static bool Authorize(string apiKey) {
            if (!Authorization) return true;

            return DA.APIKeyDA.HasKey(apiKey);
        }
    }
}