/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Web;

namespace sizingservers.beholder.dnfapi.DA {
    /// <summary>
    /// All gathered data is stored in a SQLite3 database.
    /// </summary>
    internal static class SQLiteDataAccess {
        /// <summary>
        /// Gets or sets the command timeout in seconds.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        [DefaultValue(30)]
        public static int CommandTimeout { get; set; }

        static SQLiteDataAccess() {
            CommandTimeout = 30;
        }

        public static void ExecuteSQL(string commandText, CommandType commandType = CommandType.Text, SQLiteTransaction transaction = null, params SQLiteParameter[] parameters) {
            using (var command = BuildCommand(commandText, commandType, transaction, parameters)) {
                command.ExecuteNonQuery();
            }

        }
        public static DataTable GetDataTable(string commandText, CommandType commandType = CommandType.Text, SQLiteTransaction transaction = null, params SQLiteParameter[] parameters) {
            using (var command = BuildCommand(commandText, commandType, transaction, parameters)) {
                var dataAdapter = new SQLiteDataAdapter();
                dataAdapter.SelectCommand = command;

                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                return dataSet.Tables[0];
            }
        }

        private static SQLiteCommand BuildCommand(string commandText, CommandType commandType, SQLiteTransaction transaction, SQLiteParameter[] parameters) {
            var command = new SQLiteCommand(commandText, GetConnection());
            if (transaction != null) command.Transaction = transaction;

            command.CommandType = commandType;
            command.Parameters.AddRange(parameters);
            command.CommandTimeout = CommandTimeout;

            return command;
        }
        private static SQLiteConnection GetConnection() {
            string appDataFolder = HttpContext.Current.Server.MapPath("~/App_Data/");

            var con = new SQLiteConnection("Data Source=" + Path.Combine(appDataFolder, "beholder.db") + ";Version=3;");
            con.Open();
            return con;

        }
    }
}