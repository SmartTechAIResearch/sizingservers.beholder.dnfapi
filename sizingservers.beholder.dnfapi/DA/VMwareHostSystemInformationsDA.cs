/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.dnfapi.Models;
using SizingServers.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace sizingservers.beholder.dnfapi.DA {
    public static class VMwareHostSystemInformationsDA {
        public static void AddOrUpdate(VMwareHostSystemInformation row) {
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

                if (Get(row.ipOrHostname) == null) {
                    SQLiteDataAccess.ExecuteSQL("Insert into VMwareHostSystemInformations(" + string.Join(",", propNames) + ") values(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
                }
                else {
                    var set = new List<string>();
                    for (int i = 0; i != propNames.Count; i++)
                        set.Add(propNames[i] + "=" + paramNames[i]);


                    string paramName = "@param" + (++paramI);
                    parameters.Add(new SQLiteParameter(paramName, row.ipOrHostname));

                    SQLiteDataAccess.ExecuteSQL("Update VMwareHostSystemInformations set " + string.Join(",", set) + " where ipOrHostname=" + paramName, CommandType.Text, null, parameters.ToArray());
                }
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed adding or updating vhost system info to the database", ex, new object[] { row });
                throw;
            }
        }

        public static void Remove(params VMwareHostSystemInformation[] rows) {
            try {
                var hostnames = new string[rows.Length];
                for (int i = 0; i != rows.Length; i++) hostnames[i] = rows[i].ipOrHostname;

                Remove(hostnames);
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed removing vhost system info", ex, new object[] { rows });
                throw;
            }
        }

        public static void Remove(params string[] ipOrHostnames) {
            try {
                var paramNames = new List<string>();
                var parameters = new List<SQLiteParameter>();

                int paramI = 0;
                foreach (string hostname in ipOrHostnames) {
                    string paramName = "@param" + (++paramI);
                    paramNames.Add(paramName);
                    parameters.Add(new SQLiteParameter(paramName, hostname));
                }

                SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostSystemInformations where ipOrHostname in(" + string.Join(",", paramNames) + ")", CommandType.Text, null, parameters.ToArray());
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed removing vhost system info", ex, new object[] { ipOrHostnames });
                throw;
            }
        }
        public static void Clear() {
            try {
                SQLiteDataAccess.ExecuteSQL("Delete from VMwareHostSystemInformations");
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed clearing vhost system info", ex);
                throw;
            }
        }
        public static VMwareHostSystemInformation[] GetAll() {
            try {
                var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostSystemInformations");
                if (dt == null) return new VMwareHostSystemInformation[0];

                var all = new VMwareHostSystemInformation[dt.Rows.Count];
                for (int i = 0; i != all.Length; i++)
                    all[i] = Parse(dt.Rows[i]);

                return all;
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed retrieving all vhost system info", ex);
                throw;
            }
        }

        public static VMwareHostSystemInformation Get(string hostname) {
            try {
                var dt = SQLiteDataAccess.GetDataTable("Select * from VMwareHostSystemInformations where ipOrHostname=@param1", CommandType.Text, null, new SQLiteParameter("@param1", hostname));
                if (dt.Rows.Count == 0) return null;

                return Parse(dt.Rows[0]);
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed retrieving vhost system info", ex, new object[] { hostname });
                throw;
            }
        }

        private static VMwareHostSystemInformation Parse(DataRow row) {
            var sysinfo = new VMwareHostSystemInformation();
            foreach (PropertyInfo propInfo in sysinfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                var val = row[propInfo.Name];

                if (val is DBNull) {
                    if (propInfo.PropertyType == typeof(string)) propInfo.SetValue(sysinfo, "");
                }
                else {
                    propInfo.SetValue(sysinfo, Convert.ChangeType(val, propInfo.PropertyType));
                }
            }

            return sysinfo;
        }
    }
}