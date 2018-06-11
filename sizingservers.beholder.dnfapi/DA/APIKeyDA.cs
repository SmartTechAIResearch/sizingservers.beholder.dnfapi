/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.Data;
using System.Data.SQLite;

namespace sizingservers.beholder.dnfapi.DA {
    public static class APIKeyDA {
        public static bool HasKey(string key) {
            return SQLiteDataAccess.GetDataTable("Select key from APIKeys where key=@param1", CommandType.Text, null, new SQLiteParameter("@param1", key)).Rows.Count != 0;
        }
    }
}