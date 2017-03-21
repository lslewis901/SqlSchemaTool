using Lewis.SST.DTSPackageClass;

using NLog;

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Lewis.SST
{
	/// <summary>
	/// DTS command line tool class.  Uses the DTS package class to serialize/deserialize DTS packages to/from SQL servers, to/from DTS structured storage files, 
	/// or to/from serialized DTS XML documents.
	/// </summary>
	class DTSCommandLine
	{
		private static string gsComputername;
		private static string vuser;
		private static string vpass;

		private static bool ErrorFlag = false;
		private static bool debugflag = false;
		private static DTSPackage2.authTypeFlags AuthType = DTSPackage2.authTypeFlags.Default;
        private static Logger logger = LogManager.GetLogger("Lewis.SST.DTSCommandLine");

		/// <summary>
		/// The main entry point for the command line application.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		[STAThread]
		static void Main(string[] args)
		{
			Assembly a = Assembly.GetExecutingAssembly();
			if (args.Length == 0)
			{
				About frmAbout = new About(a);
				frmAbout.ShowDialog();
			}
			else
			{
				ProcessCommandLine(args, a);
			}
		}

		/// <summary>
		/// Processes the command line arguments.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		/// <param name="a">Assembly.</param>
		private static void ProcessCommandLine(string [] args, Assembly a)
		{
			string vLogFileName = null;
			string vserver = null;
			string vSaveFileName = null;
			string vLoadFileName = null;
			string vPackageGUID = null;
			string vVersionGUID = null;
			string vEncryptPassword = null;
			bool PkgFlag = false;
			bool Sqlflag = false;
			bool removeflag = false;
			bool saveflag = false;
			bool loadflag = false;
			bool DTSflag1 = false;
			bool DTSflag2 = false;
			bool TestFlag = false;
			bool verflag = false;

			string vpkgname = "Replace This Name";

			for (int ii = 0; ii < args.Length; ii++)
			{
				switch (args[ii].ToLower())
				{
					case "help":
					case "/help":
					case "/?":
					{
						About frmAbout = new About(a);
						frmAbout.ShowDialog();
						return;
					}
					case "/h":
					{
						ErrorFlag = true;
						break;
					}
					case "/d":
					{
						debugflag = true;
						break;
					}
					case "/r":
					{
						removeflag = true;
						break;
					}
					case "/f":
					{
						if (ii + 1 < args.Length)
						{
							vLogFileName = args[ii + 1];
							if (!vLogFileName.ToLower().EndsWith("\\"))
							{
								vLogFileName = vLogFileName + "DTSPackage.log";
							}
							ii ++;
						}
						break;
					}
					case "/x":
					{
						saveflag = true;
						removeflag = false;
						if (ii + 1 < args.Length)
						{
							vSaveFileName = args[ii + 1];
							if (!vSaveFileName.ToLower().EndsWith(".xml"))
							{
								if (!vSaveFileName.ToLower().EndsWith(".dts"))
								{
									vSaveFileName += ".xml";
								}
								else
								{
									DTSflag2 = true;
								}
							}
							ii ++;
						}
						break;
					}
					case "/l":
					{
						loadflag = true;
						removeflag = false;
						if (ii + 1 < args.Length)
						{
							vLoadFileName = args[ii + 1];
							if (!vLoadFileName.ToLower().EndsWith(".xml"))
							{
								if (!vLoadFileName.ToLower().EndsWith(".dts"))
								{
									vLoadFileName += ".xml";
								}
								else
								{
									DTSflag1 = true;
								}
							}
							ii ++;
						}
						break;
					}
					case "/i": //package ID guid
					{
						if (ii + 1 < args.Length)
						{
							vPackageGUID = args[ii + 1];
							if (!vPackageGUID.EndsWith("}"))
							{
								vPackageGUID = vPackageGUID + "}";
							}
							if (!vPackageGUID.StartsWith("{"))
							{
								vPackageGUID = "{" + vPackageGUID;
							}
							ii ++;
						}
						break;
					}
					case "/v": //package version guid
					{
						if (ii + 1 < args.Length)
						{
							vVersionGUID = args[ii + 1];
							if (!vVersionGUID.EndsWith("}"))
							{
								vVersionGUID = vVersionGUID + "}";
							}
							if (!vVersionGUID.StartsWith("{"))
							{
								vVersionGUID = "{" + vVersionGUID;
							}
							ii ++;
						}
						break;
					}
					case "/w":
					{
						AuthType = DTSPackage2.authTypeFlags.Trusted;
						break;
					}
					case "/t":
					{
						TestFlag = true;
						break;
					}
					case "/u":
					{
						if (ii + 1 < args.Length)
						{
							vuser = args[ii + 1];
							ii ++;
						}
						break;
					}
					case "/p":
					{
						if (ii + 1 < args.Length)
						{
							vpass = args[ii + 1];
							ii ++;
						}
						break;
					}
					case "/s":
					{
						Sqlflag = true;
						if (ii + 1 < args.Length)
						{
							vserver = args[ii + 1];
							ii ++;
						}
						break;
					}
					case "/n":
					{
						PkgFlag = true;
						if (ii + 1 < args.Length)
						{
							vpkgname = args[ii + 1];
							ii ++;
						}
						break;
					}
					case "/e":
					{
						if (ii + 1 < args.Length)
						{
							vEncryptPassword = args[ii + 1];
							ii ++;
						}
						break;
					}
					case "/pi":
					{
						verflag = true;
						break;
					}
				}
			} // end of for loop that parses the passed in args array
			if (TestFlag)
			{
				ErrorFlag = false;
				debugflag = true;
			}
			if (vuser == null)
			{
				vuser = "sa"; //set default user
			}

			if (Sqlflag && vserver != null)
			{
				if (!ErrorFlag && vserver.IndexOf("/") >= 0 )
				{
                    logger.Error("SQL Server Name [" + vserver + "] contains a forward slash!\n\nThe DTS installation program can not connect.\n\nTo connect to a SQL instance, the SQL Server Instance string\nmust have a back slash.");
                    return;
					//MessageBox.Show("SQL Server Name [" + vserver + "] contains a forward slash!\n\nThe DTS installation program can not connect.\n\nTo connect to a SQL instance, the SQL Server Instance string\nmust have a back slash.", "SQL CONNECTION ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					gsComputername = vserver;
					if (!ErrorFlag && debugflag)
					{
						string msgStr = null;
						if (AuthType == DTSPackage2.authTypeFlags.Default)
						{
							msgStr = "SQL Server Name: " + gsComputername + "\nUser ID: " + vuser + "\nPassword: not shown";
						}
						else
						{
							msgStr = "SQL Server Name: " + gsComputername + "\nSQL Login is using Windows Authentication.";
						}
                        logger.Debug(msgStr);
						//MessageBox.Show(msgStr, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			else
			{
				gsComputername = GetLocalMachineName();
				if (!ErrorFlag && debugflag)
				{
					string msgStr = null;
					if (AuthType == DTSPackage2.authTypeFlags.Default)
					{
						msgStr = "SQL Server Name: " + gsComputername + "\nUser ID: " + vuser + "\nPassword: not shown";
					}
					else
					{
						msgStr = "SQL Server Name: " + gsComputername + "\nSQL Login is using Windows Authentication.";
					}
                    logger.Debug(msgStr);
					//MessageBox.Show(msgStr, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			if (PkgFlag && vpkgname != null)
			{
				if (!ErrorFlag && debugflag)
				{
                    logger.Debug("CommandLine DTS Package Name: " + vpkgname);
					//MessageBox.Show("CommandLine DTS Package Name: " + vpkgname, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else
			{
				if (!ErrorFlag)
				{
                    logger.Info("CommandLine DTS Package Name: " + vpkgname);
					//MessageBox.Show("CommandLine DTS Package Name: " + vpkgname, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				return;
			}

			if (TestFlag)
			{
				string testresult = null;
				if (TestConnection(gsComputername, vuser, vpass))
				{
					testresult = "Successful";
				}
				else
				{
					testresult = "Failed";
				}
				if (!ErrorFlag)
				{
                    logger.Info(testresult + " Connection Test.");
					//MessageBox.Show(testresult + " Connection Test.", "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				return;
			}

			DTSPackage2 oPackage = InitializeDTSPackage(
				vpkgname,
				(vLogFileName != null ? vLogFileName : a.Location + vpkgname + ".log"),
				System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileDescription);

			oPackage.Authentication = AuthType;

			if (!removeflag && !saveflag && !loadflag && !DTSflag1 && !DTSflag2 && !verflag)
			{
				if (!ErrorFlag)
				{
                    logger.Info("No valid command line options were entered for loading or saving the package.");
					//MessageBox.Show("No valid command line options were entered for loading or saving the package.", "PACKAGE OPTIONS ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return;
			}
			if (!removeflag && !saveflag && verflag && !DTSflag1)
			{
				string packageInfo = oPackage.GetPackageVersionInfo(gsComputername, vpkgname, vuser, vpass, AuthType, false);
				if (!ErrorFlag)
				{
					About frmAbout = new About();
					frmAbout.ShowText(packageInfo);
					frmAbout.Dispose();
				}
				return;
			}
			if (!removeflag && !saveflag && verflag && DTSflag1)
			{
				string packageInfo = oPackage.GetPackageVersionInfo(vLoadFileName);
				if (!ErrorFlag)
				{
					About frmAbout = new About();
					frmAbout.ShowText(packageInfo);
					frmAbout.Dispose();
				}
				return;
			}
			if (!removeflag && !saveflag && loadflag && !DTSflag1)
			{
				// call Create DTS on SQL Server from XML file
				LoadPackageFromXMLDoc(oPackage, vLoadFileName, vEncryptPassword);
				return;
			}
			if (!removeflag && !saveflag && loadflag && DTSflag1)
			{
				// call Create DTS on SQL Server from DTS file
				LoadPackageFromDTSFile(oPackage, vLoadFileName, vEncryptPassword, vPackageGUID, vVersionGUID);
				return;
			}
			if (!removeflag && saveflag && loadflag && !DTSflag1 && DTSflag2)
			{
				// call convert XML file to DTS File
				LoadPackageFromXMLDoc(oPackage, vSaveFileName, vEncryptPassword);
				return;
			}
			if (!removeflag && saveflag && loadflag && DTSflag1 && !DTSflag2)
			{
				// call convert DTS file to XML file
				LoadPackageFromDTSFile(oPackage, vLoadFileName, vSaveFileName, vEncryptPassword, vPackageGUID, vVersionGUID);
				return;
			}
			if (!removeflag && saveflag && !loadflag)
			{
				// call load Package from SQL Server
				LoadPackageFromSQLServer(oPackage, vEncryptPassword ,vPackageGUID, vVersionGUID);

				// call Serialize as XML file
				SerializePackageAsXMLFile(oPackage, vSaveFileName);
				return;
			}
			if(removeflag)
			{
				// call Remove DTS
				RemovePackageFromSQLServer(oPackage, vPackageGUID, vVersionGUID);
				return;
			}
		}

		/// <summary>
		/// Initializes the DTS package.
		/// </summary>
		/// <param name="name">The DTS package name.</param>
		/// <param name="logfile">The DTS logfile name.</param>
		/// <param name="description">The DTS package description.</param>
		/// <returns></returns>
		private static DTSPackage2 InitializeDTSPackage(string name, string logfile, string description)
		{
			DTSPackage2 oPackage = null;
			try
			{
				oPackage = new DTSPackage2(name, logfile, description);
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package Error is:" + ex.Message);
					//MessageBox.Show("Package Error is:" + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			return oPackage;
		}

		/// <summary>
		/// Removes the DTS package from SQL server.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="packGUID">The DTS package GUID.</param>
		/// <param name="verGUID">The DTS version GUID.</param>
		private static void RemovePackageFromSQLServer(DTSPackage2 oPackage, string packGUID, string verGUID)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					string msgstr = null;
					if (packGUID != null && verGUID != null)
					{
						msgstr = "Remove all versions of the DTS Package: ";
					}
					else
					{
						msgstr = "Remove version " + verGUID + " of the DTS Package: ";
					}
					DialogResult retval = MessageBox.Show("Remove DTS Package: " + oPackage.Name , "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						oPackage.Remove(gsComputername, vuser, vpass, packGUID, verGUID);
					}
				}
				else
				{
					oPackage.Remove(gsComputername, vuser, vpass, packGUID, verGUID);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package remove error: " + ex.Message);
					//MessageBox.Show("Package remove error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Loads the DTS package from an XML doc into the DTS Package object class.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="fileName">Name of the XML doc file.</param>
		/// <param name="encryptPass">The encrypt password.</param>
		private static void LoadPackageFromXMLDoc(DTSPackage2 oPackage, string fileName, string encryptPass)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					DialogResult retval = MessageBox.Show("Load XML File: [" + fileName + "] to the SQL server: [" + gsComputername + "]?", "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						// read XML file
						oPackage.Load(fileName);
						// save package to SQL server
						oPackage.Save(gsComputername, vuser, vpass, encryptPass);
						if (!ErrorFlag && debugflag)
						{
                            logger.Debug("Loaded DTS Package: " + oPackage.Name + ", from the XML file: " + fileName);
							//MessageBox.Show("Loaded DTS Package: " + oPackage.Name + ", from the XML file: " + fileName, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);				
						}
					}
				}
				else
				{
					// read XML file
					oPackage.Load(fileName);
					// save package to SQL server
					oPackage.Save(gsComputername, vuser, vpass, encryptPass);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package load error: " + ex.Message);
					//MessageBox.Show("Package load error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Loads the DTS package from an XML doc into the DTS Package object class.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="DTSfileName">Name of the DTS file.</param>
		/// <param name="XMLFileName">Name of the XML doc file.</param>
		/// <param name="encryptPass">The encrypt password.</param>
		private static void LoadPackageFromXMLDoc(DTSPackage2 oPackage, string DTSfileName, string XMLFileName, string encryptPass)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					DialogResult retval = MessageBox.Show("Convert XML File: [" + XMLFileName + "] to the DTS File: [" + DTSfileName + "]?", "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						// read XML file
						oPackage.Load(XMLFileName);
						// save package to DTS File
						oPackage.Save(DTSfileName, encryptPass);
						if (!ErrorFlag && debugflag)
						{
                            logger.Debug("Loaded DTS Package: " + oPackage.Name + ", from the XML file: " + XMLFileName + "] to the DTS File: [" + DTSfileName + "]");
							//MessageBox.Show("Loaded DTS Package: " + oPackage.Name + ", from the XML file: " + XMLFileName + "] to the DTS File: [" + DTSfileName + "]", "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);				
						}
					}
				}
				else
				{
					// read XML file
					oPackage.Load(XMLFileName);
					// save package to DTS File
					oPackage.Save(DTSfileName, encryptPass);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package conversion error: " + ex.Message);
					//MessageBox.Show("Package conversion error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Loads the DTS package from DTS structured storage file into the DTS Package object class.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="fileName">Name of the DTS structured storage file.</param>
		/// <param name="encryptPass">The encrypt password.</param>
		/// <param name="packageGUID">The DTS package GUID.</param>
		/// <param name="versionGUID">The DTS version GUID.</param>
		private static void LoadPackageFromDTSFile(DTSPackage2 oPackage, string fileName, string encryptPass, string packageGUID, string versionGUID)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					DialogResult retval = MessageBox.Show("Load DTS File: [" + fileName + "] to the SQL server: [" + gsComputername + "]?", "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						// read DTS file
						oPackage.Load(fileName, encryptPass, packageGUID, versionGUID);
						// save package to SQL server
						oPackage.Save(gsComputername, vuser, vpass, encryptPass);
						if (!ErrorFlag && debugflag)
						{
                            logger.Debug("Loaded DTS Package: " + oPackage.Name + ", from the DTS file: " + fileName);
							//MessageBox.Show("Loaded DTS Package: " + oPackage.Name + ", from the DTS file: " + fileName, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);				
						}
					}
				}
				else
				{
					// read DTS file
					oPackage.Load(fileName, encryptPass, packageGUID, versionGUID);
					// save package to SQL server
					oPackage.Save(gsComputername, vuser, vpass, encryptPass);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package load error: " + ex.Message);
					//MessageBox.Show("Package load error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Loads the DTS package from DTS structured storage file into the DTS Package object class.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="DTSfileName">Name of the DTS file.</param>
		/// <param name="XMLFileName">Name of the XML doc file.</param>
		/// <param name="encryptPass">The DTS encrypt password.</param>
		/// <param name="packageGUID">The DTS package GUID.</param>
		/// <param name="versionGUID">The DTS version GUID.</param>
		private static void LoadPackageFromDTSFile(DTSPackage2 oPackage, string DTSfileName, string XMLFileName, string encryptPass, string packageGUID, string versionGUID)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					DialogResult retval = MessageBox.Show("Convert DTS File: [" + DTSfileName + "] to the XML File: [" + XMLFileName + "]?", "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						// read DTS file
						oPackage.Load(DTSfileName, encryptPass, packageGUID, versionGUID);
						// save package to XML file
						oPackage.Save(XMLFileName);
						if (!ErrorFlag && debugflag)
						{
                            logger.Debug("Successfully converted DTS Package: " + oPackage.Name + ", from the DTS file: " + DTSfileName + "] to the XML File: [" + XMLFileName + "]");
							//MessageBox.Show("Successfully converted DTS Package: " + oPackage.Name + ", from the DTS file: " + DTSfileName + "] to the XML File: [" + XMLFileName + "]", "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);				
						}
					}
				}
				else
				{
					// read DTS file
					oPackage.Load(DTSfileName, encryptPass, packageGUID, versionGUID);
					// save package to XML file
					oPackage.Save(XMLFileName);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package conversion error: " + ex.Message);
					//MessageBox.Show("Package conversion error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Loads the DTS package from SQL server into the DTS Package object class.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="encryptPass">The DTS encrypt password.</param>
		/// <param name="packGUID">The DTS packackage GUID.</param>
		/// <param name="verGUID">The DTS version GUID.</param>
		private static void LoadPackageFromSQLServer(DTSPackage2 oPackage, string encryptPass, string packGUID, string verGUID)
		{
			try
			{
				oPackage.Load(gsComputername, vuser, vpass, encryptPass, packGUID, verGUID);
				if (!ErrorFlag && debugflag)
				{
                    logger.Debug("Loaded DTS Package: " + oPackage.Name);
					//MessageBox.Show("Loaded DTS Package: " + oPackage.Name, "DEBUG HELP", MessageBoxButtons.OK, MessageBoxIcon.Information);				
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("Package load error: " + ex.Message);
					//MessageBox.Show("Package load error: " + ex.Message, "PACKAGE ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Serializes the DTS package as an XML file.
		/// </summary>
		/// <param name="oPackage">The DTS package object.</param>
		/// <param name="filename">The XML doc filename.</param>
		private static void SerializePackageAsXMLFile(DTSPackage2 oPackage, string filename)
		{
			try
			{
				if (!ErrorFlag && debugflag)
				{
					DialogResult retval = MessageBox.Show("Save DTS Package: " + 
						oPackage.Name + " as XML file: " + 
						filename, "DEBUG HELP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (retval == DialogResult.Yes)
					{
						oPackage.Save(filename);
					}
				}
				else
				{
					oPackage.Save(filename);
				}
			}
			catch(Exception ex)
			{
				if (!ErrorFlag)
				{
                    logger.Error("XML Serializer Error is: " + ex.Message);
					//MessageBox.Show("XML Serializer Error is: " + ex.Message, "SERIALIZER ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Gets the name of the local machine.
		/// </summary>
		/// <returns>Returns the name of the local machine.</returns>
		private static string GetLocalMachineName()
		{
			return Environment.MachineName;
		}

		/// <summary>
		/// Tests the SQL server connection.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="userID">The user ID.</param>
		/// <param name="password">The password.</param>
		/// <returns>Returns true of connection ok or false if connection fails.</returns>
		private static bool TestConnection(string server, string userID, string password)
		{
			bool retval = false;
			string strSQL;
			string security;

			if (AuthType == DTSPackage2.authTypeFlags.Trusted)
			{
				security = "Integrated Security=yes";
			}
			else
			{
				security = "User ID = " + userID + ";Password = " + password;
			}

			strSQL = "Data Source = " + server + ";Initial Catalog = Master;" + security + ";Connection Timeout = 60;";

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{				
				using (SqlConnection cn = new SqlConnection(strSQL))
				{
					cn.Open();
					cn.Close();
					retval = true;
				}
			}
			catch(Exception ex)
			{
				if (! ErrorFlag)
				{
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    logger.Error("Error Opening Data Connection: " + ex.Message);
                    //MessageBox.Show("Error Opening Data Connection: " + ex.Message, "SQL CONNECTION ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
			return retval;
		}
	}
}
