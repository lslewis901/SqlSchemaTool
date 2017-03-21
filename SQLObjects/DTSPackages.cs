using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Lewis.SST.SQLMethods;

namespace Lewis.SST.SQLObjects
{
    /// <summary>
    /// Class to list DTS packages residing on SQL server.
    /// Code for this was borrowed from: http://www.extremeexperts.com/SQL/FAQ/ListDTSPackagesVBNET.aspx 
    /// </summary>
    [Serializable()]
    public class DTSPackages : BaseObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DTSPackages"/> class.
        /// </summary>
        public DTSPackages() : base () { }

        		/// <summary>
		/// Main Entry point of the Funcs object which inherits from a DataSet class
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
        public DTSPackages(string DBName) : base(DBName) { }

        /// <summary>
        /// GetDTSPackages
        /// </summary>
        /// <typeparam name="DTSPackages"></typeparam>
        /// <param name="_connection"></param>
        /// <param name="args"></param>
        public override void GetObject<DTSPackages>(SqlConnection _connection, params object[] args)
        {
            if (typeof(bool).IsInstanceOfType(args[0]))
            {
                GetDTSPackages(_connection, (bool)args[0]);
            }
            else
            {
                string SQLServerName = (string)args[0];
                bool IntegratedSecurity = (bool)args[1];
                string User = (string)args[2];
                string Password = (string)args[3];
                bool MostRecentVersion = (bool)args[4];
                GetDTSPackages(SQLServerName, IntegratedSecurity, User, Password, MostRecentVersion);
            }
        }

        /// <summary>
        /// Populates this object with a dataset containing the list of DTS packages on the selected SQL server.
        /// </summary>
        /// <param name="_connection">SQL connection object.</param>
        /// <param name="MostRecentVersion">if set to <c>true</c> [most recent version].</param>
        private void GetDTSPackages(SqlConnection _connection, bool MostRecentVersion)
        {
            try
            {
			    string cmd_GetDTSPackagesList = string.Format(SqlQueryStrings.GetDTSPackagesList, _connection.Database);
                if (!MostRecentVersion)
                {
                    cmd_GetDTSPackagesList += " @flags=0x04";
                }

                using (SqlCommand _command_DTSPackages = new SqlCommand(cmd_GetDTSPackagesList, _connection))
                {
                    _command_DTSPackages.Prepare();
                    using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_DTSPackages))
                    {
                        _sdaTables.Fill(this);
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.Error("\nGetDTSPackages failed on {0}, cause: {1}", ex.Message, ex.Procedure);
            }
        }

        /// <summary>
        /// Gets the DTS packages list and pushes it into a dataset.
        /// </summary>
        /// <param name="SQLServerName">Name of the SQL server.</param>
        /// <param name="IntegratedSecurity">if set to <c>true</c> [integrated security].</param>
        /// <param name="User">The user.</param>
        /// <param name="Password">The password.</param>
        /// <param name="MostRecentVersion">if set to <c>true</c> [most recent version].</param>
        /// <returns>returns a dataset containing the list of DTS packages on the selected SQL server.</returns>
        private void GetDTSPackages(string SQLServerName, bool IntegratedSecurity, string User, string Password, bool MostRecentVersion)
        {
            try
            {
                string connectstr = string.Format("Server={0};Database=MSDB;Integrated Security={1};User ID={2};Password={3}", SQLServerName, IntegratedSecurity.ToString(), User, Password);
                string commandstr = "EXEC sp_enum_dtspackages"; //requires that the selected catalog be MSDB
                if (!MostRecentVersion)
                {
                    commandstr += " @flags=0x04";
                }
                using (SqlConnection conn = new SqlConnection(connectstr))
                {
                    using (SqlCommand comm = new SqlCommand(commandstr, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(comm))
                        {
                            da.Fill(this);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.Error("\nGetDTSPackages failed on {0}, cause: {1}", ex.Message, ex.Procedure);
            }
        }

    }
}
