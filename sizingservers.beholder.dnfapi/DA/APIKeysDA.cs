/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using SizingServers.Log;
using System;
using System.Data;
using System.Data.SQLite;

namespace sizingservers.beholder.dnfapi.DA {
    public static class APIKeysDA {
        public static bool HasKey(string key) {
            try {
                return SQLiteDataAccess.GetDataTable("Select key from APIKeys where key=@param1", CommandType.Text, null, new SQLiteParameter("@param1", key)).Rows.Count != 0;
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed getting API key from database", ex, new object[] { key });
                throw;
            }
        }
    }
}