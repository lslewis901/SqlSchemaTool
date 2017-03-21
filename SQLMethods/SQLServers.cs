using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

#region change history
/// 08-22-2008: C01: LLEWIS: added sorted return for database/catalog names
#endregion

namespace Lewis.SST.SQLMethods
{
    /// <summary>
    /// Class to enumerate SQL servers using the SqlDataSourceEnumerator.
    /// </summary>
    public static class SQLServers
    {
        private const string getDB_Names = "SELECT DISTINCT [name] FROM [master].[dbo].sysdatabases ORDER BY [NAME]";

        /// <summary>
        /// Gets the array of SQL servers that are available on the LAN.
        /// </summary>
        /// <returns>string[] of SQL server names</returns>
        public static string[] GetSQLServers()
        {
            string[] retval = null;
            DataTable dt = null;

            System.Data.Sql.SqlDataSourceEnumerator sds = System.Data.Sql.SqlDataSourceEnumerator.Instance;
            dt = sds.GetDataSources();
            ArrayList al = new ArrayList();
            if (retval != null)
            {
                for (int zz = 0; zz < retval.Length; zz++)
                {
                    if (!al.Contains(retval[zz]))
                    {
                        al.Add(retval[zz]);
                    }
                }
            }
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["ServerName"].ToString() + (row["InstanceName"].ToString().Trim().Length != 0 ? "\\" + row["InstanceName"].ToString() : "");
                    if (!al.Contains(name))
                    {
                        al.Add(name);
                    }
                }
                dt.Dispose();
            }
            al.Sort();
            return (string[])al.ToArray(typeof(string));
        }

        /// <summary>
        /// method returns all dbnames defined on the SQL server, specified by the SQL connection object
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <returns></returns>
        public static string[] GetDBNames(SqlConnection sqlConn)
        {
            ArrayList arl = new ArrayList();
            using (SqlCommand sqlCommand = new SqlCommand(getDB_Names, sqlConn))
            {
                if (sqlConn.State != ConnectionState.Open)
                {
                    sqlConn.Open();
                }
                using (SqlDataReader sdr = sqlCommand.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        System.Data.SqlTypes.SqlString dbName = sdr.GetSqlString(0);
                        arl.Add(dbName.ToString());
                    }
                    sdr.Close();
                }
            }
            // C01: LLEWIS: added sorted return
            arl.Sort();
            return (string[])arl.ToArray(typeof(string));
        }
    }
}
