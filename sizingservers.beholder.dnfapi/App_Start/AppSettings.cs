/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Web;

namespace sizingservers.beholder.dnfapi {
    public static class AppSettings {
        public static T GetValue<T>(string key) where T : struct, IConvertible {
            JObject jo = null;
            using (var sr = new StreamReader(Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", "appsettings.json"))) {
                jo = JObject.Parse(sr.ReadToEnd());
            }

            return (T)jo.GetValue(key, StringComparison.InvariantCultureIgnoreCase).ToObject(typeof(T));
        }
    }
}