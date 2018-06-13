/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.dnfapi.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace sizingservers.beholder.dnfapi.DA {
    public static class SystemInformationDA {
        public static void AddOrUpdate(SystemInformation row) {
            var propNames = new List<string>();
            var paramNames = new List<string>();
            var parameters = new List<SQLiteParameter>();

            int paramI = 0;
            foreach (PropertyInfo propInfo in row.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                propNames.Add(propInfo.Name);

                string paramName = "@param" + (++paramI);
                paramNames.Add(paramName);
                parameters.Add(new SQLiteParameter(paramName, propInfo.GetValue(row)));
            }

            if (Get(row.hostname) == null) {
                SQLiteDataAccess.ExecuteSQL("Insert into SystemInformations(" + string.Join(",", propNames) + ") values(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
            }
            else {
                var set = new List<string>();
                for (int i = 0; i != propNames.Count; i++)
                    set.Add(propNames[i] + "=" + paramNames[i]);


                string paramName = "@param" + (++paramI);
                parameters.Add(new SQLiteParameter(paramName, row.hostname));

                SQLiteDataAccess.ExecuteSQL("Update SytemInformations set " + string.Join(",", set) + " where hostname=" + paramName, CommandType.Text, null, parameters.ToArray());
            }
        }

        public static void Remove(params SystemInformation[] rows) {
            var hostnames = new string[rows.Length];
            for (int i = 0; i != rows.Length; i++) hostnames[i] = rows[i].hostname;

            Remove(hostnames);
        }

        public static void Remove(params string[] hostnames) {
            var paramNames = new List<string>();
            var parameters = new List<SQLiteParameter>();

            int paramI = 0;
            foreach (string hostname in hostnames) {
                string paramName = "@param" + (++paramI);
                paramNames.Add(paramName);
                parameters.Add(new SQLiteParameter(paramName, hostname));
            }

            SQLiteDataAccess.ExecuteSQL("Delete from SystemInformations where hostname in(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
        }
        public static void Clear() {
            SQLiteDataAccess.ExecuteSQL("Delete from SystemInformations");
        }
        public static SystemInformation[] GetAll() {
            var dt = SQLiteDataAccess.GetDataTable("Select * from SystemInformations");
            if (dt == null) return new SystemInformation[0];

            var all = new SystemInformation[dt.Rows.Count];
            for (int i = 0; i != all.Length; i++)
                all[i] = Parse(dt.Rows[i]);

            return all;
        }

        public static SystemInformation Get(string hostname) {
            var dt = SQLiteDataAccess.GetDataTable("Select * from SystemInformations where hostname=@param1", CommandType.Text, null, new SQLiteParameter("@param1", hostname));
            if (dt.Rows.Count == 0) return null;

            return Parse(dt.Rows[0]);
        }

        private static SystemInformation Parse(DataRow row) {
            var sysinfo = new SystemInformation();
            foreach (PropertyInfo propInfo in sysinfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                var val = row[propInfo.Name];
                if (!(val is System.DBNull)) propInfo.SetValue(sysinfo, val);
            }

            return sysinfo;
        }
    }
}