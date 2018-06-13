using sizingservers.beholder.dnfapi.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace sizingservers.beholder.dnfapi.DA {
    public static class VMwareHostConnectionInfosDA {
        public static void AddOrUpdate(VMwareHostConnectionInfo row) {
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
            if (row.username == ".&DO_NOT_UPDATE_Credentials&.") {
                int fieldI = propNames.IndexOf("username");
                propNames.RemoveAt(fieldI);
                paramNames.RemoveAt(fieldI);
                parameters.RemoveAt(fieldI);

                fieldI = propNames.IndexOf("password");
                propNames.RemoveAt(fieldI);
                paramNames.RemoveAt(fieldI);
                parameters.RemoveAt(fieldI);
            }

            if (Get(row.ipOrHostname) == null) {
                SQLiteDataAccess.ExecuteSQL("Insert into VMwareHostConnectionInfos(" + string.Join(",", propNames) + ") values(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
            }
            else {
                var set = new List<string>();
                for (int i = 0; i != propNames.Count; i++)
                    set.Add(propNames[i] + "=" + paramNames[i]);


                string paramName = "@param" + (++paramI);
                parameters.Add(new SQLiteParameter(paramName, row.ipOrHostname));

                SQLiteDataAccess.ExecuteSQL("Update VMwareHostConnectionInfos set " + string.Join(",", set) + " where ipOrHostname=" + paramName, CommandType.Text, null, parameters.ToArray());
            }
        }

        public static void Remove(params SystemInformation[] rows) {
            var hostnames = new string[rows.Length];
            for (int i = 0; i != rows.Length; i++) hostnames[i] = rows[i].hostname;

            Remove(hostnames);
        }

        public static void Remove(params string[] ipsOrHostnames) {
            var paramNames = new List<string>();
            var parameters = new List<SQLiteParameter>();

            int paramI = 0;
            foreach (string ipOrHostname in ipsOrHostnames) {
                string paramName = "@param" + (++paramI);
                paramNames.Add(paramName);
                parameters.Add(new SQLiteParameter(paramName, ipOrHostname));
            }

            SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostConnectionInfos where ipOrHostname in(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
        }
        public static void Clear() {
            SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostConnectionInfos");
        }
        public static VMwareHostConnectionInfo[] GetAll() {
            var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostConnectionInfos");
            if (dt == null) return new VMwareHostConnectionInfo[0];

            var all = new VMwareHostConnectionInfo[dt.Rows.Count];
            for (int i = 0; i != all.Length; i++)
                all[i] = Parse(dt.Rows[i]);

            return all;
        }

        public static VMwareHostConnectionInfo Get(string ipOrHostname) {
            var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostConnectionInfos where ipOrHostname=@param1", CommandType.Text, null, new SQLiteParameter("@param1", ipOrHostname));
            if (dt.Rows.Count == 0) return null;

            return Parse(dt.Rows[0]);
        }

        private static VMwareHostConnectionInfo Parse(DataRow row) {
            var vmwinfo = new VMwareHostConnectionInfo();
            foreach (PropertyInfo propInfo in vmwinfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                var val = row[propInfo.Name];
                propInfo.SetValue(vmwinfo, (val is System.DBNull) ? "" : val);
            }

            return vmwinfo;
        }
    }
}