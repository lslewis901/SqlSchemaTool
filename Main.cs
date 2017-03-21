using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Xml.XPath;

using Microsoft.XmlDiffPatch;


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
	class NamespaceDoc
	{// this class is used for nothing except Ndoc output for the namespace
	}

	/// <summary>
	/// Commandline tool to compare a source against a destination DB, create XML output, 
	/// Translate the XML output into a SQL script and perform an upgrade of the destination 
	/// using a SQL script.
	/// <para/>
	/// <remarks>The following are the parameters used for this tool:
	/// <code>
	/// 
	///	/? or /Help for this help message
	///
	///	PARAMETERS: (SQL Connections or Files to use)
	///	=============================================================================
	///	/Src_Server=[Source sql server]
	///	/Src_Database=[Source sql catalog or database] OR 
	///	/Src_Catalog=[Source sql catalog or database]
	///	/Src_Trusted - use trusted connection, user and pwd not required
	///	/Src_User=[Source sql server user login] OR 
	///	/Src_UID=[Source sql server user login]
	///	/Src_Password=[Source sql server user password] OR 
	///	/Src_PWD=[Source sql server user password]
	///	/Src_File=[output filename for source SQL server schema create script]
	///	/Src_Schema=[filename for existing source SQL server schema xml file]
	///
	///	When connecting to a SQL server:
	///		When wanting to just get the SQL create schema XML/SQL file(s)
	///		for a single SQL DB, only use the Src_?????? parameters or the
	///		/CreateXMLFile parameter.
	///
	///	Only use the Dest_?????? parameters when doing a compare.
	///
	///	/Dest_Server=[Destination sql server]
	///	/Dest_Database=[Destination sql catalog or database] OR 
	///	/Dest_Catalog=[Destination sql catalog or database]
	///	/Dest_Trusted - use trusted connection, user and pwd not required
	///	/Dest_User=[Destination sql server user login] OR 
	///	/Dest_UID=[Destination sql server user login]
	///	/Dest_Password=[Destination sql server user password] OR 
	///	/Dest_PWD=[Destination sql server user password]
	///	/Dest_File=[output filename for destination SQL server schema create script]
	///	/Dest_Schema=[filename for existing destination SQL server schema xml file]
	///
	///	When providing just a single schema or diff XML file,
	///	Use the following parameters:
	///
	///	/DiffXMLFile=[XML file name] - used by itself, this will apply the SQL diff
	///		transform against specified XML file.  Turns on the transform flag.
	///		Autogenerates SQL file name from XML input file name if none is
	///		provided via the next parameter '/DiffSQLFile'.
	///
	///	/DiffSQLFile=[SQL file name] - used with the '/DiffXMLFile' paramter, this 
	///		will assign the SQL file name used for the transformation output.
	///
	///	/CreateXMLFile=[XML file name] - used by itself, this will apply the SQL
	///		Create transform against specified XML file. Turns on the transform 
	///		flag.  	Autogenerates SQL file name from XML input file name if none
	///		is provided via the next parameter '/CreateSQLFile'.
	///
	///	/CreateSQLFile=[SQL file name] - used with the '/CreateXMLFile' paramter, this 
	///		will assign the SQL file name used for the transformation output.
	///
	///	OPTIONS: (changes operations of compare or output)
	///	=============================================================================
	///	/CompareSprocText - add this option to command line to compare source and
	///		dest stored procedure text line by line. Otherwise if both source and
	///		dest sprocs exists, the source sproc is always forced to 'alter
	///		procedure'.
	///
	///	/Transform - add this option to command line to perform translation of the
	///		XML file into a SQL file for creating or updating the SQL Destination
	///		DB. Requires some combination of Source and Dest SQL connections
	///		and/or XML Schema files.
	///		
	///	/Primary - add this option for force all tables to use the Primary 
	///		filegroup for CREATE TABLE or ALTER TABLE when serializing the schema.
	///		This option will also cause the compare to ignore FileGroups, since
	///		they all will be set to PRIMARY.
	/// </code>
	/// </remarks>
	/// <para/>
	/// <note type="caution">Passwords entered as a command parameter in conjunction with a batch file calling SST and the output redirector '&gt;' to some textfile can cause credentials to be stored in plain text.</note>
	/// </summary>
    class CommandLine
    {

        private static string _src_server;
        private static string _src_user;
        private static string _src_db;
        private static string _src_password;
        private static string _src_trusted = "false";
        private static string _src_file;
        private static string _src_schema;

        private static string _dest_server;
        private static string _dest_user;
        private static string _dest_db;
        private static string _dest_password;
        private static string _dest_trusted = "false";
        private static string _dest_file;
        private static string _dest_schema;

        private static string _diffXMLFile;
        private static string _createXMLFile;
        private static string _diffName;
        private static string _createName;

        private static bool _CompareSprocs = false;
        private static bool _Translate = false;
        private static bool _Primary = false;


        /// <summary>
        /// const value for the help text that is displayed when the /? parameter is used. 
        /// </summary>
        private const string _help =
@"SST Version: {0}

/? or /Help for this help message

PARAMETERS: (SQL Connections or Files to use)
=============================================================================
/Src_Server=[Source sql server]
/Src_Database=[Source sql catalog or database] OR 
/Src_Catalog=[Source sql catalog or database]
/Src_Trusted - use trusted connection, user and pwd not required
/Src_User=[Source sql server user login] OR 
/Src_UID=[Source sql server user login]
/Src_Password=[Source sql server user password] OR 
/Src_PWD=[Source sql server user password]
/Src_File=[output filename for source SQL server schema create script]
/Src_Schema=[filename for existing source SQL server schema xml file]

When connecting to a SQL server:
	When wanting to just get the SQL create schema XML/SQL file(s)
	for a single SQL DB, only use the Src_?????? parameters or the
	/CreateXMLFile parameter. 

	Only use the Dest_?????? parameters when doing a compare.

/Dest_Server=[Destination sql server]
/Dest_Database=[Destination sql catalog or database] OR 
/Dest_Catalog=[Destination sql catalog or database]
/Dest_Trusted - use trusted connection, user and pwd not required
/Dest_User=[Destination sql server user login] OR 
/Dest_UID=[Destination sql server user login]
/Dest_Password=[Destination sql server user password] OR 
/Dest_PWD=[Destination sql server user password]
/Dest_File=[output filename for destination SQL server schema create script]
/Dest_Schema=[filename for existing destination SQL server schema xml file]

When providing just a single schema or diff XML file,
	Use the following parameters:

/DiffXMLFile=[XML file name] - used by itself, this will apply the SQL diff
	transform against specified XML file.  Turns on the transform flag.
	Autogenerates SQL file name from XML input file name if none is
	provided via the next parameter '/DiffSQLFile'.

/DiffSQLFile=[SQL file name] - used with the '/DiffXMLFile' paramter, this 
	will assign the SQL file name used for the transformation output.

/CreateXMLFile=[XML file name] - used by itself, this will apply the SQL
	Create transform against specified XML file. Turns on the transform 
	flag.  	Autogenerates SQL file name from XML input file name if none
	is provided via the next parameter '/CreateSQLFile'.

/CreateSQLFile=[SQL file name] - used with the '/CreateXMLFile' paramter, this 
	will assign the SQL file name used for the transformation output.

OPTIONS: (changes operations of compare or output)
=============================================================================
/CompareSprocText - add this option to command line to compare source and
	dest stored procedure text line by line. Otherwise if both source and
	dest sprocs exists, the source sproc is always forced to 'alter
	procedure'.

/Transform - add this option to command line to perform translation of the
	XML file into a SQL file for creating or updating the SQL Destination
	DB. Requires some combination of Source and Dest SQL connections
	and/or XML Schema files.

/Primary - add this option for force all tables to use the Primary 
	filegroup for CREATE TABLE or ALTER TABLE when serializing the schema.
	This option will also cause the compare to ignore FileGroups, since
	they all will be set to PRIMARY.
";

        /// <summary>
        /// The main entry point for the application.
        /// Handles processing of the command line args.
        /// </summary>
        /// <param name="args">Passes arguments string array into the appropriate methods and functions contained within the class.</param>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // TODO: add additional commandline args functions
                foreach (string arg in args)
                {
                    if (arg.Split('=').Length == 2)
                    {
                        string command = arg.Split('=')[0].Trim().ToLower();
                        string argument = arg.Split('=')[1].Trim();
                        if (command.IndexOf("/src_server") >= 0)
                        {
                            _src_server = argument;
                        }
                        else if (command.IndexOf("/src_user") >= 0 || command.IndexOf("/src_uid") >= 0)
                        {
                            _src_user = argument;
                        }
                        else if (command.IndexOf("/src_password") >= 0 || command.IndexOf("/src_pwd") >= 0)
                        {
                            _src_password = argument;
                        }
                        else if (command.IndexOf("/src_database") >= 0 || command.IndexOf("/src_catalog") >= 0)
                        {
                            _src_db = argument;
                        }
                        else if (command.IndexOf("/src_file") >= 0)
                        {
                            _src_file = argument;
                            _Translate = true;
                        }
                        else if (command.IndexOf("/src_schema") >= 0)
                        {
                            _src_schema = argument;
                        }
                        if (command.IndexOf("/dest_server") >= 0)
                        {
                            _dest_server = argument;
                        }
                        else if (command.IndexOf("/dest_user") >= 0 || command.IndexOf("/dest_uid") >= 0)
                        {
                            _dest_user = argument;
                        }
                        else if (command.IndexOf("/dest_password") >= 0 || command.IndexOf("/dest_pwd") >= 0)
                        {
                            _dest_password = argument;
                        }
                        else if (command.IndexOf("/dest_database") >= 0 || command.IndexOf("/dest_catalog") >= 0)
                        {
                            _dest_db = argument;
                        }
                        else if (command.IndexOf("/dest_file") >= 0)
                        {
                            _dest_file = argument;
                            _Translate = true;
                        }
                        else if (command.IndexOf("/dest_schema") >= 0)
                        {
                            _dest_schema = argument;
                        }
                        else if (command.IndexOf("/diffxmlfile") >= 0)
                        {
                            _diffXMLFile = argument;
                            _Translate = true;
                        }
                        else if (command.IndexOf("/diffsqlfile") >= 0)
                        {
                            _diffName = argument;
                            _Translate = true;
                        }
                        else if (command.IndexOf("/createxmlfile") >= 0)
                        {
                            _createXMLFile = argument;
                            _Translate = true;
                        }
                        else if (command.IndexOf("/createsqlfile") >= 0)
                        {
                            _createName = argument;
                            _Translate = true;
                        }
                    }
                    if (arg.Split('=').Length == 1 && arg.Length > 0)
                    {
                        if (arg.IndexOf("/?") >= 0 || arg.ToLower().IndexOf("/help") >= 0)
                        {
                            Assembly a = Assembly.GetExecutingAssembly();
                            Console.WriteLine("{0}", string.Format(_help, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion));
                            return;
                        }
                        else if (arg.ToLower().IndexOf("/src_trusted") >= 0)
                        {
                            _src_trusted = "true";
                        }
                        else if (arg.ToLower().IndexOf("/dest_trusted") >= 0)
                        {
                            _dest_trusted = "true";
                        }
                        else if (arg.ToLower().IndexOf("/comparesproctext") >= 0)
                        {
                            _CompareSprocs = true;
                        }
                        else if (arg.ToLower().IndexOf("/transform") >= 0)
                        {
                            _Translate = true;
                        }
                        else if (arg.ToLower().IndexOf("/translate") >= 0) // this is for my benefit since I keep typing it in wrong on the commandline.
                        {
                            _Translate = true;
                        }
                        else if (arg.ToLower().IndexOf("/primary") >= 0)
                        {
                            _Primary = true;
                        }
                    }
                }
            }
            else
            {
                Assembly a = Assembly.GetExecutingAssembly();
                Console.WriteLine("{0}", string.Format(_help, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion));
                return;
            }

            if (_src_trusted.ToLower() == "true")
            {
                _src_user = null;
                _src_password = null;
            }
            else
            {
                if (_src_server != null && _src_db != null && (_src_user == null || _src_password == null))
                {
                    Console.WriteLine("You must enter the trusted argument or a user name and password for the source SQL server in the commandline arguments.");
                    return;
                }
            }

            if (_dest_trusted.ToLower() == "true")
            {
                _dest_user = null;
                _dest_password = null;
            }
            else
            {
                if (_dest_server != null && _dest_db != null && (_dest_user == null || _dest_password == null))
                {
                    Console.WriteLine("You must enter the trusted argument or a user name and password for the destination SQL server in the commandline arguments.");
                    return;
                }
            }
            if (_src_server != null && _src_db != null && _dest_server != null && _dest_db != null)
            {
                if (_src_server.ToLower() == _dest_server.ToLower() && _src_db.ToLower() == _dest_db.ToLower())
                {
                    Console.WriteLine("Thats silly, you can't compare the same SQL server and database against itself!");
                    return;
                }
            }

            if (_src_schema != null && _dest_schema != null)
            {
                if (_src_schema.ToLower() == _dest_schema.ToLower())
                {
                    Console.WriteLine("Thats silly, you can't compare the same SQL server and database XML file against itself!");
                    return;
                }
            }

            // TODO: add the ability to exclude database objects by name or regex string
            // TODO: add error checks for file.exists on all passed in file names for files that are supposed to already exist
            if (_src_server != null && _src_db != null && _src_schema == null)
            {
                _src_schema = SerializeDB(_src_server, _src_db, _src_user, _src_password, _src_file, _Translate, _Primary);
            }

            if (_dest_server != null && _dest_db != null && _dest_schema == null)
            {
                _dest_schema = SerializeDB(_dest_server, _dest_db, _dest_user, _dest_password, _dest_file, _Translate, _Primary);
            }

            if (_src_schema != null && _dest_schema != null)
            {
                string diffName = null;
                // some logic to create default diff XML file name
                if (_diffXMLFile == null || _diffXMLFile.Length == 0)
                {
                    if (_src_server != null && _src_db != null && _dest_server != null && _dest_db != null)
                    {
                        diffName = string.Format("{0}_{1}_DIFF_{2}_{3}_SCHEMA.xml", _src_server.Replace("\\", "_"), _src_db, _dest_server.Replace("\\", "_"), _dest_db);
                    }
                    else
                    {
                        string src = _src_schema.ToLower().Replace("_schema", "").Replace(".xml", "");
                        string dest = _dest_schema.ToLower().Replace("_schema", "").Replace(".xml", "");
                        diffName = string.Format("{0}_DIFF_{1}_SCHEMA.xml", src, dest);
                    }
                }
                else
                {
                    diffName = _diffXMLFile;
                }
                string SQLfile = diffName.ToLower().Replace(".xml", ".sql");
                if (!SQLfile.EndsWith(".sql")) SQLfile += ".sql";
                // do the compare
                CompareSchema(_src_schema, _dest_schema, diffName, SQLfile, _CompareSprocs, _Translate);
                return;
            }

            if (_diffXMLFile != null && _diffXMLFile.Length > 0 && _src_schema == null && _dest_schema == null && _Translate)
            {
                // perform garbage collection to free up memory
                GC.Collect();

                Console.WriteLine("Please wait. Beginning SQL transformation of DiffGram XML...");
                string diffName = string.Empty;
                if (_diffName == null)
                {
                    diffName = _diffXMLFile.ToLower().Replace(".xml", ".sql");
                }
                else
                {
                    diffName = _diffName;
                }
                if (!diffName.EndsWith(".sql")) diffName += ".sql";

                SQLTransform(_diffXMLFile, "Lewis.SST.Xslt.Diff_DB_Objs.xslt", diffName);
                Console.WriteLine("SQL Diff Schema has been saved to " + diffName + ".");
                return;
            }

            if (_createXMLFile != null && _createXMLFile.Length > 0 && _src_schema == null && _dest_schema == null && _Translate)
            {
                // perform garbage collection to free up memory
                GC.Collect();

                Console.WriteLine("Please wait. Beginning SQL transformation of schema XML...");
                string createName = string.Empty;
                if (_createName == null)
                {
                    createName = _createXMLFile.ToLower().Replace(".xml", ".sql");
                }
                else
                {
                    createName = _createName;
                }
                if (!createName.EndsWith(".sql")) createName += ".sql";

                SQLTransform(_createXMLFile, "Lewis.SST.Xslt.Create_DB_Objs.xslt", createName);
                Console.WriteLine("SQL Create Schema has been saved to " + createName + ".");
                return;
            }

            if (_src_schema != null && _src_schema.Length > 0 && _createXMLFile == null && _dest_schema == null && _Translate)
            {
                // perform garbage collection to free up memory
                GC.Collect();

                Console.WriteLine("Please wait. Beginning SQL transformation of schema XML...");
                string createName = _src_schema.ToLower().Replace(".xml", ".sql");
                if (_src_file != null && _src_file.Length > 0)
                {
                    createName = _src_file;
                }
                if (!createName.EndsWith(".sql")) createName += ".sql";

                SQLTransform(_src_schema, "Lewis.SST.Xslt.Create_DB_Objs.xslt", createName);
                Console.WriteLine("SQL Create Schema has been saved to " + createName + ".");
                return;
            }
        }

    }
}
