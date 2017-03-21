using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Lewis.SST.Help
{
    /// <summary>
    /// Static Helptext class
    /// </summary>
    public static class HelpText
    {
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

/SqlObjectMask=[Byte] - a Hexadecimal value used to specify which SQL objects 
    are compared.  To compare more than one object you simply OR the values.
    By default, all objects are compared.  
    The value for all objects = [FF].
    TABLEs      = [80]
    UDDTs       = [40]
    RULEs       = [20]
    DEFAULTs    = [10]
    TRIGGERs    = [8]
    FUNCTIONs   = [4]
    VIEWs       = [2]
    SPROCs      = [1]

/CustomXSLT=[XSLT file name] - optionally used to transform XML output with a
    custom user supplied XSLT file.

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
        /// static property for help string
        /// </summary>
        public static string SSTUsageString
        {
            get { return _help; }
        }

        /// <summary>
        /// static method to display default help dialog
        /// </summary>
        public static void DisplayDefaultHelp()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string message = string.Format(_help, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion);
            About frmAbout = new About(a, message);
            frmAbout.ShowDialog();
        }

        /// <summary>
        /// Method displays default help with a string title parameter
        /// </summary>
        /// <param name="DialogTitle"></param>
        public static void DisplayDefaultHelp(string DialogTitle)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string message = string.Format(_help, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion);
            About frmAbout = new About(a, message);
            frmAbout.Text = DialogTitle;
            frmAbout.ShowDialog();
        }

        /// <summary>
        /// statis method to display passed in help text
        /// </summary>
        /// <param name="helpText"></param>
        /// <param name="DialogTitle"></param>
        public static void DisplayDefaultHelp(string DialogTitle, String helpText)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            About frmAbout = new About(a);
            frmAbout.Message = string.Format(helpText, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion);
            frmAbout.ShowDialog();
        }
    }
}
