using DTS;
using DTSCustTasks;

using NLog;

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Lewis.SST.DTSPackageClass
{
	/// <summary>
	/// DTSPackage class for wrapping DTS package2 functionality.
	/// helpful information came from: http://www.sqldev.net/dts.htm
	/// additional information was found here: http://www.developer.com/db/article.php/3502036
	/// </summary>
	/// 
	public class DTSPackage2
	{
		#region Variables and enums
        private static Logger logger = LogManager.GetLogger("Lewis.SST.DTSPackageClass");

        /// <summary>
		/// The connection type enum
		/// </summary>
		public enum ConnectionType : int
		{
			/// <summary>
			/// SQLOLEDB connection type.
			/// </summary>
			SQLOLEDB,
			/// <summary>
			/// Flat file connection type.
			/// </summary>
			FlatFile			
		}

		/// <summary>
		/// the authentication type enum
		/// </summary>
		public enum authTypeFlags : int
		{
			/// <summary>
			/// Default authentication type. Sets type to SQL server login.
			/// </summary>
			Default = DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_Default,
			/// <summary>
			/// Trusted authentication type.  Sets type to local Windows login.
			/// </summary>
			Trusted = DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_UseTrustedConnection
		}

		private DTS.DTSSQLServerStorageFlags authType = DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_Default;

		private string [] ConnTypes = new string [] {"SQLOLEDB.1", "DTSFlatFile.1"};

		private DTS.Package2 oPackage;

		private DTSColumns _sourceColumns;
		private DTSColumns _destinationColumns;
		private Hashtable htConstraints = new Hashtable();
		#endregion

		#region Class constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DTSPackage2"/> class.
		/// </summary>
		public DTSPackage2()
		{
			try
			{
				oPackage = new DTS.Package2();
				initializeDefaults();
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DTSPackage2"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="logFile">The log file.</param>
		/// <param name="description">The description.</param>
		public DTSPackage2(string name, string logFile, string description)
		{
			try
			{
				oPackage =  new DTS.Package2();
				initializeDefaults();
				oPackage.Name = name;
				oPackage.LogFileName = logFile;
				oPackage.Description = description;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public void Dispose()
		{
			oPackage = null;
		}

		#endregion

		#region Public methods
		// returns the connection id
		// could make this into an override, if we add more connection types - that would be best
		/// <summary>
		/// Adds the DTS connection to a DTS package object.
		/// </summary>
		/// <param name="connectionName">Name of the connection.</param>
		/// <param name="dataSource">The data source.</param>
		/// <param name="catalog">The catalog.</param>
		/// <param name="user">The user.</param>
		/// <param name="password">The password.</param>
		/// <param name="ct">The ct.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="rowDelimiter">The row delimiter.</param>
		/// <param name="colDelimiter">The col delimiter.</param>
		/// <returns></returns>
		public int AddConnection(string connectionName, string dataSource, string catalog, string user, string password, 
			ConnectionType ct, int timeout, string rowDelimiter, string colDelimiter)
		{
			int retval = -1;
			try
			{
				DTS.Connection oConnection = oPackage.Connections.New(ConnTypes[Convert.ToInt32(ct)]);
				oConnection.ID = oPackage.Connections.Count;
				oConnection.ConnectionTimeout = timeout;
				oConnection.Name = connectionName;
				oConnection.UserID = user;
				oConnection.Password = password;
				oConnection.DataSource = dataSource;
				oConnection.Catalog = catalog;
				oConnection.UseTrustedConnection = false;
				oConnection.UseDSL = false;
				oConnection.Reusable = true;
				oConnection.ConnectImmediate = false;
				oConnection.ConnectionProperties.Item("Data Source").Value = dataSource;

				switch (Convert.ToInt32(ct))
				{
					case 0:	// sqloledb connection
					{		  
						oConnection.ConnectionProperties.Item("User ID").Value = user;
						oConnection.ConnectionProperties.Item("Password").Value = password;
						oConnection.ConnectionProperties.Item("Initial Catalog").Value = catalog;
						oConnection.ConnectionProperties.Item("Application Name").Value = "FFADTS";
						oConnection.ConnectionProperties.Item("Workstation ID").Value = Environment.MachineName;
						oConnection.ConnectionProperties.Item("Auto Translate").Value = true;
						oConnection.ConnectionProperties.Item("Persist Security Info").Value = true;
						oConnection.ConnectionProperties.Item("Locale Identifier").Value = 1033;
						oConnection.ConnectionProperties.Item("Prompt").Value = 4;
						oConnection.ConnectionProperties.Item("Use Procedure for Prepare").Value = 1;
						oConnection.ConnectionProperties.Item("Packet Size").Value = 4096;
						break;
					}
					case 1:	// flat file connection
					{
						oConnection.ConnectionProperties.Item("Row Delimiter").Value = rowDelimiter; //chr(13) + chr(10)
						oConnection.ConnectionProperties.Item("Column Delimiter").Value = colDelimiter;
						oConnection.ConnectionProperties.Item("First Row Column Name").Value = false;
						oConnection.ConnectionProperties.Item("Mode").Value = 1;
						oConnection.ConnectionProperties.Item("File Format").Value = 1;
						oConnection.ConnectionProperties.Item("File Type").Value = 1;
						oConnection.ConnectionProperties.Item("Skip Rows").Value = 0;
						oConnection.ConnectionProperties.Item("Text Qualifier").Value = '"';
						oConnection.ConnectionProperties.Item("Number of Column").Value = 0;
						oConnection.ConnectionProperties.Item("Max characters per delimited column").Value = 255;
						break;
					}
				}
				oPackage.Connections.Add(oConnection);
				retval = oConnection.ID;
				oConnection = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
			return retval;
		}

		/// <summary>
		/// Adds the DTS step to a DTS package object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		public void AddStep(string name, string description)
		{
			AddStep(name, description, null, null);
		}

		/// <summary>
		/// Adds the DTS step to a DTS package object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="axScript">The ax script.</param>
		/// <param name="functionName">Name of the function.</param>
		public void AddStep(string name, string description, string axScript, string functionName)
		{
			try
			{
				DTS.Step2 oStep = (DTS.Step2)oPackage.Steps.New();
				oStep.Name = name;
				oStep.Description = description;
				oStep.ExecutionStatus = DTS.DTSStepExecStatus.DTSStepExecStat_Waiting;
				oStep.TaskName = name;
				oStep.ActiveXScript = axScript;
				oStep.FunctionName = functionName;
				oStep.ScriptLanguage = "VBScript";
				oStep.CommitSuccess = false;
				oStep.RollbackFailure = false;
				oStep.AddGlobalVariables = true;
				oStep.CloseConnection = false;
				oStep.ExecuteInMainThread = false;
				oStep.IsPackageDSORowset = false;
				oStep.JoinTransactionIfPresent = false;
				oStep.DisableStep = false;
				oStep.RelativePriority = DTS.DTSStepRelativePriority.DTSStepRelativePriority_Normal;
				oPackage.Steps.Add(oStep);
				oStep = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		// add a constraint for each step
		/// <summary>
		/// Adds the constaint to the DTS step.
		/// </summary>
		/// <param name="sourceStepName">Name of the source step.</param>
		/// <param name="constrainedStepName">Name of the constrained step.</param>
		public void AddConstaintToStep(string sourceStepName, string constrainedStepName)
		{
			try
			{
				DTS.Step2 oStep = (DTS.Step2)oPackage.Steps.Item(sourceStepName);
				DTS.PrecedenceConstraint oPrecConstraint = oStep.PrecedenceConstraints.New(constrainedStepName);
				oPrecConstraint.StepName = constrainedStepName;
				oPrecConstraint.PrecedenceBasis = DTS.DTSStepPrecedenceBasis.DTSStepPrecedenceBasis_ExecStatus;
				oPrecConstraint.Value = 4;
				oStep.PrecedenceConstraints.Add(oPrecConstraint);
				oPrecConstraint = null;
				oStep = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		// add a general task - need a task per each step
		// returns the custom task
		/// <summary>
		/// Adds the general DTS task to the DTS package object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <returns>returns the custom task</returns>
		public DTS.CustomTask AddTask(string name, string description)
		{
			DTS.CustomTask retval;
			try
			{
				DTS.Task oTask = (DTS.Task)oPackage.Tasks.New("DTSDataPumpTask");	
				oTask.Name = name;
				oTask.Description = description;
				oPackage.Tasks.Add(oTask);
				retval = oTask.CustomTask;
				oTask = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}			
			return retval;
		}

		// add a SQL Exec task 
		/// <summary>
		/// Adds the SQL Exec task to the DTS package object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="SQLQuery">The SQL query.</param>
		/// <param name="connectionID">The connection ID.</param>
		/// <param name="timeOut">The time out.</param>
		public void AddTask(string name, string description, string SQLQuery, int connectionID, int timeOut)
		{
			try
			{
				DTS.Task oTask = (DTS.Task)oPackage.Tasks.New("DTSExecuteSQLTask");	
				oTask.Name = name;
				oTask.Description = description;

				DTS.ExecuteSQLTask2 oCustomTask = (DTS.ExecuteSQLTask2)oTask.CustomTask;

				oCustomTask.Name = name;
				oCustomTask.Description = description;
				oCustomTask.SQLStatement = SQLQuery;
				oCustomTask.CommandTimeout = timeOut;
				oCustomTask.ConnectionID = connectionID;

				oPackage.Tasks.Add(oTask);
				oCustomTask = null;
				oTask = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		// add a datapump task 
		/// <summary>
		/// Adds the datapump task to the DTS package object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="sourceConnectID">The source connect ID.</param>
		/// <param name="sourceObjectName">Name of the source object.</param>
		/// <param name="destinationConnectID">The destination connect ID.</param>
		/// <param name="destinationObjectName">Name of the destination object.</param>
		/// <param name="exceptionFileName">Name of the exception file.</param>
		public void AddTask(string name, string description, int sourceConnectID, string sourceObjectName, 
			int destinationConnectID, string destinationObjectName, string exceptionFileName)
		{
			try
			{
				DTS.Task oTask = (DTS.Task)oPackage.Tasks.New("DTSDataPumpTask");	
				oTask.Name = name;
				oTask.Description = description;

				// add a reference for the task to the custom task object
				DTS.DataPumpTask2 oCustomTask = (DTS.DataPumpTask2)oTask.CustomTask;
				oCustomTask.Name = name;
				oCustomTask.Description = description;
				oCustomTask.SourceConnectionID = sourceConnectID;
				oCustomTask.SourceObjectName = sourceObjectName;
				oCustomTask.DestinationConnectionID = destinationConnectID;
				oCustomTask.DestinationObjectName = destinationObjectName;
				oCustomTask.ExceptionFileName = exceptionFileName;
				oCustomTask.ExceptionFileColumnDelimiter = "|";
				oCustomTask.ExceptionFileRowDelimiter = Convert.ToString(Convert.ToChar(13) + Convert.ToChar(10));
				oCustomTask.UseFastLoad = true;
				oCustomTask.AllowIdentityInserts = false;
				oCustomTask.ProgressRowCount = 1000;
				oCustomTask.MaximumErrorCount = 0;
				oCustomTask.FetchBufferSize = 1;
				oCustomTask.InsertCommitSize = 0;
				oCustomTask.FirstRow = 0;
				oCustomTask.LastRow = 0;
				oCustomTask.FastLoadOptions = DTS.DTSFastLoadOptions.DTSFastLoad_CheckConstraints;

				// add source and destination columns here
				for(int ii = 0; ii < destinationColumns.Count; ii++)
				{
					DTSColumn destCol = _destinationColumns[ii];
					DTSColumn srcCol = _sourceColumns[ii];
					createColumnTransformation(srcCol, destCol, ref oCustomTask, ii);
				}

				oPackage.Tasks.Add(oTask);
				oCustomTask = null;
				oTask = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Gets the DTS package step by name.
		/// </summary>
		/// <param name="stepName">Name of the step.</param>
		/// <returns>Returns the DTS step object</returns>
		public DTS.Step2 GetPackageStep(string stepName)
		{
			return (DTS.Step2)oPackage.Steps.Item(stepName);
		}

		/// <summary>
		/// Gets the DTS package step by step number.
		/// </summary>
		/// <param name="stepNo">The step no.</param>
		/// <returns>Returns the DTS step object.</returns>
		public DTS.Step2 GetPackageStep(int stepNo)
		{
			return (DTS.Step2)oPackage.Steps.Item(stepNo);
		}

		/// <summary>
		/// Gets the DTS package task by name.
		/// </summary>
		/// <param name="taskName">Name of the task.</param>
		/// <returns>Returns the DTS task object.</returns>
		public DTS.Task GetPackageTask(string taskName)
		{
			return oPackage.Tasks.Item(taskName);
		}

		/// <summary>
		/// Gets the DTS package task by number.
		/// </summary>
		/// <param name="taskNo">The task no.</param>
		/// <returns>Returns the DTS task object.</returns>
		public DTS.Task GetPackageTask(int taskNo)
		{
			return oPackage.Tasks.Item(taskNo);
		}

		/// <summary>
		/// Gets the DTS package connection by name.
		/// </summary>
		/// <param name="connectionName">Name of the connection.</param>
		/// <returns>Returns the DTS connection object.</returns>
		public DTS.Connection2 GetPackageConnection(string connectionName)
		{
			return (DTS.Connection2)oPackage.Connections.Item(connectionName);
		}

		/// <summary>
		/// Gets the DTS package connection by number.
		/// </summary>
		/// <param name="connectionID">The connection ID.</param>
		/// <returns>Returns the DTS connection object.</returns>
		public DTS.Connection2 GetPackageConnection(int connectionID)
		{
			return (DTS.Connection2)oPackage.Connections.Item(connectionID);
		}

		/// <summary>
		/// Gets the DTS package version info.
		/// </summary>
		/// <param name="UNCFile">The UNC file name of the DTS package Structured Storage file to read.</param>
		/// <returns>Returns version information as a string.</returns>
		public string GetPackageVersionInfo(string UNCFile)
		{
			int verMajor;
			int verMinor;
			int verBuild;
			string verComments = null;
			string retval = null;

			oPackage.GetDTSVersionInfo(out verMajor, out verMinor, out verBuild, out verComments);
			retval = "SQL Version information: " + verMajor.ToString() + "." + verMinor.ToString() + "." + verBuild.ToString() + "\n";
			DTS.SavedPackageInfos spis = oPackage.GetSavedPackageInfos(UNCFile);
			foreach(DTS.SavedPackageInfo spi in spis)
			{
				retval = retval + "\nPackageName: " + spi.PackageName;
				retval = retval + "\nPackageID: " + spi.PackageID;
				retval = retval + "\nVersionID: " + spi.VersionID;
				retval = retval + "\nEncrypted: " + spi.IsVersionEncrypted.ToString();
				retval = retval + "\nDescription: " + spi.Description;
				retval = retval + "\nCreation Date: " + spi.PackageCreationDate;
				retval = retval + "\nLast Saved Date: " + spi.VersionSaveDate;
				retval = retval + "\n\n";
			}
			return retval;
		}

		/// <summary>
		/// Gets the DTS package version info.
		/// </summary>
		/// <param name="serverName">Name of the server.</param>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="userName">Name of the user.</param>
		/// <param name="passWord">The pass word.</param>
		/// <param name="Auth">The auth.</param>
		/// <param name="LatestPackage">if set to <c>true</c> [latest package].</param>
		/// <returns>Returns version information as a string.</returns>
		public string GetPackageVersionInfo(string serverName, string packageName, string userName, string passWord, authTypeFlags Auth, bool LatestPackage)
		{
			int verMajor;
			int verMinor;
			int verBuild;
			string verComments = null;
			string retval = null;

			oPackage.GetDTSVersionInfo(out verMajor, out verMinor, out verBuild, out verComments);
			retval = "SQL Version information: " + verMajor.ToString() + "." + verMinor.ToString() + "." + verBuild.ToString() + "\n";
			DTS.Application app = new DTS.ApplicationClass();
			DTS.PackageSQLServer pss = app.GetPackageSQLServer(serverName, userName, passWord, ConvertFromAuthTypeFlag(Auth));
			foreach(DTS.PackageInfo pi in pss.EnumPackageInfos("", LatestPackage, ""))
			{
				if (packageName == pi.Name)
				{
					retval = retval + "\nPackageName: " + pi.Name;
					retval = retval + "\nPackageID: " + pi.PackageID;
					retval = retval + "\nVersionID: " + pi.VersionID;
					retval = retval + "\nOwner: " + pi.Owner;
					retval = retval + "\nDescription: " + pi.Description;
					retval = retval + "\nCreation Date: " + pi.CreationDate;
					retval = retval + "\nPackage Data Size: " + pi.PackageDataSize.ToString();
					retval = retval + "\n\n";
				}
			}
			return retval;
		}

		/// <summary>
		/// Loads the DTS package from the DTS specified structured storage file.
		/// </summary>
		/// <param name="UNCFile">The UNC file.</param>
		/// <param name="encryptPass">The encrypt pass.</param>
		/// <param name="packageGUID">The package GUID.</param>
		/// <param name="versionGUID">The version GUID.</param>
		public void Load(string UNCFile, string encryptPass, string packageGUID, string versionGUID)
		{
			object obj = null;
			try
			{
				oPackage.LoadFromStorageFile(UNCFile, encryptPass, packageGUID, versionGUID, oPackage.Name, ref obj); 
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Loads the DTS package object from the specified SQL server.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="user">The user.</param>
		/// <param name="password">The password.</param>
		/// <param name="encryptPass">The encrypt pass.</param>
		/// <param name="packageGUID">The package GUID.</param>
		/// <param name="versionGUID">The version GUID.</param>
		public void Load(string server, string user, string password, string encryptPass, string packageGUID, string versionGUID)
		{
			object obj = null;
			try
			{
				oPackage.LoadFromSQLServer(server, user, password, authType, encryptPass, packageGUID, versionGUID, oPackage.Name, ref obj);
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Loads the DTS package object from the specified serialized DTS XML file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public void Load(string fileName)
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(fileName);
				createPackageFromXMLDoc(xDoc);
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Saves the persisted DTS package object to the specified SQL server.
		/// </summary>
		/// <param name="SQLServerName">Name of the SQL server.</param>
		/// <param name="user">The user.</param>
		/// <param name="password">The password.</param>
		/// <param name="encryptPass">The encrypt pass.</param>
		public void Save(string SQLServerName, string user, string password, string encryptPass)
		{
			object obj = null;
			try
			{
				oPackage.SaveToSQLServer(SQLServerName, user, password, authType, encryptPass, null, null, ref obj, false );
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Saves the persisted DTS package object to the specified serialized DTS XML file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public void Save(string fileName)
		{
			try
			{
				XmlDocument xDoc = createXMLDocFromPackage();
				XmlSerializer serializer = new XmlSerializer(typeof(XmlDocument));
				TextWriter writer = new StreamWriter(fileName);
				serializer.Serialize(writer, xDoc);
				writer.Close();
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Saves the persisted DTS package object to the DTS specified structured storage file.
		/// </summary>
		/// <param name="UNCName">Name of the UNC.</param>
		/// <param name="encryptPass">The encrypt pass.</param>
		public void Save(string UNCName, string encryptPass)
		{
			object obj = null;
			try
			{
				oPackage.SaveToStorageFile(UNCName, encryptPass, null, ref obj, false);
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Removes the specified DTS package from the specified SQL server.
		/// </summary>
		/// <param name="SQLServerName">Name of the SQL server.</param>
		/// <param name="user">The user.</param>
		/// <param name="password">The password.</param>
		/// <param name="packageGUID">The package GUID.</param>
		/// <param name="versionGUID">The version GUID.</param>
		public void Remove(string SQLServerName, string user, string password, string packageGUID, string versionGUID)
		{
			try
			{
				oPackage.RemoveFromSQLServer(SQLServerName, user, password, authType, packageGUID, versionGUID, oPackage.Name);
			}
			catch( Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region Public class properties
		/// <summary>
		/// Gets or sets the authentication type.
		/// </summary>
		/// <value>The authentication type.</value>
		public authTypeFlags Authentication
		{
			get
			{
				if (authType == DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_Default)
					return authTypeFlags.Default;
				else
					return authTypeFlags.Trusted;
			}
			set
			{	
				if (value == authTypeFlags.Default)
					authType = DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_Default;
				else
					authType = DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_UseTrustedConnection;
			}
		}

		/// <summary>
		/// Gets or sets the DTS Package name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return oPackage.Name;
			}
			set
			{
				oPackage.Name = value;
			}
		}

		/// <summary>
		/// Gets the DTS Package version.
		/// </summary>
		/// <value>The version.</value>
		public string Version
		{
			get
			{
				return oPackage.VersionID;
			}
		}

		/// <summary>
		/// Gets the DTS package ID.
		/// </summary>
		/// <value>The ID.</value>
		public string ID
		{
			get
			{
				return oPackage.PackageID;
			}
		}

		/// <summary>
		/// Gets or sets the DTS package source columns.
		/// </summary>
		/// <value>The source columns.</value>
		public DTSColumns sourceColumns
		{
			get
			{
				return _sourceColumns;
			}
			set 
			{
				_sourceColumns = value;
			}
		}

		/// <summary>
		/// Gets or sets the DTS package destination columns.
		/// </summary>
		/// <value>The destination columns.</value>
		public DTSColumns destinationColumns
		{
			get
			{
				return _destinationColumns;
			}
			set 
			{
				_destinationColumns = value;
			}
		}
		#endregion

		#region Helper Methods
		/// <summary>
		/// Initializes the defaults for the DTS package object.
		/// </summary>
		private void initializeDefaults()
		{
			try
			{
				oPackage.UseOLEDBServiceComponents = true;
				oPackage.AutoCommitTransaction = true;
				oPackage.UseTransaction = true;
				oPackage.ExplicitGlobalVariables = true;
				oPackage.FailOnError = false;
				oPackage.WriteCompletionStatusToNTEventLog = false;
				oPackage.PackagePriorityClass = DTS.DTSPackagePriorityClass.DTSPriorityClass_Normal;
				oPackage.MaxConcurrentSteps = 4;
				oPackage.RepositoryMetadataOptions = 0;
				oPackage.LineageOptions = 0;
				oPackage.TransactionIsolationLevel = DTS.DTSIsolationLevel.DTSIsoLevel_ReadCommitted;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Creates the column transformation object.
		/// </summary>
		/// <param name="sourceColumn">The source column.</param>
		/// <param name="destinationColumn">The destination column.</param>
		/// <param name="customTask">The custom task.</param>
		/// <param name="fieldOrdinal">The field ordinal.</param>
		private void createColumnTransformation(DTSColumn sourceColumn, DTSColumn destinationColumn, 
			ref DTS.DataPumpTask2 customTask, int fieldOrdinal)
		{
			try
			{
				DTS.Transformation2 oTransformation = (DTS.Transformation2)
					customTask.Transformations.New("DTS.DataPumpTransformCopy.1");
				// create new source and destination DTS.columns
				oTransformation.SourceColumns.Add(oTransformation.SourceColumns.New(sourceColumn.Name, fieldOrdinal));	
				oTransformation.DestinationColumns.Add(oTransformation.DestinationColumns.New(destinationColumn.Name, fieldOrdinal));
				oTransformation = null;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Populates the DTS package object from a loaded XML doc that is a serialized DTS package.
		/// </summary>
		/// <param name="xDoc">The XML doc.</param>
		private void createPackageFromXMLDoc ( XmlDocument xDoc )
		{
			XmlNode xRoot = xDoc.SelectSingleNode("descendant::Package");
			string pName = oPackage.Name;
			htConstraints.Clear();
			ReadXMLNodes( xRoot, oPackage, xRoot.Name );
			// do steps constraints last
			foreach (DTS.Step2 s in oPackage.Steps)
			{
				object obj = htConstraints[s.Name];
				ReadXMLNodes((XmlNode)obj, s, "PrecedenceConstraints");
			}
			oPackage.Name = pName;
		}

		/// <summary>
		/// Reads the XML nodes and converts them into DTS Package objects.
		/// </summary>
		/// <param name="xNode">The XML node to parse.</param>
		/// <param name="obj">The obj to deserialize.</param>
		/// <param name="t">The object type.</param>
		/// <param name="parent">The parent node name.</param>
		private void ReadXMLNodes ( XmlNode xNode, object obj, Type t, string parent )
		{
			foreach ( XmlElement xEl in xNode.ChildNodes )
			{
				try
				{
					PropertyInfo pi = t.GetProperty( xEl.Name );
					switch ( parent )
					{
						case  "Transformations":
						{
							if ( xEl.Name.StartsWith("DTSTransformation_") )
							{
								if (xEl.SelectSingleNode("Properties/TransformServerID") != null)
								{
									string newDPTypeID = xEl.SelectSingleNode("Properties/TransformServerID").InnerText;
									PropertyInfo pi1 = t.GetProperty("Transformations");
									Type ti = pi1.PropertyType;
									object transObj = pi1.GetValue(obj, new object[0]);
									DTS.Transformation2 dt2 = (DTS.Transformation2)((DTS.Transformations)transObj).New(newDPTypeID);
									ReadXMLNodes(xEl, dt2, typeof(DTS.Transformation2), "Transformation");
									((DTS.Transformations)transObj).Add(dt2);
								}
							}
							break;
						}
						case "Transformation":
						{
							switch ( xEl.Name )
							{
								case "TransformServerProperties":
								case "Properties":
								{
									SetDTSProperties((DTS.Properties)pi.GetValue(obj, new object[0]), (XmlNode)xEl);
									break;
								}
								case "DestinationColumns":
								{
									ReadXMLNodes(xEl, obj, typeof(DTS.Transformation2), xEl.Name);
									break;
								}
								case "SourceColumns":
								{
									ReadXMLNodes(xEl, obj, typeof(DTS.Transformation2), xEl.Name);
									break;
								}
								default:
								{
									SetProperty(obj, pi, xEl.InnerText);
									break;
								}
							}
							break;
						}
						case "DestinationColumns":
						{
							if ( xEl.Name.StartsWith("Col_") )
							{
								DTS.Transformation2 dt2 = (DTS.Transformation2)(obj);
								DTS.Column col = dt2.DestinationColumns.New(
									xEl.SelectSingleNode("Name").InnerText, 
									Convert.ToInt32(xEl.SelectSingleNode("Ordinal").InnerText));
								ReadXMLNodes(xEl, col, typeof(DTS.Column), xEl.Name);
								SetDTSProperties( col.Properties, xEl.SelectSingleNode("Properties") );
								dt2.DestinationColumns.Add(col);
							}
							break;
						}
						case "SourceColumns":
						{
							if ( xEl.Name.StartsWith("Col_") )
							{
								DTS.Transformation2 dt2 = (DTS.Transformation2)(obj);
								DTS.Column col = dt2.SourceColumns.New(
									xEl.SelectSingleNode("Name").InnerText, 
									Convert.ToInt32(xEl.SelectSingleNode("Ordinal").InnerText));
								ReadXMLNodes(xEl, col, typeof(DTS.Column), xEl.Name);
								SetDTSProperties( col.Properties, xEl.SelectSingleNode("Properties") );
								dt2.SourceColumns.Add(col);
							}
							break;
						}
						case "Assignments":
						{
							if ( xEl.Name.StartsWith("DynamicPropertyAssignment_") )
							{
								DTSCustTasks.DynamicPropertiesTask dyn = (DTSCustTasks.DynamicPropertiesTask)(obj);
								DTSCustTasks.DynamicPropertiesTaskAssignment a = dyn.Assignments().New();
								ReadXMLNodes(xEl, a, typeof(DTSCustTasks.DynamicPropertiesTaskAssignment), xEl.Name);
								dyn.Assignments().Add(a);
							}
							break;
						}
						default:
						{
							SetProperty(obj, pi, xEl.InnerText);
							break;
						}
					}
				}
				catch(Exception ex)
				{
					throw new Exception(ex.Message + "\nOcurred in node: " + xEl.Name, ex);
				}
			}
		}

		/// <summary>
		/// Reads the XML nodes and converts them into DTS Package objects.
		/// </summary>
		/// <param name="xNode">The XML node.</param>
		/// <param name="obj">The obj to deserialize.</param>
		/// <param name="parent">The parent node name.</param>
		private void ReadXMLNodes ( XmlNode xNode, object obj, string parent )
		{
			foreach ( XmlElement xEl in xNode.ChildNodes )
			{
				try
				{
					Type t = obj.GetType();
					PropertyInfo pi = t.GetProperty( xEl.Name);
					if ( pi != null ) // this check tells us that we need to create the object
					{
						switch ( xEl.Name )
						{
							case "GlobalVariables":
							case "Connections":
							case "Steps":
							case "Tasks":
							{
								ReadXMLNodes((XmlNode)xEl, pi.GetValue(obj, new object[0]), xEl.Name);
								break;
							}
							case "Properties":
							{
								SetDTSProperties((DTS.Properties)pi.GetValue(obj, new object[0]), (XmlNode)xEl);
								break;
							}
							default:
							{
								SetProperty(obj, pi, xEl.InnerText);
								break;
							}
						}
					}
					else
					{
						switch ( parent )
						{
							case "GlobalVariables":
							{
								if ( xEl.Name.StartsWith("GlobalVariable_") )
								{
									string name = xEl.SelectSingleNode("Name").InnerText;
									DTS.GlobalVariable2 g = (DTS.GlobalVariable2)oPackage.GlobalVariables.New(name);
									ReadXMLNodes((XmlNode)xEl, g, typeof(DTS.GlobalVariable2), xEl.Name);
									SetDTSProperties(g.Properties, ((XmlNode)xEl).SelectSingleNode("Properties"));
									oPackage.GlobalVariables.Add((DTS.GlobalVariable2)g);
								}
								break;
							}
							case "Connections":
							{
								if ( xEl.Name.StartsWith("Connection_") )
								{
									string providerID = xEl.SelectSingleNode("ProviderID").InnerText;
									DTS.Connection2 c = (DTS.Connection2)oPackage.Connections.New(providerID);
									ReadXMLNodes((XmlNode)xEl, c, typeof(DTS.Connection2), xEl.Name);
									SetDTSProperties(c.Properties, ((XmlNode)xEl).SelectSingleNode("Properties"));
									try
									{
										SetOLEDBProperties(c.ConnectionProperties, ((XmlNode)xEl).SelectSingleNode("ConnectionProperties"));
									}
									catch(Exception ex)
									{
                                        // output error using logger
                                        logger.Error("Connection: " + xEl.Name + " \n" + ex.Message + " \n" + ex.StackTrace);
									}
									oPackage.Connections.Add((DTS.Connection)c);
								}
								break;
							}
							case "Steps":
							{
								if ( xEl.Name.StartsWith("Step_") )
								{
									DTS.Step2 s = (DTS.Step2)oPackage.Steps.New();
									ReadXMLNodes((XmlNode)xEl, s, typeof(DTS.Step2), xEl.Name);
									SetDTSProperties(s.Properties, ((XmlNode)xEl).SelectSingleNode("Properties"));
									// build hashtable for constraints
									htConstraints.Add(s.Name, xEl.SelectSingleNode("PrecedenceConstraints"));
									//ReadXMLNodes((XmlNode)xEl.SelectSingleNode("PrecedenceConstraints"), (DTS.Step2)s, "PrecedenceConstraints");
									oPackage.Steps.Add((DTS.Step2)s);
								}
								break;
							}
							case "PrecedenceConstraints":
							{
								if ( xEl.Name.StartsWith("PrecedenceConstraint_") )
								{
									DTS.Step2 s = (DTS.Step2)obj;
									string stepName = xEl.SelectSingleNode("StepName").InnerText;
									DTS.PrecedenceConstraint c = s.PrecedenceConstraints.New(stepName);
									ReadXMLNodes( (XmlNode)xEl, c, typeof(DTS.PrecedenceConstraint), parent);
									SetDTSProperties(c.Properties, ((XmlNode)xEl).SelectSingleNode("Properties"));
									s.PrecedenceConstraints.Add(c);
								}
								break;
							}
							case "Tasks":
							{
								if ( xEl.Name.StartsWith("Task_") )
								{
									string taskID = xEl.SelectSingleNode("CustomTaskID").FirstChild.InnerText;
									Type custTaskType = GetCustomTaskFromID(taskID);
									DTS.Task t1 = oPackage.Tasks.New(taskID);
									ReadXMLNodes((XmlNode)xEl, t1, typeof(DTS.Task), xEl.Name);
									SetDTSProperties(t1.Properties, ((XmlNode)xEl).SelectSingleNode("Properties"));
									// set the custom task properties
									ReadXMLNodes((XmlNode)xEl.SelectSingleNode("CustomTaskID/CustomTaskProperties"), t1.CustomTask, custTaskType, "CustomTaskProperties");
									// set the custom task properties.properties
									SetDTSProperties(t1.CustomTask.Properties, ((XmlNode)xEl).SelectSingleNode("CustomTaskID/CustomTaskProperties/Properties"));
									// set the custom task OLE DB properties
									try
									{
										PropertyInfo pi1 = (custTaskType).GetProperty("CommandProperties");
										DTS.OleDBProperties op1 = (DTS.OleDBProperties)pi1.GetValue(t1.CustomTask, new object[0]);
										SetOLEDBProperties(op1, ((XmlNode)xEl).SelectSingleNode("CustomTaskID/CustomTaskProperties/CommandProperties"));
									}
									catch(Exception ex)
									{
                                        // error logging using nlog
										string err = ex.Message;
                                        logger.Error("OLEDB Error in Task: " + xEl.Name + " \n" + ex.Message + " \n" + ex.StackTrace);
									}
									try
									{
										PropertyInfo pi1 = (custTaskType).GetProperty("SourceCommandProperties");
										DTS.OleDBProperties op1 = (DTS.OleDBProperties)pi1.GetValue(t1.CustomTask, new object[0]);
										SetOLEDBProperties(op1, ((XmlNode)xEl).SelectSingleNode("CustomTaskID/CustomTaskProperties/SourceCommandProperties"));
									}
									catch(Exception ex)
									{
                                        // error logging using nlog
										string err = ex.Message;
                                        logger.Error("OLEDB Error in Task: " + xEl.Name + " \n" + ex.Message + " \n" + ex.StackTrace);
									}
									try
									{
										PropertyInfo pi1 = (custTaskType).GetProperty("DestinationCommandProperties");
										DTS.OleDBProperties op1 = (DTS.OleDBProperties)pi1.GetValue(t1.CustomTask, new object[0]);
										SetOLEDBProperties(op1, ((XmlNode)xEl).SelectSingleNode("CustomTaskID/CustomTaskProperties/DestinationCommandProperties"));
									}
									catch(Exception ex)
									{
                                        // error logging using nlog
										string err = ex.Message;
                                        logger.Error("OLEDB Error in Task: " + xEl.Name + " \n" + ex.Message + " \n" + ex.StackTrace);
									}
									switch (taskID)
									{
										case "DTSDataPumpTask":
										case "DTSDataDrivenQueryTask":
										case "DTSParallelDataPumpTask":
										{
											ReadXMLNodes((XmlNode)xEl.SelectSingleNode("CustomTaskID/CustomTaskProperties/Transformations"), t1.CustomTask, custTaskType, "Transformations");
											break;
										}
										case "DTSDynamicPropertiesTask":
										{
											ReadXMLNodes((XmlNode)xEl.SelectSingleNode("CustomTaskID/DynamicPropertyAssignments"), t1.CustomTask, typeof(DTSCustTasks.DynamicPropertiesTask), "Assignments");
											break;
										}
									}
									oPackage.Tasks.Add(t1);
								}
								break;
							}
						}
					}
				}
				catch(Exception ex)
				{
					throw new Exception(ex.Message + "\nOcurred in node: " + xEl.Name, ex);
				}
			}
		}

		/// <summary>
		/// Creates/serializes an XML doc from the persisted DTS package object.
		/// </summary>
		/// <returns>The XML doc.</returns>
		private XmlDocument createXMLDocFromPackage()
		{
			XmlDocument xDoc = new XmlDocument();
			
			XmlNode xRoot = xDoc.CreateNode(XmlNodeType.Element, "DTS_File", "");
			XmlAttribute xAttr;
			xAttr = xDoc.CreateAttribute("Package_Name", "");
			xAttr.Value = oPackage.Name;
			xRoot.Attributes.Append(xAttr);

			// create top level Package_Property nodes
			int zz = 0;
			CreateChildNodes(xDoc, ref xRoot, typeof(DTS.Package2Class), "Package", oPackage);

			// create GlobalVariables nodes
			XmlNode xGlobalVariables = xDoc.CreateNode(XmlNodeType.Element, "GlobalVariables", "");
			zz = 0;
			foreach( DTS.GlobalVariable2 gv2 in oPackage.GlobalVariables )
			{
				Type t = typeof(DTS.GlobalVariable2);
				string nodeName = "GlobalVariable_" + zz.ToString();
				CreateChildNodes(xDoc, ref xGlobalVariables, t, nodeName, gv2);
				zz ++;
			}
			xRoot.SelectSingleNode("descendant::Package").RemoveChild(xRoot.SelectSingleNode("descendant::Package/GlobalVariables"));
			xRoot.SelectSingleNode("descendant::Package").AppendChild(xGlobalVariables);

			// create connections nodes
			XmlNode xConnections = xDoc.CreateNode(XmlNodeType.Element, "Connections", "");
			zz = 0;
			foreach( DTS.Connection2 cn in oPackage.Connections )
			{
				Type t = typeof(DTS.Connection2);
				string nodeName = "Connection_" + zz.ToString();
				CreateChildNodes(xDoc, ref xConnections, t, nodeName, cn);
				zz ++;
			}
			xRoot.SelectSingleNode("descendant::Package").RemoveChild(xRoot.SelectSingleNode("descendant::Package/Connections"));
			xRoot.SelectSingleNode("descendant::Package").AppendChild(xConnections);

			// create steps nodes
			XmlNode xSteps = xDoc.CreateNode(XmlNodeType.Element, "Steps", "");
			zz = 0;
			foreach( DTS.Step2 sp in oPackage.Steps )
			{
				Type t = typeof(DTS.Step2);
				string nodeName = "Step_" + zz.ToString();
				CreateChildNodes(xDoc, ref xSteps, t, nodeName, sp);
				zz ++;
			}
			xRoot.SelectSingleNode("descendant::Package").RemoveChild(xRoot.SelectSingleNode("descendant::Package/Steps"));
			xRoot.SelectSingleNode("descendant::Package").AppendChild(xSteps);
			
			// create tasks nodes
			XmlNode xTasks = xDoc.CreateNode(XmlNodeType.Element, "Tasks", "");
			zz = 0;
			foreach( DTS.Task ts in oPackage.Tasks )
			{
				Type t = typeof(DTS.Task);
				string nodeName = "Task_" + zz.ToString();
				CreateChildNodes(xDoc, ref xTasks, t, nodeName, ts);
				// remove customtask node as it is only being used to get the customtask properties under the customtaskID node
				xTasks.SelectSingleNode("child::" + nodeName).RemoveChild(xTasks.SelectSingleNode("child::" + nodeName + "/CustomTask"));
				zz ++;
			}
			xRoot.SelectSingleNode("descendant::Package").RemoveChild(xRoot.SelectSingleNode("descendant::Package/Tasks"));
			xRoot.SelectSingleNode("descendant::Package").AppendChild(xTasks);

			xDoc.AppendChild(xRoot);
			xDoc.Normalize();
			return xDoc;
		}

		/// <summary>
		/// Creates the child nodes in the XML doc.
		/// </summary>
		/// <param name="xDoc">The XML doc.</param>
		/// <param name="xTop">The XML top node.</param>
		/// <param name="t">The type of node.</param>
		/// <param name="nodeName">Name of the node.</param>
		/// <param name="obj">The obj to serialize.</param>
		private void CreateChildNodes(XmlDocument xDoc, ref XmlNode xTop, Type t, string nodeName, object obj)
		{
			int CustomTaskIndex = -1;
			int TaskIndex = -1;
			PropertyInfo [] pi;

			if (nodeName.StartsWith("Task_"))
			{
				TaskIndex = Convert.ToInt32(nodeName.Split('_')[1]);
			}
			if (nodeName == "CustomTaskProperties")
			{
				// Instance properites.
				pi = t.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic | 
					BindingFlags.Public);
				// look for interface properties if there are no instance properties
				if (pi.Length == 0)
				{
					Type [] Ti  = t.GetInterfaces();
					if (Ti.Length > 0)
					{
						pi = Ti[0].GetProperties (BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | 
							BindingFlags.Public);				
					}
				}
			}
			else
			{
				// Instance properites.
				pi = t.GetProperties (BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | 
					BindingFlags.Public);
			}

			XmlNode xParent = xDoc.CreateNode(XmlNodeType.Element, nodeName, "");
			for(int ii = 0; ii < pi.Length; ii++) 
			{
				MemberInfo m = pi[ii];

				XmlNode xChild = xDoc.CreateNode(XmlNodeType.Element, m.Name, "");

				if (!pi[ii].CanWrite)
				{
					XmlAttribute xAttr;
					xAttr = xDoc.CreateAttribute("ReadOnly", "");
					xAttr.Value = (!pi[ii].CanWrite).ToString();
					xChild.Attributes.Append(xAttr);
				}

				// only output values for those properties that are not null and not a comObject
				if (m.Name != "CommandProperties" && m.Name != "SourceCommandProperties" && m.Name != "DestinationCommandProperties" && m.Name != "ConnectionProperties")
				{
					if (pi[ii].GetValue(obj, new object[0]) != null) 
					{
						if (pi[ii].GetValue(obj, new object[0]).ToString() != "System.__ComObject")
						{
							xChild.InnerText = pi[ii].GetValue(obj, new object[0]).ToString();
						}
					}
				}

				if (m.Name == "PrecedenceConstraints")
				{
					DTS.PrecedenceConstraints ps = (DTS.PrecedenceConstraints)pi[ii].GetValue(obj, new object[0]);
					int zz = 0;
					foreach (DTS.PrecedenceConstraint p1 in ps)
					{
						CreateChildNodes(xDoc, ref xChild, typeof(DTS.PrecedenceConstraint), "PrecedenceConstraint_" + zz.ToString(), p1);
						zz ++;
					}
					ps = null;
				}
				if (m.Name == "CustomTask")
				{
					CustomTaskIndex = ii;
				}
				if (m.Name == "CustomTaskID")
				{
					if (CustomTaskIndex >= 0)
					{						
						DTS.CustomTask ct = (DTS.CustomTask)pi[CustomTaskIndex].GetValue(obj, new object[0]);
						string CustomTaskType = pi[ii].GetValue(obj, new object[0]).ToString();
						Type t1 = GetCustomTaskFromID(CustomTaskType);
						if (CustomTaskType == "DTSDynamicPropertiesTask")
						{
							DTS.Task dt = oPackage.Tasks.Item(TaskIndex + 1);
							DTSCustTasks.DynamicPropertiesTask dyn = (DynamicPropertiesTask)dt.CustomTask;
							CreateChildNodes(xDoc, ref xChild, typeof(DTSCustTasks.DynamicPropertiesTask), "CustomTaskProperties", dyn);
							DTSCustTasks.DynamicPropertiesTaskAssignments ass = dyn.Assignments();
							CreateChildNodes(xDoc, ref xChild, typeof(DTSCustTasks.DynamicPropertiesTaskAssignments), "DynamicPropertyAssignments", ass);
							int zz = 0;
							foreach (DTSCustTasks.DynamicPropertiesTaskAssignment a in ass)
							{
								XmlNode xNode = xChild.SelectSingleNode("DynamicPropertyAssignments");
								CreateChildNodes(xDoc, ref xNode, typeof(DTSCustTasks.DynamicPropertiesTaskAssignment), "DynamicPropertyAssignment_" + zz.ToString(), a);
								zz ++;
							}
							ass = null;
							dyn = null;
						}
						else
						{
							CreateChildNodes(xDoc, ref xChild, t1, "CustomTaskProperties", ct);
						}
						CustomTaskIndex = -1;
					}
				}
				if (m.Name == "Transformations")
				{
					DTS.Transformations trans = (DTS.Transformations)pi[ii].GetValue(obj, new object[0]);
					XmlAttribute xAttr1;
					xAttr1 = xDoc.CreateAttribute("Count", "");
					xAttr1.Value = trans.Count.ToString();
					xChild.Attributes.Append(xAttr1);
					foreach (DTS.Transformation2 t2 in trans)
					{
						CreateChildNodes(xDoc, ref xChild, typeof(DTS.Transformation2), CleanInput(t2.Name), t2);
					}
					trans = null;
				}
				if (m.Name == "SourceColumns" || m.Name == "DestinationColumns")
				{
					DTS.Columns cols = (DTS.Columns)pi[ii].GetValue(obj, new object[0]);
					int zz = 0;
					foreach(DTS.Column col in cols)
					{
						CreateChildNodes(xDoc, ref xChild, typeof(DTS.Column), "Col_" + zz.ToString(), col);
						zz++;
					}
					cols = null;
				}
				if (m.Name == "TransformServerProperties" || m.Name == "Properties")
				{
					// walk thru properties
					DTS.Properties p = (DTS.Properties)pi[ii].GetValue(obj, new object[0]);
					if (p != null)
					{
						foreach (DTS.Property p1 in p)
						{
							XmlNode xChildProperty = xDoc.CreateNode(XmlNodeType.Element, CleanInput(p1.Name), "");

							if (!p1.Set)
							{
								XmlAttribute xAttr1;
								xAttr1 = xDoc.CreateAttribute("ReadOnly", "");
								xAttr1.Value = (!p1.Set).ToString();
								xChildProperty.Attributes.Append(xAttr1);
							}
							if (p1.Value != null)
							{
								xChildProperty.InnerText = p1.Value.ToString();						
							}
							xChild.AppendChild(xChildProperty);
						}
						p = null;
					}
				}
				if (m.Name == "CommandProperties" || m.Name == "ConnectionProperties"  || m.Name == "SourceCommandProperties" || m.Name == "DestinationCommandProperties")
				{
					// walk thru properties
					try
					{
						DTS.OleDBProperties p = (DTS.OleDBProperties)pi[ii].GetValue(obj, new object[0]);
						foreach (DTS.OleDBProperty2 p1 in p)
						{
							XmlNode xChildProperty = xDoc.CreateNode(XmlNodeType.Element, CleanInput(p1.Name), "");
							if (p1.Value != null)
							{
								// TODO: add reconversion of non-visible chars
								xChildProperty.InnerText = p1.Value.ToString().Replace("\r", "{cr}").Replace("\n", "{lf}");						
							}
							xChild.AppendChild(xChildProperty);
						}
						p = null;
					}
					catch(Exception ex)
					{
						XmlNode xnErr = xDoc.CreateNode(XmlNodeType.Element, m.Name + "_OLEDBError","");
						xnErr.InnerText = ex.Message + " \n" + ex.StackTrace;
						xChild.AppendChild(xnErr);
					}
				}
				if (m.Name == "Lookups")
				{
					DTS.Lookups lookups = (DTS.Lookups)pi[ii].GetValue(obj, new object[0]);
					int zz = 0;
					foreach (DTS.Lookup l in lookups)
					{
						CreateChildNodes(xDoc, ref xChild, typeof(DTS.Column), "Lookup_" + zz.ToString(), l);
						zz++;
					}
					lookups = null;
				}
				xParent.AppendChild(xChild);
			}
			xTop.AppendChild(xParent);
		}

		/// <summary>
		/// Sets the DTS package/object properties from a serialized XML node.
		/// </summary>
		/// <param name="p">The DTS properties.</param>
		/// <param name="x">The XML node.</param>
		private void SetDTSProperties(DTS.Properties p, XmlNode x)
		{
			if (p == null) return;
			if (x == null) return;

			foreach ( XmlElement xl in x )
			{
				foreach (DTS.Property p1 in p)
				{
					if (p1.Set && p1.Name == xl.Name)
					{
						if (xl.InnerText.Length > 0)
						{
							p1.Value = xl.InnerText.Replace("{cr}","\r").Replace("{lf}","\n");
						}
						break;
					}
				}
			}
			p = null;
		}

		/// <summary>
		/// Sets the DTS package/object OLEDB properties from a serialized XML node.
		/// </summary>
		/// <param name="p">The DTS OLEDB properties.</param>
		/// <param name="x">The XML node.</param>
		private void SetOLEDBProperties(DTS.OleDBProperties p, XmlNode x)
		{
			if (p == null) return;
			if (x == null) return;

			foreach ( XmlElement xl in x )
			{
				foreach (DTS.OleDBProperty2 p1 in p)
				{
					if (p1.Name == xl.Name.Replace("_", " "))
					{
						// TODO: add reconversion of non-visible chars 
						if (xl.InnerText.Length > 0) 
						{
							p1.Value = xl.InnerText.Replace("{cr}","\r").Replace("{lf}","\n");
						}
						break;
					}
				}
			}
			p = null;
		}

		/// <summary>
		/// Sets the specified DTS property.
		/// </summary>
		/// <param name="obj">The obj to set properties for.</param>
		/// <param name="pi">The PropertyInfo object.</param>
		/// <param name="Value">The value to set.</param>
		private void SetProperty(object obj, PropertyInfo pi, string Value)
		{
			try
			{
				if (pi == null) return;
				if (obj == null) return;
				if (Value == null) return;
				if (!pi.CanWrite) return;

				Type t = ((System.Reflection.PropertyInfo)((MemberInfo)pi)).PropertyType;
				if (t.BaseType == typeof(System.Enum))
				{
					pi.SetValue(obj, TypeDescriptor.GetConverter(t).ConvertFrom(Value), new object[0]);
				}
				else
				{
					pi.SetValue(obj, Convert.ChangeType(Value, t), new object[0]);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Gets the custom DTS task from a ID string.
		/// </summary>
		/// <param name="customTaskID">The custom task ID.</param>
		/// <returns>Returns a custom DTS task type.</returns>
		private Type GetCustomTaskFromID(string customTaskID)
		{
			Type tt;
			switch (customTaskID)
			{
				case "DTSDataPumpTask":
				{
					tt = typeof(DTS.DataPumpTask2);
					break;
				}
				case "DTSExecuteSQLTask":
				{
					tt = typeof(DTS.ExecuteSQLTask2);
					break;
				}
				case "DTSCreateProcessTask":
				{
					tt = typeof(DTS.CreateProcessTask2);
					break;
				}
				case "DTSActiveScriptTask":
				{
					tt = typeof(DTS.ActiveScriptTask);
					break;
				}
				case "DTSBulkInsertTask":
				{
					tt = typeof(DTS.BulkInsertTask);
					break;
				}
				case "DTSDataDrivenQueryTask":
				{
					tt = typeof(DTS.DataDrivenQueryTask2);
					break;
				}
				case "DTSTransferObjectsTask":
				{
					tt = typeof(DTS.TransferObjectsTask2);
					break;
				}
				case "DTSExecutePackageTask":
				{
					tt = typeof(DTS.ExecutePackageTask);
					break;
				}
				case "DTSParallelDataPumpTask":
				{
					tt = typeof(DTS.ParallelDataPumpTask);
					break;
				}
				case "DTSSendMailTask":
				{
					tt = typeof(DTS.SendMailTask);
					break;
				}
				case "DTSFTPTask":
				{
					tt = typeof(DTSCustTasks.DTSFTPTask);
					break;
				}
				case "DTSMessageQueueTask":
				{
					tt = typeof(DTSCustTasks.DTSMessageQueueTask);
					break;
				}
				case "DTSDynamicPropertiesTask":
				{
					tt = typeof(DTSCustTasks.DynamicPropertiesTask);
					break;
				}
				default:
				{
					tt = typeof(DTS.CustomTask);
					break;
				}
			}
			return tt;
		}

		/// <summary>
		/// Replace invalid characters with empty strings.
		/// </summary>
		/// <param name="strIn">The string to clean.</param>
		/// <returns></returns>
		private String CleanInput(string strIn)
		{
			// Replace invalid characters with empty strings.
			return Regex.Replace(strIn, @"[^\0x20\w\.@-]", "_"); 
		}

		/// <summary>
		/// Converts from auth type flag to DTS SQL server storage type flag.
		/// </summary>
		/// <param name="Value">The auth type flag value.</param>
		/// <returns>Returns DTS SQL server storage type flag.</returns>
		private DTS.DTSSQLServerStorageFlags ConvertFromAuthTypeFlag(authTypeFlags Value)
		{
			if (Value == authTypeFlags.Default)
				return DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_Default;
			else
				return DTS.DTSSQLServerStorageFlags.DTSSQLStgFlag_UseTrustedConnection;
		}
		#endregion
	}

}
