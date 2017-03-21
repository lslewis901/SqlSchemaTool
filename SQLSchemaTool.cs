using Lewis.Xml;

using Microsoft.XmlDiffPatch;
using NLog;

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace Lewis.SST
{
    /// <summary>The SQL Schema Tool is a commandline executable application for generating XML schema documents from SQL server databases, 
    /// comparing those databases or the XML schema documents, and finally generating a DiffGram XML document from the compare results. 
    /// <para>
    /// The tool limitations currently are that it does not support "WITH NOCHECK" or extended properties.  Additionally, the tool only 
    /// supports adding calculated columns using CREATE TABLE or ALTER TABLE ADD 'column name' SQL statements.  Also, calculated columns
    /// are added last in order, so that the columns they may be dependant on will already be in place.  Column Default Constraints are 
    /// dropped using sp_unbindefault and they are not re-added later in the diffgram SQL script script using sp_bindefault. 
    /// </para>
    /// <para>
    /// Instead the tool assigns the default value to the column using the ALTER TABLE ADD DEFAULT 'default value' FOR column_name 
    /// method.  Doing the diffgram schema this way does cause a DB updated with the diffgram to not compare exactly using the Redgate 
    /// SQL Compare tool. However, the equivalent DB functionality is still there.
    /// </para>
    /// <para>
    /// The order of columns will also be different between the source and destination DBs as the calculated columns are added to the 
    /// table last.  This by design so as to avoid dependency issues with other columns that the calculation may be dependant on. 
    /// Speaking of dependant objects, especially stored procedures, these are output in the order indicated in the sysdepends table.
    /// If the sysdepends table gets out of sync with the database, then the stored procedures will likely also be out of sync.
    /// </para>
    /// <para>
    /// This tool uses embedded XSLT 'style sheets' to transform the generated XML docs into the SQL script files.  Also the code was 
    /// changed to use XPathDocuments as suppliers of the XML to the XMLTransformation object.  The performance increase is significant 
    /// over using the XmlDataDocument object.
    /// </para>
    /// <para>
    /// Check out the following URL for great tips on error handling in SQL: 
    /// <see href="http://www.sommarskog.se/error-handling-II.html#whyerrorcheck">SQL Error Handling</see>
    /// 
    /// </para>
    /// </summary>
    public class SQLSchemaTool
    {
        #region Variables
        /// <summary>
        /// namespace manager for the source schema
        /// </summary>
        private static XmlNamespaceManager nsmgr_Source;
        /// <summary>
        /// namespace manager for the destination/target schema
        /// </summary>
        private static XmlNamespaceManager nsmgr_Dest;
        /// <summary>
        /// Indicates the node type within the XML document
        /// </summary>
        public enum _NodeType
        {
            /// <summary>
            /// Indicates stored procedure node types.
            /// </summary>
            SPROC = 0x01,
            /// <summary>
            /// Indicates view node types.
            /// </summary>
            VIEW = 0x02,
            /// <summary>
            /// Indicates function node types.
            /// </summary>
            FUNCTION = 0x04,
            /// <summary>
            /// Indicates trigger node types.
            /// </summary>
            TRIGGER = 0x08,
            /// <summary>
            /// Indicates default node types.
            /// </summary>
            DEFAULT = 0x10,
            /// <summary>
            /// Indicates rule node types.
            /// </summary>
            RULE = 0x20,
            /// <summary>
            /// Indicates UDDT node types.
            /// </summary>
            UDDT = 0x40,
            /// <summary>
            /// 
            /// </summary>
            TABLE = 0x80
        }
        /// <summary>
        /// const value of "/DataBase_Schema/SPROC" used in XPATH queries.
        /// </summary>
        private const string SPROCPATH = @"/DataBase_Schema/SPROC";
        /// <summary>
        /// const value of "SPROC_TEXT[SPROC_NAME='" used in XPATH queries.
        /// </summary>
        private const string SPROCTEXT = @"SPROC_TEXT[SPROC_NAME='";
        /// <summary>
        /// const value of "SPROC_DEPENDS[SPROC_NAME='" used in XPATH queries.
        /// </summary>
        private const string SPROCDEP = @"SPROC_DEPENDS[SPROC_NAME='";
        /// <summary>
        /// const value of "/DataBase_Schema/FUNC" used in XPATH queries.
        /// </summary>
        private const string FUNCPATH = @"/DataBase_Schema/FUNC";
        /// <summary>
        /// const value of "FUNC_TEXT[FUNC_NAME='" used in XPATH queries.
        /// </summary>
        private const string FUNCTEXT = @"FUNC_TEXT[FUNC_NAME='";
        /// <summary>
        /// const value of "FUNC_DEPENDS[FUNC_NAME='" used in XPATH queries.
        /// </summary>
        private const string FUNCDEP = @"FUNC_DEPENDS[FUNC_NAME='";
        /// <summary>
        /// const value of "/DataBase_Schema/VIEW" used in XPATH queries.
        /// </summary>
        private const string VIEWPATH = @"/DataBase_Schema/VIEW";
        /// <summary>
        /// const value of "CREATE_TEXT[VIEW_NAME='" used in XPATH queries.
        /// </summary>
        private const string VIEWTEXT = @"CREATE_TEXT[VIEW_NAME='";
        /// <summary>
        /// const value of "CREATE_TEXT[VIEW_NAME='" used in XPATH queries.
        /// </summary>
        private const string VIEWDEP = @"VIEW_DEPENDS[VIEW_NAME='";
        /// <summary>
        /// const value of "VIEW_DEPENDS[VIEW_NAME='" used in XPATH queries.
        /// </summary>
        private const string TRIGGERPATH = @"/DataBase_Schema/TRIGGER";
        /// <summary>
        /// const value of "/DataBase_Schema/TRIGGER" used in XPATH queries.
        /// </summary>
        private const string TRIGGERTEXT = @"TRIGGER_TEXT[TRIGGER_NAME='";
        /// <summary>
        /// const value of "TRIGGER_TEXT[TRIGGER_NAME='" used in XPATH queries.
        /// </summary>
        private const string DEFAULTPATH = @"/DataBase_Schema/DEFAULT";
        /// <summary>
        /// const value of "/DataBase_Schema/DEFAULT" used in XPATH queries.
        /// </summary>
        private const string DEFAULTTEXT = @"DEFAULT_TEXT[DEFAULT_NAME='";
        /// <summary>
        /// const value of "DEFAULT_TEXT[DEFAULT_NAME='" used in XPATH queries.
        /// </summary>
        private const string RULEPATH = @"/DataBase_Schema/RULE";
        /// <summary>
        /// const value of "/DataBase_Schema/RULE" used in XPATH queries.
        /// </summary>
        private const string RULETEXT = @"RULE_TEXT[RULE_NAME='";
        /// <summary>
        /// const value of "RULE_TEXT[RULE_NAME='" used in XPATH queries.
        /// </summary>
        private const string UDDTPATH = @"/DataBase_Schema/UDDT";
        /// <summary>
        /// const value of "/DataBase_Schema/UDDT" used in XPATH queries.
        /// </summary>
        private const string UDDTTEXT = @"UDDT_TEXT[UDDT_NAME='";

        /// <summary>
        /// 2 part xml filename format string requires 2 string params for name
        /// </summary>
        public const string _OUTPUTFILE = "{0}_{1}_SCHEMA.xml";

        /// <summary>
        /// 2 part xml filename format string requires 2 string params for name
        /// </summary>
        public const string _DIFFFILE = "{0}_DIFF_{1}_SCHEMA.xml";

        /// <summary>
        /// 3 part error format string requires message, source and stack as passed in params
        /// </summary>
        public const string ERRORFORMAT = "\nError: {0}\nSource{1}\nStack{2}";

        private static object _threaded;

        private static Logger logger = LogManager.GetLogger("Lewis.SST.SQLSchemaTool");

        #endregion

		/// <summary>
		/// convert DB schema information into XML string/XML file
		/// </summary>
		/// <param name="SQLServer"></param>
		/// <param name="DBName"></param>
		/// <param name="UID"></param>
		/// <param name="PWD"></param>
		/// <param name="SQLfile"></param>
		/// <param name="Translate"></param>
		/// <param name="Primary"></param>
		/// <param name="threaded"></param>
		/// <param name="objectsToSerialize"></param>
		/// <param name="CustomXSLT"></param>
		/// <returns></returns>
        public static string SerializeDB(string SQLServer, string DBName, string UID, string PWD, string SQLfile, bool Translate,
            bool Primary, object threaded, byte objectsToSerialize, string CustomXSLT)
        {
            return SerializeDB(SQLServer, DBName, UID, PWD, SQLfile, Translate,
                Primary, threaded, objectsToSerialize, CustomXSLT, null);
        }

        /// <summary>
        /// Converts sql db objects of a connected SQL server database into an XML schema file.
        /// </summary>
        /// <param name="SQLServer">The string for the SQL server name to connect to.</param>
        /// <param name="DBName">The string for the SQL server database name to connect to.</param>
        /// <param name="UID">The string for the SQL server user login to use for the SqlConnections object.</param>
        /// <param name="PWD">The string for the SQL server user password to use for the SqlConnections object.</param>
        /// <param name="SQLfile">The SQL script file that will be translated from the create XML schema file.</param>
        /// <param name="Translate">A flag to cause the translation method to be called, thus creating the SQL script file 
        /// specified with the SQLfile param.</param>
        /// <param name="Primary">A flag to force the FileGroups to only have Primary, no matter what the actual one is in 
        /// the database.</param>
        /// <param name="threaded">Object that passes in a null or as bool</param>
        /// <param name="objectsToSerialize">bit mask of SQL objects to serialize - use the _nodetype enum</param>
        /// <param name="CustomXSLT">the full path and name of a custom xslt to be applied to the final xml output</param>
        /// <param name="delimTableNames">the comma delimited string of table names to be serialized</param>
        /// <returns>Returns the delimited ',' XML + SQL output filename(s)</returns>
        public static string SerializeDB(
            string SQLServer, 
            string DBName, 
            string UID, 
            string PWD, 
            string SQLfile, 
            bool Translate,
            bool Primary, 
            object threaded, 
            byte objectsToSerialize, 
            string CustomXSLT, 
            string delimTableNames)
        {
            _threaded = threaded;
            string _serverDB = SQLServer + ":" + DBName;
            string outputFile = string.Format(_OUTPUTFILE, SQLServer.Replace("\\", "_").Replace(":", "-"), 
                DBName.Replace("\\", "_").Replace(":", "-"));
            try
            {

                // TODO:  add threads if this takes a long while
                SQLMethods.SQLConnections _connections;
                if (UID != null && PWD != null)
                {
                    _connections = new SQLMethods.SQLConnections(SQLServer, DBName, UID, PWD, false);
                }
                else
                {
                    _connections = new SQLMethods.SQLConnections(SQLServer, DBName);
                }
                if (_connections != null && _connections.Count > 0)
                {
                    DataSet _ds = new DataSet("DataBase_Schema");
                    _ds.EnforceConstraints = false;
                    DataTable dt = _ds.Tables.Add("Database");
                    dt.Columns.Add("Name");
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Time");
                    DataRow dr = dt.NewRow();
                    dr.ItemArray = 
                        new object[] { DBName, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString() };
                    dt.Rows.Add(dr);

                    Sleep();

                    // get defaults, rules and UDDTs : in this order because of dependant behavior
                    if ((objectsToSerialize & Convert.ToByte(_NodeType.DEFAULT)) == (int)_NodeType.DEFAULT)
                    {
                        SQLObjects.Defaults _defaults = new SQLObjects.Defaults(DBName);
                        _defaults.GetObject<SQLObjects.Defaults>(_connections[0].sqlConnection);

                        _ds.Merge(_defaults);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.RULE)) == (int)_NodeType.RULE)
                    {
                        SQLObjects.Rules _rules = new SQLObjects.Rules(DBName);
                        _rules.GetObject<SQLObjects.Rules>(_connections[0].sqlConnection);

                        _ds.Merge(_rules);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.UDDT)) == (int)_NodeType.UDDT)
                    {
                        SQLObjects.UDDTs _uddts = new SQLObjects.UDDTs(DBName);
                        _uddts.GetObject<SQLObjects.UDDTs>(_connections[0].sqlConnection);

                        _ds.Merge(_uddts);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.TABLE)) == (int)_NodeType.TABLE)
                    {
                        SQLObjects.Tables _tables = new SQLObjects.Tables(DBName);
                        if (!string.IsNullOrEmpty(delimTableNames))
                        {
                            _tables.GetObject<SQLObjects.Tables>(_connections[0].sqlConnection, Primary, delimTableNames);
                        }
                        else
                        {
                            _tables.GetObject<SQLObjects.Tables>(_connections[0].sqlConnection, Primary);
                        }
                        // TODO:  make work with DBs attached as MDF files to SQL 2005
                        _ds.Merge(_tables);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.VIEW)) == (int)_NodeType.VIEW)
                    {
                        SQLObjects.Views _views = new SQLObjects.Views(DBName);
                        _views.GetObject<SQLObjects.Views>(_connections[0].sqlConnection);

                        _ds.Merge(_views);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.SPROC)) == (int)_NodeType.SPROC)
                    {
                        SQLObjects.Sprocs _sprocs = new SQLObjects.Sprocs(DBName);
                        _sprocs.GetObject<SQLObjects.Sprocs>(_connections[0].sqlConnection);

                        _ds.Merge(_sprocs);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.FUNCTION)) == (int)_NodeType.FUNCTION)
                    {
                        SQLObjects.Funcs _funcs = new SQLObjects.Funcs(DBName);
                        _funcs.GetObject<SQLObjects.Funcs>(_connections[0].sqlConnection);

                        _ds.Merge(_funcs);
                    }
                    Sleep();

                    if ((objectsToSerialize & Convert.ToByte(_NodeType.TRIGGER)) == (int)_NodeType.TRIGGER)
                    {
                        SQLObjects.Triggers _triggers = new SQLObjects.Triggers(DBName);
                        _triggers.GetObject<SQLObjects.Triggers>(_connections[0].sqlConnection);

                        _ds.Merge(_triggers);
                    }
                    // TODO: add jobs, users, roles

                    Sleep();

                    // get rid of old files
                    if (File.Exists(outputFile)) File.Delete(outputFile);

                    // write out xml schema document
                    XmlDataDocument xmlData = new XmlDataDocument(_ds);
                    //xmlData.Save(outputFile);

                    // reload to xml schema to avoid the "deleted row" error when removing the dependant child nodes
                    XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.Load(outputFile);
                    xmlDoc.LoadXml(xmlData.OuterXml);

                    Sleep();

                    // sort the dependencies for views, functions, and stored procedures
                    SortDependencies(_serverDB, VIEWPATH, VIEWDEP, ref xmlDoc);
                    SortDependencies(_serverDB, FUNCPATH, FUNCDEP, ref xmlDoc);
                    SortDependencies(_serverDB, SPROCPATH, SPROCDEP, ref xmlDoc);

                    foreach (Char c in Path.GetInvalidFileNameChars())
                    {
                        outputFile = outputFile.Replace(c, '_');
                    }
                    foreach (Char c in Path.GetInvalidPathChars())
                    {
                        outputFile = outputFile.Replace(c, '_');
                    }
                    xmlDoc.Save(outputFile);

                    // perform garbage collection to free up memory
                    GC.Collect();

                    if (Translate && outputFile != null && outputFile.Trim().Length > 0)
                    {
                        string createName = outputFile.ToLower().Replace(".xml", ".sql");
                        if (SQLfile != null && SQLfile.Length > 0)
                        {
                            createName = SQLfile.ToLower().Replace(".xml", ".sql");
                        }
                        if (!createName.EndsWith(".sql")) { createName += ".sql"; }
                        XsltHelper.SQLTransform(outputFile, XsltHelper.SQLCREATEXSLT, createName);
                        outputFile += "," + createName;
                        logger.Info("\nSQL Create Schema has been saved to " + createName + ".");
                    }
                    if (CustomXSLT != null && CustomXSLT.Trim().Length > 0)
                    {
                        FileInfo fi = new FileInfo(CustomXSLT);
                        File.WriteAllText("CustomOutput.XML", XsltHelper.Transform(xmlDoc.OuterXml, fi));
                        logger.Info("\nThe Custom XSLT {0}, has been applied and saved as CustomOutput.XML.", CustomXSLT);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Data.SqlClient.SqlException)
                {
                    logger.Error("\nSQL Error: {0}, DB Server {1}", ex.Message, _serverDB);
                }
                else
                {
                    logger.Error(ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
            }
            return outputFile;
        }

        /// <summary>
        /// Compares and outputs a SQL diffgram schema, both as XML and SQL script files.
        /// </summary>
        /// <param name="sourceFile">The source XML file name to compare with the destination XML file.</param>
        /// <param name="destinationFile">The destination/target XML file name to compare with the source XML file.</param>
        /// <param name="diffgramFile">The XML Diffgram file name to create.</param>
        /// <param name="SQLfile">The SQL script file that will be translated from the diffgram XML schema file.</param>
        /// <param name="CompareSprocText">A flag to cause stored procedures to be compared character by character, excluding 
        /// whitespace, tabs, linefeeds and carriage returns.</param>
        /// <param name="CompareViewText">A flag to cause views to be compared character by character, excluding whitespace, 
        /// tabs, linefeeds and carriage returns.</param>
        /// <param name="Translate">A flag to cause the translation method to be called, thus creating the SQL script file 
        /// specified with the SQLfile param.</param>
        /// <param name="objectsToSerialize">bit mask of SQL objects to serialize - use the _nodetype enum</param>
        /// <param name="CustomXSLT">the full path and name of a custom xslt to be applied to the final xml output</param>
        /// <returns>Returns the delimited ',' XML + SQL output filename(s)</returns>
        public static string CompareSchema(string sourceFile, string destinationFile, string diffgramFile, string SQLfile,
            bool CompareSprocText, bool CompareViewText, bool Translate, byte objectsToSerialize, string CustomXSLT)
        {
            string _serverDB = sourceFile + ":" + destinationFile;
            string outputfile = string.Format(_DIFFFILE, Path.GetFileNameWithoutExtension(sourceFile).Replace("_schema", "").Replace(".xml", ""), Path.GetFileNameWithoutExtension(destinationFile).Replace("_schema", "").Replace(".xml", ""));
            if (diffgramFile == null)
            {
                diffgramFile = outputfile;
            }
            try
            {
                // create XMlDiff object, used to compare xml nodes
                XmlDiff xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace);

                // create XmlTextWriter where the diffgram will be saved
                //XmlWriter diffgramWriter = null;
                if (diffgramFile != null)
                {
                    // get rid of old files
                    if (File.Exists(diffgramFile)) File.Delete(diffgramFile);
                    //diffgramWriter = new XmlTextWriter( diffgramFile, Encoding.Unicode );
                }

                if (!File.Exists(sourceFile))
                {
                    logger.Info("\nCompareSchema: The designated source file: {0}, cannot be located!", sourceFile);
                    return string.Empty;
                }

                // setup Xml doc to load the source from a XML file
                //XmlDataDocument xmlSourceDoc = new XmlDataDocument();
                XmlDocument xmlSourceDoc = new XmlDocument();
                xmlSourceDoc.Load(sourceFile);
                nsmgr_Source = new XmlNamespaceManager(xmlSourceDoc.NameTable);

                if (!File.Exists(destinationFile))
                {
                    logger.Info("\nCompareSchema: The designated destination file: {0}, cannot be located!", destinationFile);
                    return string.Empty;
                }

                // setup Xml doc to load the destination from a XML file
                //XmlDataDocument xmlDestinationDoc = new XmlDataDocument();
                XmlDocument xmlDestinationDoc = new XmlDocument();
                xmlDestinationDoc.Load(destinationFile);
                nsmgr_Dest = new XmlNamespaceManager(xmlDestinationDoc.NameTable);

                // setup a new XML doc for Output of the DiffGram
                //XmlDataDocument xmlDiffDoc = new XmlDataDocument();
                XmlDocument xmlDiffDoc = new XmlDocument();
                xmlDiffDoc.LoadXml("<DataBase_Schema>DiffData</DataBase_Schema>");
                XmlElement xel = xmlDiffDoc.CreateElement("Database");
                XmlElement xel1 = xmlDiffDoc.CreateElement("Name");
                xel1.InnerXml = string.Format("Compare results for Source DB XML snapshot: {0},\n-- with Target DB XML snapshot: {1}.\n-- Results are the SQL changes necessary to match\n-- the Target DB schema to the Source DB schema.", sourceFile, destinationFile);
                xel.AppendChild(xel1);
                XmlElement xel2 = xmlDiffDoc.CreateElement("Date");
                xel2.InnerXml = DateTime.Now.ToShortDateString();
                xel.AppendChild(xel2);
                XmlElement xel3 = xmlDiffDoc.CreateElement("Time");
                xel3.InnerXml = DateTime.Now.ToShortTimeString();
                xel.AppendChild(xel3);
                xmlDiffDoc.DocumentElement.AppendChild(xel);

                // TODO: add threads to perform the compare of each basic db object type in a seperate thread
                // TODO: add flags to allow seperate/individual processing of basic db object types

                if ((objectsToSerialize & Convert.ToByte(_NodeType.DEFAULT)) == (int)_NodeType.DEFAULT)
                {
                    // keep in mind that the compare expects the objects to be in name - alpha order
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.DEFAULT, xmlDiff, false);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.RULE)) == (int)_NodeType.RULE)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.RULE, xmlDiff, false);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.UDDT)) == (int)_NodeType.UDDT)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.UDDT, xmlDiff, false);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.TABLE)) == (int)_NodeType.TABLE)
                {
                    CompareTables(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, xmlDiff);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.VIEW)) == (int)_NodeType.VIEW)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.VIEW, xmlDiff, CompareViewText);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.SPROC)) == (int)_NodeType.SPROC)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.SPROC, xmlDiff, CompareSprocText);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.FUNCTION)) == (int)_NodeType.FUNCTION)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.FUNCTION, xmlDiff, true);
                }
                if ((objectsToSerialize & Convert.ToByte(_NodeType.TRIGGER)) == (int)_NodeType.TRIGGER)
                {
                    CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, _NodeType.TRIGGER, xmlDiff, true);
                }

                // write out xml DIFF doc
                foreach (Char c in Path.GetInvalidFileNameChars())
                {
                    diffgramFile = diffgramFile.Replace(c, '_');
                }
                foreach (Char c in Path.GetInvalidPathChars())
                {
                    diffgramFile = diffgramFile.Replace(c, '_');
                }
                xmlDiffDoc.Save(diffgramFile);

                logger.Info("\nSQL XML diffgram has been saved to " + diffgramFile + ".");

                // perform garbage collection to free up memory
                GC.Collect();

                // apply xslt transformation to diffgram xml schema to produce sql schema file
                if (SQLfile != null && Translate)
                {
                    logger.Info("\nPlease wait. Beginning SQL transformation of DiffGram XML...");
                    if (!SQLfile.EndsWith(".sql")) SQLfile += ".sql";

                    XsltHelper.SQLTransform(diffgramFile, XsltHelper.SQLDIFFXSLT, SQLfile);
                    logger.Info("\nSQL Diff Schema has been saved to " + SQLfile + ".");
                }
                if (CustomXSLT != null && CustomXSLT.Trim().Length > 0)
                {
                    FileInfo fi = new FileInfo(CustomXSLT);
                    File.WriteAllText("CustomOutput.XML", XsltHelper.Transform(xmlDiffDoc.OuterXml, fi));
                    logger.Info("\nThe Custom XSLT {0}, has been applied and saved as CustomOutput.XML.", CustomXSLT);
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Data.SqlClient.SqlException)
                {
                    logger.Error("\nSQL Error: {0}, DB Server {1}", ex.Message, _serverDB);
                }
                else
                {
                    logger.Error(ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
            }
            return diffgramFile + (SQLfile == null ? string.Empty : ("," + SQLfile));
        }

        /// <summary>
        /// Compares SQL table schemas from two distinct XML schema files.  These files are generated by the SerializeDB method.
        /// </summary>
        /// <param name="_serverDB"></param>
        /// <param name="xmlSourceDoc">The XmlDataDocument, the source XML to compare with the destination XML.</param>
        /// <param name="xmlDestinationDoc">The XmlDataDocument, the destination/target XML to compare with the source XML.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram document to update.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        public static void CompareTables(string _serverDB, XmlDocument xmlSourceDoc, XmlDocument xmlDestinationDoc,
            XmlDocument xmlDiffDoc, XmlDiff xmlDiff)
        {
            CompareTables(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, xmlDiff, null);
        }

        /// <summary>
        /// Compares SQL table schemas from two distinct XML schema files.  These files are generated by the SerializeDB method.
        /// </summary>
        /// <param name="_serverDB"></param>
        /// <param name="xmlSourceDoc">The XmlDataDocument, the source XML to compare with the destination XML.</param>
        /// <param name="xmlDestinationDoc">The XmlDataDocument, the destination/target XML to compare with the source XML.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram document to update.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        /// <param name="TableName">specifies a specific SQL table to compare</param>
        public static void CompareTables(string _serverDB, XmlDocument xmlSourceDoc, XmlDocument xmlDestinationDoc,
            XmlDocument xmlDiffDoc, XmlDiff xmlDiff, string TableName)
        {
            // NOTE:  we are ignoring the destination tables that don't exist in the source DB
            XPathNavigator navSource = xmlSourceDoc.CreateNavigator();
            XPathNavigator navDest = xmlDestinationDoc.CreateNavigator();

            // iterate thru the source tables 
            // we are going to ignore destination tables which are not in the source DB
            // as they could be custom tables added by some outside tool or person
            XmlNodeList xNodeList = xmlSourceDoc.SelectNodes("/DataBase_Schema/TABLE");
            foreach (XmlNode xnChild_Source in xNodeList)
            {
                XmlNode xmlTableNode = null;
                string tableName = xnChild_Source.ChildNodes[0].InnerXml;
                if (TableName != null && !tableName.ToLower().Equals(TableName.ToLower()))
                {
                    continue; // if in single table mode then loop until correct table is found
                }
                string xpath = "descendant::TABLE[TABLE_NAME='" + tableName + "']/COLUMN";
                logger.Debug("\n{1}: Comparing columns for {0}.", tableName, _serverDB);

                XPathExpression exprSource = navSource.Compile(xpath);
                XPathExpression exprDest = navDest.Compile(xpath);

                // get xmlnodes for columns in source
                XmlNodeList xnlSource = xmlSourceDoc.SelectNodes(exprSource.Expression);
                // get xmlnode for table in destination
                XmlNode xmlDestTable = xmlDestinationDoc.SelectSingleNode("/DataBase_Schema/TABLE[TABLE_NAME='" + tableName + "']");

                if (xmlDestTable != null)
                {
                    // if table doesn't compare then walk thru each column else
                    if (!xmlDiff.Compare(xnChild_Source, xmlDestTable))
                    {
                        // get xmlnodes for columns in destination
                        XmlNodeList xnlDestination = xmlDestinationDoc.SelectNodes(exprDest.Expression);
                        // Compare table columns and updates the DiffGram XML doc
                        // TODO: use async threads to launch CompareColumns so that multiples can run at the same time
                        CompareColumns(tableName, xnlSource, xnlDestination, xmlDiffDoc, xmlDiff);

                        // add the missing source table related child nodes
                        xmlTableNode = xmlDiffDoc.SelectSingleNode("/DataBase_Schema/TABLE[TABLE_NAME='" + tableName + "']");
                        if (xmlTableNode != null)
                        {
                            // assume same owner as source table, may need to change this in the future
                            xmlTableNode.SelectSingleNode("TABLE_OWNER").InnerXml = xnChild_Source.SelectSingleNode("TABLE_OWNER").InnerXml;
                            // assume same FileGroup as source table, may need to change this in the future
                            xmlTableNode.SelectSingleNode("TABLE_FILEGROUP").InnerXml = xnChild_Source.SelectSingleNode("TABLE_FILEGROUP").InnerXml;
                            // walk all source and dest references nodes and add them to the xml diff doc
                            CompareTableObjs("References", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                            // walk all source and dest index nodes and add them to the xml diff doc
                            CompareTableObjs("Indexes", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                            // walk all source and dest constraints nodes and add them to the xml diff doc
                            CompareTableObjs("Constraints", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                        }
                        else
                        {
                            xmlTableNode = xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE", xmlDiffDoc.NamespaceURI);
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_NAME", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_OWNER", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_FILEGROUP", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_REFERENCE", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_CONSTRAINTS", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_ORIG_CONSTRAINTS", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.AppendChild(xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE_ORIG_REFERENCE", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.Attributes.Append((XmlAttribute)xmlDiffDoc.CreateNode(XmlNodeType.Attribute, "Action", xmlDiffDoc.NamespaceURI));
                            xmlTableNode.SelectSingleNode("TABLE_NAME").InnerXml = tableName;
                            xmlTableNode.Attributes["Action"].Value = "Alter";

                            xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xmlTableNode);
                            // assume same owner as source table, may need to change this in the future
                            xmlTableNode.SelectSingleNode("TABLE_OWNER").InnerXml = xnChild_Source.SelectSingleNode("TABLE_OWNER").InnerXml;
                            // assume same FileGroup as source table, may need to change this in the future
                            xmlTableNode.SelectSingleNode("TABLE_FILEGROUP").InnerXml = xnChild_Source.SelectSingleNode("TABLE_FILEGROUP").InnerXml;
                            // walk all source and dest references nodes and add them to the xml diff doc
                            CompareTableObjs("References", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                            // walk all source and dest index nodes and add them to the xml diff doc
                            CompareTableObjs("Indexes", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                            // walk all source and dest constraints nodes and add them to the xml diff doc
                            CompareTableObjs("Constraints", xnChild_Source, xmlDestTable, xmlTableNode, xmlDiffDoc, xmlDiff);
                        }
                    }
                    else
                        continue;
                }
                else // a new table which doesn't have the add element or attribute for the columns, just the table itself
                {
                    // add copy of source table node (and child nodes) to xmldiffdoc, since it doesn't exist in the destination 
                    // table
                    xmlTableNode = xmlDiffDoc.CreateNode(XmlNodeType.Element, "TABLE", xmlDiffDoc.NamespaceURI);
                    xmlTableNode.InnerXml = xnChild_Source.InnerXml;
                    xmlTableNode.Attributes.Append((XmlAttribute)xmlDiffDoc.CreateNode(XmlNodeType.Attribute, "Action", xmlDiffDoc.NamespaceURI));
                    xmlTableNode.Attributes["Action"].Value = "Add";
                    xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xmlTableNode);
                }
            }
            navSource = null;
            navDest = null;
        }

        /// <summary>
        /// Compares table indexes, table constraints, and table FK/PK references from two distinct XML schema files. These 
        /// files are generated by the SerializeDB method.  This method is called by the CompareTables method.
        /// </summary>
        /// <param name="objType">The type of database table related object collection to compare, i.e. indexes, constraints 
        /// (check and default), and references.</param>
        /// <param name="SourceTable">The SourceTable XML Node containing all table object related child nodes to compare with 
        /// the destination child nodes.</param>
        /// <param name="DestTable">The DestTable XML Node containing all table object related child nodes to compare with 
        /// the source child nodes.</param>
        /// <param name="DiffTable">The XML Diffgram table node to update.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram document to update.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        private static void CompareTableObjs(string objType, XmlNode SourceTable, XmlNode DestTable, XmlNode DiffTable,
            XmlDocument xmlDiffDoc, XmlDiff xmlDiff)
        {
            string MainNode = string.Empty;
            string OrigNode = string.Empty;
            string xpathpart = string.Empty;
            string objName = string.Empty;
            switch (objType.ToLower())
            {
                case "indexes":
                    {
                        MainNode = "TABLE_INDEX";
                        OrigNode = "TABLE_ORIG_INDEX";
                        xpathpart = "TABLE_INDEX[index_name='";
                        objName = "index_name";
                        break;
                    }
                case "constraints":
                    {
                        MainNode = "TABLE_CONSTRAINTS";
                        OrigNode = "TABLE_ORIG_CONSTRAINTS";
                        xpathpart = "TABLE_CONSTRAINTS[CONSTRAINT_NAME='";
                        objName = "CONSTRAINT_NAME";
                        break;
                    }
                case "references":
                    {
                        MainNode = "TABLE_REFERENCE";
                        OrigNode = "TABLE_ORIG_REFERENCE";
                        xpathpart = "TABLE_REFERENCE[Constraint='";
                        objName = "Constraint";
                        break;
                    }
            }

            XmlNodeList xmlSourceObjs = SourceTable.SelectNodes(MainNode);
            XmlNodeList xmlDestObjs = DestTable.SelectNodes(MainNode);
            // compare source and dest nodes, also looking for missing dest nodes
            foreach (XmlNode xnsourceobj in xmlSourceObjs)
            {
                XmlNode xn = xnsourceobj.SelectSingleNode(objName);
                if (xn != null)
                {
                    string source_obj_name = xn.InnerXml;
                    XmlNode dest_obj_node = DestTable.SelectSingleNode(xpathpart + source_obj_name + "']");
                    // if dest index node is found then compare
                    if (dest_obj_node != null)
                    {
                        // if the source and dest index nodes don't match then add both the the xml diff file
                        if (!xmlDiff.Compare(xnsourceobj, dest_obj_node))
                        {
                            // check node for children, otherwise its a waste of time
                            if (xnsourceobj.HasChildNodes)
                            {
                                XmlNode xnChild1 = xmlDiffDoc.CreateNode(XmlNodeType.Element, MainNode, xmlDiffDoc.NamespaceURI);
                                xnChild1.InnerXml = xnsourceobj.InnerXml;
                                DiffTable.AppendChild(xnChild1);
                            }
                            // check node for children, otherwise its a waste of time
                            if (dest_obj_node.HasChildNodes)
                            {
                                XmlNode xnChild2 = xmlDiffDoc.CreateNode(XmlNodeType.Element, OrigNode, xmlDiffDoc.NamespaceURI);
                                xnChild2.InnerXml = dest_obj_node.InnerXml;
                                DiffTable.AppendChild(xnChild2);
                            }
                        }
                    }
                    else // missing dest node, so add source node
                    {
                        // check node for children, otherwise its a waste of time
                        if (xnsourceobj.HasChildNodes)
                        {
                            XmlNode xnChild1 = xmlDiffDoc.CreateNode(XmlNodeType.Element, MainNode, xmlDiffDoc.NamespaceURI);
                            xnChild1.InnerXml = xnsourceobj.InnerXml;
                            DiffTable.AppendChild(xnChild1);
                        }
                    }
                }
            }
            // look for dest nodes that don't exist in source
            foreach (XmlNode xndestobj in xmlDestObjs)
            {
                XmlNode xn = xndestobj.SelectSingleNode(objName);
                if (xn != null)
                {
                    string dest_obj_name = xn.InnerXml;
                    XmlNode source_index_node = SourceTable.SelectSingleNode(xpathpart + dest_obj_name + "']");
                    // if dest node doesn't exist in source table then add dest node to the xml diff doc so thaqt it can be removed
                    if (source_index_node == null || !source_index_node.HasChildNodes)
                    {
                        // check node for children, otherwise its a waste of time
                        if (xndestobj.HasChildNodes)
                        {
                            XmlNode xnChild2 = xmlDiffDoc.CreateNode(XmlNodeType.Element, OrigNode, xmlDiffDoc.NamespaceURI);
                            xnChild2.InnerXml = xndestobj.InnerXml;
                            DiffTable.AppendChild(xnChild2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares database objects (except tables and columns) from two distinct XML schema files. These files are generated 
        /// by the SerializeDB method.  This method is called from the CompareSchema method.
        /// </summary>
        /// <param name="_serverDB">Used to identify which server the compare objects came from</param>
        /// <param name="xmlSourceDoc">The XmlDataDocument, the source XML to compare with the destination XML.</param>
        /// <param name="xmlDestinationDoc">The XmlDataDocument, the destination/target XML to compare with the source XML.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram document to update.</param>
        /// <param name="type">The type of database object collection to compare, i.e. defaults, UDDTs, rules, views, functions, 
        /// stored procedures, and triggers.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        /// <param name="CompareTextFlag">Flag that performs full text compare between objects</param>
        public static void CompareObjects(string _serverDB, XmlDocument xmlSourceDoc, XmlDocument xmlDestinationDoc,
            XmlDocument xmlDiffDoc, _NodeType type, XmlDiff xmlDiff, bool CompareTextFlag)
        {
            CompareObjects(_serverDB, xmlSourceDoc, xmlDestinationDoc, xmlDiffDoc, type, xmlDiff, CompareTextFlag, null);
        }

        /// <summary>
        /// Compares database objects (except tables and columns) from two distinct XML schema files. These files are generated 
        /// by the SerializeDB method.  This method is called from the CompareSchema method.
        /// </summary>
        /// <param name="_serverDB">Used to identify which server the compare objects came from</param>
        /// <param name="xmlSourceDoc">The XmlDataDocument, the source XML to compare with the destination XML.</param>
        /// <param name="xmlDestinationDoc">The XmlDataDocument, the destination/target XML to compare with the source XML.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram document to update.</param>
        /// <param name="type">The type of database object collection to compare, i.e. defaults, UDDTs, rules, views, functions, 
        /// stored procedures, and triggers.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        /// <param name="CompareTextFlag">Flag that performs full text compare between objects</param>
        /// <param name="sqlObjectName">specifies a specific SQL object to compare</param>
        public static void CompareObjects(string _serverDB, XmlDocument xmlSourceDoc, XmlDocument xmlDestinationDoc,
            XmlDocument xmlDiffDoc, _NodeType type, XmlDiff xmlDiff, bool CompareTextFlag, string sqlObjectName)
        {
            SortedList sl = new SortedList();
            SortedList slDep = new SortedList();

            bool addFlag = false;
            bool alterFlag = false;
            bool sortFlag = false;

            string selectNodes = string.Empty;
            string selectText = string.Empty;
            string typeName = string.Empty;
            string depText = string.Empty;

            // iterate thru the source tables 
            // we are going to ignore destination tables which are not in the source DB
            // as they could be custom tables added by some outside tool or person
            switch (type)
            {
                case _NodeType.SPROC:
                    {
                        selectNodes = SPROCPATH;
                        selectText = SPROCTEXT;
                        depText = SPROCDEP;
                        typeName = type.ToString();
                        sortFlag = true;
                        break;
                    }
                case _NodeType.FUNCTION:
                    {
                        selectNodes = FUNCPATH;
                        selectText = FUNCTEXT;
                        depText = FUNCDEP;
                        typeName = type.ToString().Substring(0, 4);
                        sortFlag = true;
                        break;
                    }
                case _NodeType.VIEW:
                    {
                        selectNodes = VIEWPATH;
                        selectText = VIEWTEXT;
                        depText = VIEWDEP;
                        typeName = type.ToString();
                        sortFlag = true;
                        break;
                    }
                case _NodeType.TRIGGER:
                    {
                        selectNodes = TRIGGERPATH;
                        selectText = TRIGGERTEXT;
                        typeName = type.ToString();
                        break;
                    }
                case _NodeType.DEFAULT:
                    {
                        selectNodes = DEFAULTPATH;
                        selectText = DEFAULTTEXT;
                        typeName = type.ToString();
                        break;
                    }
                case _NodeType.RULE:
                    {
                        selectNodes = RULEPATH;
                        selectText = RULETEXT;
                        typeName = type.ToString();
                        break;
                    }
                case _NodeType.UDDT:
                    {
                        selectNodes = UDDTPATH;
                        selectText = UDDTTEXT;
                        typeName = type.ToString();
                        break;
                    }
            }

            SortedList slMatch = new SortedList();
            XmlNodeList xNodeList = xmlSourceDoc.SelectNodes(selectNodes, nsmgr_Source);
            XmlNode xnDestParent = xmlDestinationDoc.SelectSingleNode(selectNodes, nsmgr_Dest);

            // use stringbuilder class as it should be faster
            StringBuilder sourceTxt = new StringBuilder();
            StringBuilder destTxt = new StringBuilder();
            StringBuilder sortedname = new StringBuilder();

            foreach (XmlNode xnChild_Source in xNodeList)
            {
                XmlNode xnDest = null;
                XmlNodeList XmlDestTextList = null;
                sourceTxt.Length = 0;
                destTxt.Length = 0;
                int destNodeCount = 0;
                int sourceNodeCount = 0;

                string SqlObjectName = xnChild_Source.ChildNodes[0].InnerXml;
                if (sqlObjectName != null && !SqlObjectName.ToLower().Equals(sqlObjectName.ToLower()))
                {
                    continue; // loop until passed in object name is found
                }
                string xpath = selectText + SqlObjectName + "']";
                XmlNode xnChk = xnChild_Source.SelectSingleNode("Check_Sum");
                string src_checksum = string.Empty;
                string dst_checksum = string.Empty;
                if (xnChk != null)
                {
                    src_checksum = xnChk.InnerXml;
                }

                logger.Debug("\n{2}: Comparing {1}: {0}.", SqlObjectName, type.ToString().ToLower(), _serverDB);

                // walk thru destination xml doc nodes to find matching sproc node
                if (xnDestParent != null)
                {
                    xnDest = xnDestParent.SelectSingleNode(selectNodes + "[" + xpath.Split('[')[1]);
                }
                // get xmlnodes for text rows in destination doc
                if (xnDest != null)
                {
                    xnChk = xnDest.SelectSingleNode("Check_Sum");
                    if (xnChk != null)
                    {
                        dst_checksum = xnChk.InnerXml;
                    }
                    // if objects don't compare then walk each line of text
                    if (!xmlDiff.Compare(xnChild_Source, xnDest))
                    {
                        XmlDestTextList = xnDest.SelectNodes(selectText.Split('[')[0]);
                        // only comapre the text length (not a checksum) if there is not a match on the nodes
                        if (xnChk != null && dst_checksum == src_checksum)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                if (xnDest != null && XmlDestTextList != null && XmlDestTextList.Count > 0)
                {
                    // don't care about existing UDDTs and Defaults, only new ones
                    // so resume at next node
                    if (type == _NodeType.UDDT || type == _NodeType.DEFAULT || type == _NodeType.RULE)
                    {
                        continue;
                    }

                    if (CompareTextFlag)
                    {
                        // get xmlnodes for text rows in source
                        XmlNodeList xnlSource = xnChild_Source.SelectNodes(xpath, nsmgr_Source);
                        // build text for source
                        foreach (XmlNode xnSource in xnlSource)
                        {
                            // ignore blank text nodes, tabs, newline and carriage returns
                            if (xnSource.SelectSingleNode("Text") == null || xnSource.SelectSingleNode("Text").InnerText.Length == 0)
                            {
                                continue;
                            }
                            sourceNodeCount += 1;
                            sourceTxt.Append(xnSource.SelectSingleNode("Text").InnerText.ToLower().Trim());
                            sourceTxt.Replace(" ", "");
                            sourceTxt.Replace("\n", "");
                            sourceTxt.Replace("\r", "");
                            sourceTxt.Replace("\t", "");
                        }
                        if (sourceTxt.ToString().Length > 0)
                        {
                            // build text for destination
                            foreach (XmlNode xnChild in XmlDestTextList)
                            {
                                // ignore blank text nodes, tabs, newline and carriage returns
                                if (xnChild.SelectSingleNode("Text") == null || xnChild.SelectSingleNode("Text").InnerText.Length == 0)
                                {
                                    continue;
                                }
                                destNodeCount += 1;
                                destTxt.Append(xnChild.SelectSingleNode("Text").InnerText.ToLower().Trim());
                                destTxt.Replace(" ", "");
                                destTxt.Replace("\n", "");
                                destTxt.Replace("\r", "");
                                destTxt.Replace("\t", "");
                                // look for this text string in the source text string
                                // if theres no match then we do not need to look thru
                                // all the dest text nodes, so exit out of loop, instead of continuing on
                                // Contains = true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false. 
                                if (!sourceTxt.ToString().Contains(destTxt.ToString()) && destTxt.ToString().Length > 0)
                                {
                                    break;
                                }
                            }
                        }
                        //CompareInfo compare = CultureInfo.InvariantCulture.CompareInfo;
                        //int compareResult = compare.Compare(sourceTxt.ToString(), destTxt.ToString(), CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace);
                    }
                    else
                    {
                        alterFlag = true;
                    }
                    // compare source and dest text
                    if (sourceTxt.ToString().ToLower() != destTxt.ToString().ToLower())
                    {
                        alterFlag = true;
                    }
                    // if no differences then resume at next source node
                    if (!alterFlag)
                    {
                        continue;
                    }
                }
                else  // we have a source node, but no matching destination node so set the add flag to true
                {
                    addFlag = true;
                }
                // if no destination text, we need to add it from the source
                if (sourceNodeCount > 0 && (destNodeCount == 0 || xnDest == null))
                {
                    // the object_text node was not found in the destination so add parent node for object to DiffDoc as an add
                    addFlag = true;
                }
                XmlNode xNode = null;
                if (addFlag)
                {
                    xNode = xmlDiffDoc.CreateNode(XmlNodeType.Element, typeName, xmlDiffDoc.NamespaceURI);
                    XmlAttribute xNodeAttrib = xmlDiffDoc.CreateAttribute("Action");
                    xNodeAttrib.Value = "Add";
                    xNode.InnerXml = xnChild_Source.InnerXml;
                    xNode.Attributes.Append(xNodeAttrib);
                    if (!sortFlag) xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xNode);
                    addFlag = false;
                }
                if (alterFlag)
                {
                    xNode = xmlDiffDoc.CreateNode(XmlNodeType.Element, typeName, xmlDiffDoc.NamespaceURI);
                    XmlAttribute xNodeAttrib = xmlDiffDoc.CreateAttribute("Action");
                    xNodeAttrib.Value = "Alter";
                    xNode.InnerXml = xnChild_Source.InnerXml;
                    xNode.Attributes.Append(xNodeAttrib);
                    if (!sortFlag) xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xNode);
                    alterFlag = false;
                }
                if (sortFlag)
                {
                    if (xNode != null)
                    {
                        XmlNodeList xnlSource = xnChild_Source.SelectNodes(depText + SqlObjectName + "']", nsmgr_Source);
                        // get dependencies names and add to sorted list
                        int ii = 0;
                        foreach (XmlNode xnSource in xnlSource)
                        {
                            if (xnSource.ChildNodes[1] != null)
                            {
                                slDep.Add(string.Format("{1:0000}!{0}", SqlObjectName.ToLower(), ii), xnSource.ChildNodes[1].InnerXml.ToLower());
                                ii += 1;
                            }
                        }
                        if (ii > 0)
                        {
                            sl.Add(string.Format("{1:0000}!{0}", SqlObjectName.ToLower(), ii), xNode);
                        }
                        else
                        {
                            sl.Add(string.Format("0000!{0}", SqlObjectName.ToLower()), xNode);
                        }
                    }
                }
            } // end of main for loop
            if (sortFlag)
            {
                // if sldep has count > 0 then we need to do some additional sorting
                SortedList sl2 = new SortedList();
                int zz = 0;
                for (int ii = 0; ii < sl.Count; ii++)
                {
                    sl2.Add(string.Format("{1:00000},{0}", sl.GetKey(ii).ToString(), zz), sl.GetByIndex(ii));
                    zz += 10;
                }
                if (slDep.Count > 0)
                {
                    int start = 0;
                    RecurseDependencies(_serverDB, ref sl2, ref slDep, 0, ref start);
                }
                for (int ii = 0; ii < sl2.Count; ii++)
                {
                    XmlNode xn = (XmlNode)sl2.GetByIndex(ii);
                    xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xn);
                }
            }
        }

        /// <summary>
        /// Compares table columns from two distinct XML schema files. These files are generated by the SerializeDB method.  
        /// This method is called from the CompareTables method.
        /// </summary>
        /// <param name="tableName">The string value representing the current SQL table object.</param>
        /// <param name="xnlSource">The XmlNodeList, a collection of source XML nodes to compare with the destination XML 
        /// nodes.</param>
        /// <param name="xnlDestination">The XmlNodeList, a collection of destination/target XML nodes to compare with the 
        /// source XML nodes.</param>
        /// <param name="xmlDiffDoc">The XML Diffgram object to update.</param>
        /// <param name="xmlDiff">The XmlDiff Object used from the GotDotNet XmlDiff class. Performs the XML node compare.</param>
        private static void CompareColumns(string tableName, XmlNodeList xnlSource, XmlNodeList xnlDestination,
            XmlDocument xmlDiffDoc, XmlDiff xmlDiff)
        {
            SQLObjects.ColumnCollection tableColumns = new SQLObjects.ColumnCollection(tableName);

            Hashtable htDropAdd = new Hashtable();
            // compare source columns to destination columns, looking for changed or missing destination columns
            if (xnlSource != null)
            {
                // if a matching destination table was found with columns
                if (xnlDestination != null)
                {
                    tableColumns.SchemaAction = SQLObjects.COLUMN.ColumnAction.Alter;
                    // identify the source columns that are different
                    foreach (XmlNode xn in xnlSource)
                    {
                        string column_Name = xn.ChildNodes[1].InnerXml;
                        XmlNode Found = FindNode(xnlDestination, column_Name, "<Column_Name>{0}</Column_Name>");
                        // look for existing columns
                        if (Found != null)
                        {
                            // if the columns don't compare then
                            if (!xmlDiff.Compare(xn, Found))
                            {
                                SQLObjects.COLUMN col = new SQLObjects.COLUMN();
                                col.Action = SQLObjects.COLUMN.ColumnAction.Alter;

                                // add original_rules from the destination column so that if they are not on the source columns we can exec sp_unbindrule on them.
                                // There is XSLT code to handle sp_bindrule for the source columns where they were not bound to the destination(original) column.
                                // IF the source and destination rule names are the same, then we should be able to ignore changing the column bound rule
                                XmlNode xnRule = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "RULE_ORIG_NAME", xn.OwnerDocument.NamespaceURI);
                                if (Found.SelectSingleNode("Rule_Name") != null)
                                {
                                    xnRule.InnerXml = Found.SelectSingleNode("Rule_Name").InnerXml;
                                    xn.AppendChild(xnRule);
                                }

                                xnRule = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "RULE_ORIG_OWNER", xn.OwnerDocument.NamespaceURI);
                                if (Found.SelectSingleNode("Rule_Owner") != null)
                                {
                                    xnRule.InnerXml = Found.SelectSingleNode("Rule_Owner").InnerXml;
                                    xn.AppendChild(xnRule);
                                }

                                XmlNode xnDefault = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "DEFAULT_ORIG_NAME", xn.OwnerDocument.NamespaceURI);
                                if (Found.SelectSingleNode("Default_Name") != null)
                                {
                                    xnDefault.InnerXml = Found.SelectSingleNode("Default_Name").InnerXml;
                                    xn.AppendChild(xnDefault);
                                }

                                xnDefault = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "DEFAULT_ORIG_VALUE", xn.OwnerDocument.NamespaceURI);
                                if (Found.SelectSingleNode("Default_Value") != null)
                                {
                                    xnDefault.InnerXml = Found.SelectSingleNode("Default_Value").InnerXml;
                                    xn.AppendChild(xnDefault);
                                }

                                XmlNode xnRowGuidCol = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "ORIG_RowGuidCol", xn.OwnerDocument.NamespaceURI);
                                if (Found.SelectSingleNode("isRowGuidCol") != null)
                                {
                                    xnRowGuidCol.InnerXml = Found.SelectSingleNode("isRowGuidCol").InnerXml;
                                    xn.AppendChild(xnRowGuidCol);
                                }

                                // lookup any altered columns to see if there are Reference dependencies
                                // may need to use something like this: descendant::*[contains(local-name(),'cKeyCol')]
                                for (int x = 1; x < 17; x++)
                                {
                                    if (Found.SelectSingleNode("../TABLE_REFERENCE/cRefCol" + x.ToString()) != null)
                                    {
                                        CheckColumnDependencies(column_Name, tableName, "DropAdd_References", "TABLE_REFERENCE", "Constraint", "cRefCol" + x.ToString(),
                                            false, ref htDropAdd, Found, xn, xmlDiffDoc);
                                    }
                                }

                                // lookup any altered columns to see if there are Constraint dependencies
                                CheckColumnDependencies(column_Name, tableName, "DropAdd_Constraints", "TABLE_CONSTRAINTS", "CONSTRAINT_NAME", "COLUMN_NAME",
                                    false, ref htDropAdd, Found, xn, xmlDiffDoc);

                                // lookup any altered columns to see if there are index dependencies
                                CheckColumnDependencies(column_Name, tableName, "DropAdd_Indexes", "TABLE_INDEX", "index_name", "index_keys",
                                    false, ref htDropAdd, Found, xn, xmlDiffDoc);

                                // add xml node to the table columns collection
                                SQLObjects.COLUMN c = col.Convert(xn);
                                tableColumns.Add(c);
                            }
                            else
                                continue;
                        }
                        else // the column was not found in the destination table
                        {
                            SQLObjects.COLUMN col = new SQLObjects.COLUMN();
                            col.Action = SQLObjects.COLUMN.ColumnAction.Add;
                            tableColumns.Add(col.Convert(xn));
                        }
                    }
                }
                else // no destination table so add all the columns
                {
                    foreach (XmlNode xn in xnlSource)
                    {
                        string column_Name = xn.ChildNodes[1].InnerXml;
                        SQLObjects.COLUMN col = new SQLObjects.COLUMN();
                        col.Action = SQLObjects.COLUMN.ColumnAction.Add;
                        tableColumns.Add(col.Convert(xn));
                    }
                }
            }
            // look for desination columns not in the source table, so as to mark the desination columns to drop
            if (xnlDestination != null)
            {
                if (xnlSource != null)
                {
                    tableColumns.SchemaAction = SQLObjects.COLUMN.ColumnAction.Alter;
                    // identify the destination columns that are missing 
                    foreach (XmlNode xn in xnlDestination)
                    {
                        string column_Name = xn.ChildNodes[1].InnerXml;
                        XmlNode Found = FindNode(xnlSource, column_Name, "<Column_Name>{0}</Column_Name>");
                        if (Found == null)
                        {
                            SQLObjects.COLUMN col = new SQLObjects.COLUMN();
                            col.Action = SQLObjects.COLUMN.ColumnAction.Drop;

                            XmlNode xnRule = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "RULE_ORIG_NAME", xn.OwnerDocument.NamespaceURI);
                            if (xn.SelectSingleNode("Rule_Name") != null)
                            {
                                xnRule.InnerXml = xn.SelectSingleNode("Rule_Name").InnerXml;
                                xn.AppendChild(xnRule);
                            }

                            xnRule = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "RULE_ORIG_OWNER", xn.OwnerDocument.NamespaceURI);
                            if (xn.SelectSingleNode("Rule_Owner") != null)
                            {
                                xnRule.InnerXml = xn.SelectSingleNode("Rule_Owner").InnerXml;
                                xn.AppendChild(xnRule);
                            }

                            XmlNode xnDefault = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "DEFAULT_ORIG_NAME", xn.OwnerDocument.NamespaceURI);
                            if (xn.SelectSingleNode("Default_Name") != null)
                            {
                                xnDefault.InnerXml = xn.SelectSingleNode("Default_Name").InnerXml;
                                xn.AppendChild(xnDefault);
                            }

                            xnDefault = xn.OwnerDocument.CreateNode(XmlNodeType.Element, "DEFAULT_ORIG_VALUE", xn.OwnerDocument.NamespaceURI);
                            if (xn.SelectSingleNode("Default_Value") != null)
                            {
                                xnDefault.InnerXml = xn.SelectSingleNode("Default_Value").InnerXml;
                                xn.AppendChild(xnDefault);
                            }

                            // lookup any dropped columns to see if there are Reference dependencies,
                            // may need to use something like this: descendant::*[contains(local-name(),'cKeyCol')]
                            for (int x = 1; x < 17; x++)
                            {
                                if (xn.SelectSingleNode("../TABLE_REFERENCE/cRefCol" + x.ToString()) != null)
                                {
                                    CheckColumnDependencies(column_Name, tableName, "DropAdd_References", "TABLE_REFERENCE", "Constraint", "cRefCol" + x.ToString(),
                                        true, ref htDropAdd, null, xn, xmlDiffDoc);
                                }
                            }

                            // lookup any altered columns to see if there are Constraint dependencies
                            CheckColumnDependencies(column_Name, tableName, "DropAdd_Constraints", "TABLE_CONSTRAINTS", "CONSTRAINT_NAME", "COLUMN_NAME",
                                true, ref htDropAdd, null, xn, xmlDiffDoc);

                            // lookup any dropped columns to see if there are index dependencies
                            CheckColumnDependencies(column_Name, tableName, "DropAdd_Indexes", "TABLE_INDEX", "index_name", "index_keys",
                                true, ref htDropAdd, null, xn, xmlDiffDoc);

                            tableColumns.Add(col.Convert(xn));
                        }
                    }
                }
            }
            // persist the tableColumns collection as XML if there are any
            if (tableColumns.Count > 0)
            {
                XmlNode xTableColumns = tableColumns.SerializeAsXmlNode(xmlDiffDoc);
                foreach (object obj in htDropAdd.Values)
                {
                    xTableColumns.AppendChild((XmlNode)obj);
                }
                xmlDiffDoc.SelectSingleNode("/DataBase_Schema").AppendChild(xTableColumns);
            }
        }

        /// <summary>
        /// Checks the column dependencies for Indexes, Constraints and References.
        /// </summary>
        /// <param name="column_Name">Name of the column.</param>
        /// <param name="table_Name">Name of the table.</param>
        /// <param name="elementName">Name of the new element to add to XML doc.</param>
        /// <param name="parentNodeName">Name of the parent node.</param>
        /// <param name="childNodeName">Name of the child node.</param>
        /// <param name="checkNodeName">Name of the check node.</param>
        /// <param name="Dropped">if set to <c>true</c> [dropped].</param>
        /// <param name="htDropAdd">The ht drop add hashtable, by ref.</param>
        /// <param name="Found">The found XML node.</param>
        /// <param name="xn">The xn XML node.</param>
        /// <param name="xmlDiffDoc">xmlDiffDoc is the XML diffgram doc.</param>
        private static void CheckColumnDependencies(string column_Name, string table_Name,
            string elementName, string parentNodeName, string childNodeName, string checkNodeName, bool Dropped,
            ref Hashtable htDropAdd, XmlNode Found, XmlNode xn, XmlDocument xmlDiffDoc)
        {
            XmlNode xnDefault = null;
            if (!Dropped)
            {
                // lookup any altered columns to see if there are index dependencies
                if (Found != null && Found.ParentNode != null && Found.ParentNode.SelectSingleNode(parentNodeName) != null)
                {
                    XmlNodeList xnList = Found.ParentNode.SelectNodes(parentNodeName);
                    if (xnList != null)
                    {
                        foreach (XmlNode xnIndex in xnList)
                        {
                            if (xnIndex.SelectSingleNode(childNodeName) != null)
                            {
                                string index_name = xnIndex.SelectSingleNode(childNodeName).InnerXml;
                                // look for the item in the source xml doc to ensure that we are not creating an invalid drop add
                                if (xn.ParentNode.SelectSingleNode(parentNodeName + "[" + childNodeName + "='" + index_name + "']") == null)
                                {
                                    continue;
                                }
                                // check for valid index keys and make sure we haven't already added this index as an xml node
                                if (xnIndex.SelectSingleNode(checkNodeName) != null && !htDropAdd.Contains(table_Name + "," + index_name))
                                {
                                    if (xnIndex.SelectSingleNode(checkNodeName).InnerText.IndexOf(column_Name) >= 0)
                                    {
                                        xnDefault = xmlDiffDoc.CreateNode(XmlNodeType.Element, elementName, xn.OwnerDocument.NamespaceURI);
                                        xnDefault.Attributes.Append((XmlAttribute)xmlDiffDoc.CreateNode(XmlNodeType.Attribute, "Action", xmlDiffDoc.NamespaceURI));
                                        xnDefault.InnerXml = xnIndex.InnerXml;
                                        xnDefault.Attributes["Action"].Value = "ReAdd";
                                        htDropAdd.Add(table_Name + "," + index_name, xnDefault);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // lookup any dropped columns to see if there are index dependencies
                if (xn != null && xn.ParentNode != null && xn.ParentNode.SelectSingleNode(parentNodeName) != null)
                {
                    XmlNodeList xnList = xn.ParentNode.SelectNodes(parentNodeName);
                    if (xnList != null)
                    {
                        foreach (XmlNode xnIndex in xnList)
                        {
                            if (xnIndex.SelectSingleNode(childNodeName) != null)
                            {
                                string index_name = xnIndex.SelectSingleNode(childNodeName).InnerXml;
                                // check for valid index keys and make sure we haven't already added this index as an xml node
                                if (xnIndex.SelectSingleNode(checkNodeName) != null && !htDropAdd.Contains(table_Name + "," + index_name))
                                {
                                    if (xnIndex.SelectSingleNode(checkNodeName).InnerText.IndexOf(column_Name) >= 0)
                                    {
                                        xnDefault = xmlDiffDoc.CreateNode(XmlNodeType.Element, elementName, xn.OwnerDocument.NamespaceURI);
                                        xnDefault.Attributes.Append((XmlAttribute)xmlDiffDoc.CreateNode(XmlNodeType.Attribute, "Action", xmlDiffDoc.NamespaceURI));
                                        xnDefault.InnerXml = xnIndex.InnerXml;
                                        xnDefault.Attributes["Action"].Value = "Drop";
                                        htDropAdd.Add(table_Name + "," + index_name, xnDefault);
                                    }
                                }
                                else if (xnIndex.SelectSingleNode(checkNodeName) != null && xnIndex.SelectSingleNode(checkNodeName).InnerText.IndexOf(column_Name) >= 0)
                                {
                                    // change the 'action' attribute's value of the existing DropAdd_Indexes xml node to 'drop'
                                    ((XmlNode)htDropAdd[table_Name + "," + index_name]).Attributes["Action"].Value = "Drop";
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursive procedure to examine and sort a SortedList of database objects with dependencies against a SortedList 
        /// of dependencies to determine the proper dependency order.
        /// </summary>
        /// <param name="_serverDB">SQL Server string used to select nodes.</param>
        /// <param name="sl">The SortedList of database objects to sort based on their dependencies.</param>
        /// <param name="slDep">The SortedList of dependencies.</param>
        /// <param name="startAt">The starting index of the SortedList containing the database objects.</param>
        /// <param name="loopcnt">The count of how many times the recursive method has called itself.</param>
        private static void RecurseDependencies(string _serverDB, ref SortedList sl, ref SortedList slDep, int startAt,
            ref int loopcnt)
        {
            bool changeFlag = false;
            int start = 0;
            loopcnt += 1;

            string firstPart = ((string)sl.GetKey(sl.Count - 1)).Split('!')[0];

            int highestCnt = Convert.ToInt32(firstPart.Split(',')[1]);
            for (int ii = startAt; ii < sl.Count; ii++)
            {
                string found = string.Empty;
                string oldobj = string.Empty;
                string oldobj1 = string.Empty;
                object obj = sl.GetKey(ii);
                firstPart = ((string)obj).Split('!')[0];
                int NumOfDependancies = Convert.ToInt32(firstPart.Split(',')[1]);
                // loop thru all dependant objects in dependancy list
                for (int zz = NumOfDependancies; zz > 0; zz--)
                {
                    int cnt = zz - 1;
                    string find = string.Format("{1:0000}!{0}", ((string)obj).Split('!')[1], cnt);
                    // look for dependant object(s) in dependancy list
                    int index = slDep.IndexOfKey(find);
                    // if dependant object(s) is/are found
                    if (index >= 0)
                    {
                        found = (string)slDep.GetByIndex(index);
                        for (int xx = (sl.Count * 10); xx >= 0; xx--)
                        {
                            for (int yy = highestCnt; yy >= 0; yy--)
                            {
                                string find2 = string.Format("{1:00000},{0}", string.Format("{1:0000}!{0}", found, yy), xx);
                                // look for dependant object in main list
                                int indexFound = sl.IndexOfKey(find2);
                                // if we find a match for the dependant object in the main list
                                if (indexFound >= 0)
                                {
                                    object obj1 = sl.GetKey(indexFound);
                                    string newobj = ((string)obj).Split('!')[1];
                                    string newobj1 = ((string)obj1).Split('!')[1];
                                    if (!newobj.Equals(oldobj) || !newobj1.Equals(oldobj1))
                                    {
                                        logger.Debug("\n{3}: Checking dependancy of {0} on {1}. Recursive Pass: {2}", newobj, newobj1, loopcnt, _serverDB);
                                        oldobj = newobj;
                                        oldobj1 = newobj1;
                                    }
                                    // and the dependant object is higher (later) up in the list
                                    if (indexFound >= ii)
                                    {
                                        changeFlag = true;
                                        firstPart = ((string)obj).Split('!')[0];
                                        int newCnt = Convert.ToInt32(firstPart.Split(',')[0]);
                                        newCnt = newCnt - 1 < 0 ? 0 : newCnt - 1;
                                        firstPart = ((string)obj1).Split('!')[0];
                                        int dependCount = Convert.ToInt32(firstPart.Split(',')[1]);
                                        // copy of object to move in list
                                        XmlNode copyOf = (XmlNode)sl.GetByIndex(indexFound);
                                        // remove dependant object from old place in list
                                        sl.Remove(obj1);
                                        // add dependant object back into list using changed key to place lower (earlier) in the list
                                        // than the object depending on it
                                        string newKey = string.Format("{1:00000},{0}", string.Format("{1:0000}!{0}", ((string)obj1).Split('!')[1], dependCount), newCnt);
                                        sl.Add(newKey, copyOf);
                                        start = sl.IndexOfKey(newKey);
                                        start = (start - 1 > 0 ? start - 1 : 0);
                                        break;
                                    }
                                }
                            }
                            if (changeFlag) break;
                            Sleep();
                        }
                        if (changeFlag) break;
                        Sleep();
                    }
                }
                if (changeFlag) break;
                Sleep();
            }
            if (changeFlag && loopcnt < (sl.Count * highestCnt))
            {
                RecurseDependencies(_serverDB, ref sl, ref slDep, start, ref loopcnt);
            }
        }


        /// <summary>
        /// Sorts the view, function, and stored procedure dependencies for the SQL schema read from a SQL server and database.  
        /// This method calls the RecurseDependencies method.
        /// </summary>
        /// <param name="_serverDB">SQL Server string used to select nodes.</param>
        /// <param name="NodePath">XPath string used to select nodes to recurse through.</param>
        /// <param name="NodeSelect">XPath string used to select individual node from XmlNodeList object generated using the 
        /// NodePath string.</param>
        /// <param name="xmlDoc">By ref the XmlDocument to sort the nodes selected using the NodePath string.</param>
        private static void SortDependencies(string _serverDB, string NodePath, string NodeSelect, ref XmlDocument xmlDoc)
        {
            nsmgr_Source = new XmlNamespaceManager(xmlDoc.NameTable);

            SortedList sl = new SortedList();
            SortedList slDep = new SortedList();

            XmlNode TopNode = xmlDoc.SelectSingleNode("/DataBase_Schema");
            XmlNodeList xNodeList = xmlDoc.SelectNodes(NodePath, nsmgr_Source);

            foreach (XmlNode xNode in xNodeList)
            {
                string SqlObjectName = xNode.ChildNodes[0].InnerXml;
                XmlNodeList xnlSource = xNode.SelectNodes(NodeSelect + SqlObjectName + "']", nsmgr_Source);
                // get dependencies names and add to sorted list
                int ii = 0;
                foreach (XmlNode xnSource in xnlSource)
                {
                    if (xnSource.ChildNodes[1] != null)
                    {
                        slDep.Add(string.Format("{1:0000}!{0}", SqlObjectName.ToLower(), ii), xnSource.ChildNodes[1].InnerXml.ToLower());
                        ii += 1;
                    }
                    Sleep();
                }
                if (ii > 0)
                {
                    sl.Add(string.Format("{1:0000}!{0}", SqlObjectName.ToLower(), ii), xNode.Clone());
                    TopNode.RemoveChild(xNode);
                }
                else
                {
                    sl.Add(string.Format("0000!{0}", SqlObjectName.ToLower()), xNode.Clone());
                    TopNode.RemoveChild(xNode);
                }
                Sleep();
            }
            SortedList sl2 = new SortedList();
            int zz = 0;
            for (int ii = 0; ii < sl.Count; ii++)
            {
                sl2.Add(string.Format("{1:00000},{0}", sl.GetKey(ii).ToString(), zz), sl.GetByIndex(ii));
                zz += 10;
            }
            // if sldep has count > 0 then we need to do some additional sorting
            if (slDep.Count > 0)
            {
                int start = 0;
                RecurseDependencies(_serverDB, ref sl2, ref slDep, 0, ref start);
            }
            for (int ii = 0; ii < sl2.Count; ii++)
            {
                XmlNode xn = (XmlNode)sl2.GetByIndex(ii);
                TopNode.AppendChild(xn);
            }
        }

        /// <summary>
        /// Finds any XMLnode that contains the text in the parameter findString
        /// </summary>
        /// <param name="xnl">XmlNodeList object</param>
        /// <param name="findString">Search string.</param>
        /// <param name="node">Node name format string, i.e.: &lt;nodename&gt;{0}&lt;/nodename&gt; </param>
        /// <returns>Returns the found XML node or a null if not found.</returns>
        public static XmlNode FindNode(XmlNodeList xnl, string findString, string node)
        {
            XmlNode _x = null;
            if (xnl != null && findString != null)
            {
                foreach (XmlNode xn in xnl)
                {
                    if (xn.OuterXml.IndexOf(string.Format(node, findString)) >= 0)
                    {
                        _x = xn;
                        break;
                    }
                }
            }
            return _x;
        }

        /// <summary>
        /// Causes the current thread to sleep for 20 iterations
        /// </summary>
        private static void Sleep()
        {
            if (_threaded != null && (bool)_threaded)
            {
                Thread.SpinWait(20);
            }
        }
    }

}
