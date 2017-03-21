using Lewis.SST.SQLObjects;

using Microsoft.XmlDiffPatch;

using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Xml.XPath;

#region change history
/// 08-22-2008: C01: LLEWIS: changes to close and dispose of SQL objects
/// 03-09-2009: C02: LLEWIS: changes to handle errors if data reader is null in 
///                          GetData method and GetDBTableNames method
#endregion

namespace Lewis.SST.SQLMethods
{
    // TODO add methods to make data into xml files for compare using xmldiff
    /// <summary>
    /// Methods to get SQL data for copy and serialization
    /// </summary>
    public static class SQLData
    {
        private const string XmlDiffHeader =
            @"<html><head>
                <style TYPE='text/css' MEDIA='screen'>
                <!-- td { font-family: Courier New; font-size:14; } 
                th { font-family: Arial; } 
                p { font-family: Arial; } 
                .match { }
                .ignore { color:#AAAAAA; }
                .add { background-color:yellow; }
                .moveto { background-color:cyan; color:navy; }
                .remove { background-color:red; }
                .movefrom {  background-color:cyan; color:navy; }
                .change {  background-color:lightgreen;  }
                -->
            </style></head>
            <body>
                <table border='0' width='100%'>
                    <tr><td><table border='0' width='100%' style='table-layout:fixed;'>
                    <COL WIDTH='40'><COL WIDTH='50%'><COL WIDTH='50%'>
                    <tr><td colspan='3' align='center'>
                    <b>Legend:</b> <span class='add'>added</span>
                        <span class='remove'>removed</span>
                        <span class='change'>changed</span>
                        <span class='movefrom'>moved from</span>
                        <span class='moveto'>moved to</span>
                        <span class='ignore'>ignored</span><br/><br/>
                    </td></tr>";

        private const string XmlDiffBody =
            @"<tr><td></td><td title='{0}'><b> Source File: {1}</b></td><td title='{2}'><b> Target File: {3}</b></td></tr>";

        private static Logger logger = LogManager.GetLogger("Lewis.SST.SQLMethods.SQLData");

        /// <summary>
        /// gets an array of the table names for a given database
        /// </summary>
        /// <param name="DBName">data base name to get list of tables from</param>
        /// <param name="connection">Sql Connection object attached to database</param>
        /// <returns></returns>
        public static ArrayList GetDBTableNames(string DBName, SqlConnection connection)
        {
            ArrayList retval = new ArrayList();
            using (SqlCommand command = new SqlCommand())
            {
                SqlDataReader reader = null;
                try
                {
                    string commandQuery = string.Format(Lewis.SST.SQLObjects.SqlQueryStrings.GetTablesList, DBName, string.Empty); // changed from GetDataBaseObjects which was filtering based only on name
                    // opens connection. 
                    command.CommandType = CommandType.Text;
                    command.CommandText = commandQuery;
                    command.Connection = connection;
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    command.Prepare();
                    // executes the query.
                    reader = command.ExecuteReader();
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            retval.Add(reader.GetSqlValue(0));
                        }
                        // closes connection.
                        reader.Close();
                        connection.Close();
                        reader.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                    if (reader != null) 
                    { 
                        reader.Dispose(); 
                    }
                    throw ex;
                }
            }

            return retval;
        }

        /// <summary>
        /// compares two xml data sources as xml strings
        /// </summary>
        /// <param name="querySource">the SQL query text for the source</param>
        /// <param name="queryTarget">the SQL query text for the target</param>
        /// <param name="connectionSource">The sql connection object for the source</param>
        /// <param name="connectionTarget">The sql connection object for the target</param>
        /// <param name="asTextFile"></param>
        /// <returns></returns>
        public static string CompareData(string querySource, string queryTarget, SqlConnection connectionSource, SqlConnection connectionTarget, bool asTextFile)
        {
            bool isEqual = false;
            string tempFile = "TableDiffReport.html";
            string sourceName = querySource.Replace("select * from ", "").Split(' ')[0].Replace("\\", "_").Replace(":", "-") + ".xml";
            string targetName = queryTarget.Replace("select * from ", "").Split(' ')[0].Replace("\\", "_").Replace(":", "-") + ".xml";
            //output diff file.
            string diffFile = sourceName.Replace(".xml", "") + "_DIFF_" + targetName;
            XmlDiffOptions xdo = XmlDiffOptions.IgnoreWhitespace | XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePI;

            XmlDocument original = new XmlDocument();
            original.LoadXml(GetXMLData(querySource, connectionSource));
            original.Save(sourceName);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GetXMLData(queryTarget, connectionTarget));
            doc.Save(targetName);

            if (asTextFile)
            {
                XmlDiffView diffView = new XmlDiffView();
                diffView.DifferencesAsFormattedText(sourceName, targetName, diffFile.Replace(".xml", "") + ".txt", false, xdo);
                diffView = null;
                return diffFile.Replace(".xml", "") + ".txt";
            }
            else
            {

                XmlTextWriter diffWriter = new XmlTextWriter(diffFile, Encoding.UTF8);
                diffWriter.Formatting = Formatting.Indented;
                using (diffWriter)
                {
                    XmlDiff diff = new XmlDiff();
                    isEqual = diff.Compare(original, doc, diffWriter);
                    diff.Options = xdo;
                }

                if (isEqual)
                {
                    //This means the files were identical for given options.
                    MessageBox.Show("Tables are identical", "Identical",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return string.Empty;
                }

                using (XmlReader diffGram = XmlReader.Create(diffFile))
                {
                    XmlDiffView diffView = new XmlDiffView();
                    diffView.Load(new XmlNodeReader(original), diffGram);
                    using (TextWriter htmlWriter = new StreamWriter(tempFile))
                    {
                        SideBySideXmlNotepadHeader(sourceName, targetName, htmlWriter);
                        diffView.GetHtml(htmlWriter);
                    }
                    diffView = null;
                }
            }
            return tempFile;
        }

        /// <summary>
        /// method to convert datatable to xml string
        /// </summary>
        /// <param name="query">the SQL query text</param>
        /// <param name="connection">The sql connection object</param>
        /// <returns>an xml result string</returns>
        public static string GetXMLData(string query, SqlConnection connection)
        {
            DataTable dt = GetData(query, connection);
            int fromIdx = query.ToLower().IndexOf("from ");
            string tableName = query.Substring(fromIdx).ToLower().Replace("from ", "").Replace("* ", "").Replace(" ", "_").Replace("[", "").Replace("]", "").Replace("\\", "_").Trim();
            tableName = tableName.Substring(tableName.LastIndexOf('.') + 1);
            dt.TableName = tableName;
            // maybe something with this would work dt.Columns[0].DefaultValue
            StringWriter sw = new StringWriter();
            dt.WriteXml(sw, XmlWriteMode.IgnoreSchema);
            return sw.ToString();
        }

        /// <summary>
        /// gets the data from a SQL query and puts it into a DataTable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DataTable GetData(string query, SqlConnection connection)
        {
            DataTable dataTable = null;
            using (SqlCommand command = new SqlCommand())
            {
                SqlDataReader reader = null;
                try
                {
                    // opens connection. 
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Connection = connection;
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    command.Prepare();
                    
                    // executes the query.
                    reader = command.ExecuteReader();
                    if (reader != null)
                    {
                        dataTable = ConstructData(reader);

                        // closes connection.
                        reader.Close();
                        connection.Close();
                        reader.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("{0} caused an error: {1}", query, ex.Message);
                    logger.Error(SQLSchemaTool.ERRORFORMAT, msg, ex.Source, ex.StackTrace);
                    if (reader != null) 
                    { 
                        reader.Dispose(); 
                    }
                    throw ex;
                }
                return dataTable;
            }
        }

        /// <summary>
        /// The html header used by XmlNotepad.
        /// </summary>
        /// <param name="sourceXmlFile">name of baseline xml data</param>
        /// <param name="changedXmlFile">name of file being compared</param>
        /// <param name="resultHtml">Output file</param>
        private static void SideBySideXmlNotepadHeader(
            string sourceXmlFile,
            string changedXmlFile,
            TextWriter resultHtml)
        {
            // this initializes the html
            resultHtml.WriteLine(XmlDiffHeader);
            resultHtml.WriteLine(string.Format(XmlDiffBody,
                    System.IO.Path.GetDirectoryName(sourceXmlFile),
                    System.IO.Path.GetFileName(sourceXmlFile),
                    System.IO.Path.GetDirectoryName(changedXmlFile),
                    System.IO.Path.GetFileName(changedXmlFile)
            ));
        }

        /// <summary>
        /// Constructs the data which was extracted
        /// from the database according to user's query.
        /// </summary>
        /// <param name="reader">SqlReader - holds the queried data.</param>
        ///<returns>Queried data in DataTable.</returns>
        private static DataTable ConstructData(SqlDataReader reader)
        {
            try
            {
                if (reader.IsClosed)
                    throw new
                      InvalidOperationException("Attempt to" +
                               " use a closed SqlDataReader");

                DataTable dataTable = new DataTable();

                // constructs the columns data.
                for (int i = 0; i < reader.FieldCount; i++)
                    dataTable.Columns.Add(reader.GetName(i),
                                    reader.GetFieldType(i));

                // constructs the table's data.
                while (reader.Read())
                {
                    object[] row = new object[reader.FieldCount];
                    reader.GetValues(row);
                    dataTable.Rows.Add(row);
                }
                // Culture info.
                // TODO: get locale from app
                dataTable.Locale = new System.Globalization.CultureInfo("en-US");
                // Accepts changes.
                dataTable.AcceptChanges();

                return dataTable;
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// This method will copy the data in a table 
        /// from one database to another. The
        /// source and destination can be from any type of 
        /// .NET database provider.
        /// </summary>
        /// <param name="source">Source database connection</param>
        /// <param name="destination">Destination database connection</param>
        /// <param name="sourceSQL">Source SQL statement</param>
        /// <param name="destinationTableName">Destination table name</param>
        public static void CopyTable(IDbConnection source,
            IDbConnection destination, String sourceSQL, String destinationTableName)
        {
            // C01: LLEWIS:  changes to close and dispose of SQL objects
            logger.Info(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + " " + destinationTableName + " load started");
            IDbCommand cmd = source.CreateCommand();
            cmd.CommandText = sourceSQL;
            logger.Debug("\tSource SQL: " + sourceSQL);
            try
            {
                source.Open();
                destination.Open();
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    DataTable schemaTable = rdr.GetSchemaTable();

                    using (IDbCommand insertCmd = destination.CreateCommand())
                    {
                        string paramsSQL = String.Empty;

                        //build the insert statement
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            if (paramsSQL.Length > 0)
                                paramsSQL += ", ";
                            paramsSQL += "@" + row["ColumnName"].ToString();

                            IDbDataParameter param = insertCmd.CreateParameter();
                            param.ParameterName = "@" + row["ColumnName"].ToString();
                            param.SourceColumn = row["ColumnName"].ToString();

                            if (row["DataType"] == typeof(System.DateTime))
                            {
                                param.DbType = DbType.DateTime;
                            }

                            logger.Debug(param.SourceColumn);
                            insertCmd.Parameters.Add(param);
                        }
                        insertCmd.CommandText = String.Format("insert into {0} ( {1} ) values ( {2} )", destinationTableName, paramsSQL.Replace("@", String.Empty), paramsSQL);
                        int counter = 0;
                        int errors = 0;
                        while (rdr.Read())
                        {
                            try
                            {
                                foreach (IDbDataParameter param in insertCmd.Parameters)
                                {
                                    object col = rdr[param.SourceColumn];

                                    //special check for SQL Server and 
                                    //datetimes less than 1753
                                    if (param.DbType == DbType.DateTime)
                                    {
                                        if (col != DBNull.Value)
                                        {
                                            //sql server can not have dates less than 1753
                                            if (((DateTime)col).Year < 1753)
                                            {
                                                param.Value = DBNull.Value;
                                                continue;
                                            }
                                        }
                                    }

                                    param.Value = col;

                                    //values being used for the insert
                                    logger.Debug(param.SourceColumn + " --> " + param.ParameterName + " = " + col.ToString());
                                }
                                insertCmd.ExecuteNonQuery();
                                logger.Debug(++counter);
                            }
                            catch (Exception ex)
                            {
                                if (errors == 0)
                                {
                                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                                }
                                errors++;
                            }
                        }
                        logger.Error(errors + " errors");
                        logger.Debug(counter + " records copied");
                        logger.Info(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + " " + destinationTableName + " load completed");
                    } // dispose of SqlCommand: insertCmd
                } // dispose of DataReader: rdr
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }
            finally
            {
                cmd.Dispose();
                destination.Close();
                source.Close();
                destination.Dispose();
                source.Dispose();
            }
        }
    }
}
