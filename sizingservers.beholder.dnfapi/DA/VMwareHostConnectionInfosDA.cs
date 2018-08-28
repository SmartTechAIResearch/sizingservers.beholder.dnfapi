using sizingservers.beholder.dnfapi.Models;
using SizingServers.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace sizingservers.beholder.dnfapi.DA {
    public static class VMwareHostConnectionInfosDA {
        /// <summary>
        /// Adds the or update.
        /// </summary>
        /// <param name="row">The row.</param>
        public static void AddOrUpdate(VMwareHostConnectionInfo row) {
            try {
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
#warning Handle this better
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

                if (Get(row.hostname) == null) {
                    SQLiteDataAccess.ExecuteSQL("Insert into VMwareHostConnectionInfos(" + string.Join(",", propNames) + ") values(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
                }
                else {
                    var set = new List<string>();
                    for (int i = 0; i != propNames.Count; i++)
                        set.Add(propNames[i] + "=" + paramNames[i]);


                    string paramName = "@param" + (++paramI);
                    parameters.Add(new SQLiteParameter(paramName, row.hostname));

                    SQLiteDataAccess.ExecuteSQL("Update VMwareHostConnectionInfos set " + string.Join(",", set) + " where hostname=" + paramName, CommandType.Text, null, parameters.ToArray());
                }
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed adding or updating vhost connection info", ex, new object[] { row });
                throw;
            }
        }

        public static void ChangePKValue(VMwareHostConnectionInfo row, string newHostname) {
            try {
                SQLiteDataAccess.ExecuteSQL("Update VMwareHostConnectionInfos set hostname=@param1 where hostname=@param2", CommandType.Text, null, new SQLiteParameter[] { new SQLiteParameter("@param1", newHostname), new SQLiteParameter("@param2", row.hostname) });
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed changing the pk value in vhost connection info", ex, new object[] { newHostname });
                throw;
            }
        }

        public static void Remove(params SystemInformation[] rows) {
            try {
                var hostnames = new string[rows.Length];
                for (int i = 0; i != rows.Length; i++) hostnames[i] = rows[i].hostname;

                Remove(hostnames);
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed removing vhost connection info", ex, new object[] { rows });
                throw;
            }
        }

        public static void Remove(params string[] ipsOrHostnames) {
            try {
                var paramNames = new List<string>();
                var parameters = new List<SQLiteParameter>();

                int paramI = 0;
                foreach (string hostname in ipsOrHostnames) {
                    string paramName = "@param" + (++paramI);
                    paramNames.Add(paramName);
                    parameters.Add(new SQLiteParameter(paramName, hostname));
                }

                SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostConnectionInfos where hostname in(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed removing vhost connection info", ex, new object[] { ipsOrHostnames });
                throw;
            }
        }
        public static void Clear() {
            try {
                SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostConnectionInfos");
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed clearing vhost connection info", ex);
                throw;
            }
        }
        public static VMwareHostConnectionInfo[] GetAll() {
            try {
                var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostConnectionInfos");
                if (dt == null) return new VMwareHostConnectionInfo[0];

                var all = new VMwareHostConnectionInfo[dt.Rows.Count];
                for (int i = 0; i != all.Length; i++)
                    all[i] = Parse(dt.Rows[i]);

                return all;
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed retrieving all vhost connection info", ex);
                throw;
            }
        }

        public static VMwareHostConnectionInfo Get(string hostname) {
            try {
                var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostConnectionInfos where hostname=@param1", CommandType.Text, null, new SQLiteParameter("@param1", hostname));
                if (dt.Rows.Count == 0) return null;

                return Parse(dt.Rows[0]);
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed retrieving vhost connection info", ex);
                throw;
            }
        }

        private static VMwareHostConnectionInfo Parse(DataRow row) {
            var vmwinfo = new VMwareHostConnectionInfo();
            foreach (PropertyInfo propInfo in vmwinfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                var val = row[propInfo.Name];

                if (val is DBNull) {
                    if (propInfo.PropertyType == typeof(string)) propInfo.SetValue(vmwinfo, "");
                }
                else {
                    propInfo.SetValue(vmwinfo, Convert.ChangeType(val, propInfo.PropertyType));
                }
            }

            return vmwinfo;
        }
    }
}