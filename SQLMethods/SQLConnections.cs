using Lewis.SST.SecurityMethods;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLMethods
{
    /// <summary>
    /// SQL server login security types
    /// </summary>
    public enum SecurityType
    {
        /// <summary>
        /// indicates using integrated Windows login security model.
        /// </summary>
        Integrated = 0,
        /// <summary>
        /// indicates using SQL login security model.
        /// </summary>
        Mixed = 1,
        /// <summary>
        /// security type not yet determined
        /// </summary>
        NULL = 2
    }

    /// <summary>
	/// SQLConnections Collection class.  A class to create and contain multiple SqlConnection objects.
	/// </summary>
	[Serializable()]
	public class SQLConnections : CollectionBase
	{
        /// <summary>
        /// connect format string used by methods to format Secure SQL connection string
        /// </summary>
		[NonSerialized()]
		public static string _secureConnect = 
			@"Connection Timeout=30;Integrated Security=SSPI;Persist Security Info=false;Initial Catalog='{0}';Data Source='{1}'";

        /// <summary>
        /// connect format string used by methods to format NonSecure SQL connection string
        /// </summary>
		[NonSerialized()]
        public static string _unsecureConnect = 
			@"Connection Timeout=30;Integrated Security=false;Persist Security Info=false;Initial Catalog='{0}';Data Source='{1}';User ID='{2}';Password='{3}'";

        /// <summary>
        /// public default ctor
        /// </summary>
        public SQLConnections()
        {
            // default ctor
        }

        /// <summary>
		/// Instantiates a new SqlConnections class using a connection string.
		/// The following example illustrates a typical connection string.
		/// "Persist Security Info=False;Integrated Security=SSPI;database=northwind;server=mySQLServer"
		/// </summary>
		/// <param name="ConnectionString">The connection string that includes the source database name, and other parameters needed to establish the initial connection.</param>
		public SQLConnections(string ConnectionString)
		{
			this.Add(new SQLConnection(ConnectionString));
		}

		/// <summary>
		/// Instantiates a new SqlConnections class using a connection string.  
		/// This instantiated class assumes a secure connection using a trusted Windows login.
		/// </summary>
		/// <param name="SqlServer">The SqlServer string is the SQL server name to make the connection to.</param>
		/// <param name="Database">The Database string is the SQL database name to make the connection to.</param>
		public SQLConnections(string SqlServer, string Database)
		{
			string _connectionstr = string.Format(_secureConnect, Database, SqlServer);
            this.Add(new SQLConnection(_connectionstr));
        }

		/// <summary>
		/// Instantiates a new SqlConnections class using a connection string.
		/// This instantiated class assumes a unsecure connection using a valid SQL login.
		/// </summary>
		/// <param name="SqlServer">The SqlServer string is the SQL server name to make the connection to.</param>
		/// <param name="Database">The Database string is the SQL database name to make the connection to.</param>
		/// <param name="UserID">The UserID string is a SQL server user login name used to validate the connection.</param>
		/// <param name="Password">The Password string is a SQL server user password name used to validate the connection.</param>
        /// <param name="savePassword">The true false flag for persisting the password when saving the settings on close.</param>
        public SQLConnections(string SqlServer, string Database, string UserID, string Password, bool savePassword)
		{
			string _connectionstr = string.Format(_unsecureConnect, Database, SqlServer, UserID, Password);
            this.Add(new SQLConnection(_connectionstr, savePassword));
        }

        /// <summary>
        /// get the int index value of the server connection object
        /// </summary>
        /// <param name="_connection"></param>
        /// <returns></returns>
        public virtual int getIndex(SQLConnection _connection)
        {
            return this.List.IndexOf(_connection);
        }

        /// <summary>
        /// get the int index value of the server connection
        /// </summary>
        /// <param name="ServerName"></param>
        /// <returns></returns>
        public virtual int getIndex(string ServerName)
        {
            int ii = 0;
            for (ii = 0; ii < this.List.Count; ii++)
            {
                SQLConnection sc = (SQLConnection)this.List[ii];
                if (sc.sqlConnection.DataSource.ToLower().Equals(ServerName.ToLower()))
                {
                    return ii;
                }
            }
            return -1;
        }

		/// <summary>
		/// adds a new SqlConnection object to the SqlConnections class.
		/// </summary>
		/// <param name="_connection"></param>
		/// <returns></returns>
		public virtual int Add(SQLConnection _connection)
		{
            return (this[_connection.Server] == null) ? this.List.Add(_connection) : getIndex(_connection);
		}

        /// <summary>
        /// adds a new SqlConnection object to the SqlConnections class.
        /// </summary>
        /// <param name="SqlServer"></param>
        /// <param name="Database"></param>
        /// <param name="UserID"></param>
        /// <param name="Password"></param>
        /// <param name="savePassword"></param>
        /// <returns></returns>
        public virtual int Add(string SqlServer, string Database, string UserID, string Password, bool savePassword)
        {
            string _connectionstr = string.Format(_unsecureConnect, Database, SqlServer, UserID, Password);
            return this.Add(new SQLConnection(_connectionstr, savePassword));
        }

        /// <summary>
        /// adds a new SqlConnection object to the SqlConnections class.
        /// </summary>
        /// <param name="SqlServer"></param>
        /// <param name="Database"></param>
        /// <returns></returns>
        public virtual int Add(string SqlServer, string Database)
        {
            string _connectionstr = string.Format(_secureConnect, Database, SqlServer);
            return this.Add(new SQLConnection(_connectionstr));
        }

        /// <summary>
        /// adds a new SqlConnection object to the SqlConnections class.
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <returns></returns>
        public virtual int Add(string ConnectionString)
        {
            return this.Add(new SQLConnection(ConnectionString));
        }

        /// <summary>
        /// adds a new SqlConnection object to the SqlConnections class with an encrypted connection string
        /// </summary>
        /// <param name="encryptedConnectionString"></param>
        /// <param name="sKey"></param>
        /// <param name="savePassword"></param>
        /// <returns></returns>
        public virtual int AddEncrypted(string encryptedConnectionString, string sKey, bool savePassword)
        {
            string connectionString = Security.DecryptString(encryptedConnectionString, sKey);
            return this.Add(new SQLConnection(connectionString, savePassword));
        }

        /// <summary>
        /// removes SqlConnection object from the collection
        /// </summary>
        /// <param name="_connection"></param>
        public virtual void Remove(SQLConnection _connection)
        {
            if (this[_connection.Server] != null)
            {
                this.List.Remove(_connection);
            }
        }

        /// <summary>
        /// removes SqlConnection object from the collection
        /// </summary>
        /// <param name="ServerName"></param>
        public virtual void Remove(string ServerName)
        {
            if (this[ServerName] != null)
            {
                this.List.Remove(this[ServerName]);
            }
        }

        /// <summary>
		/// Gets the index of the current SqlConnection object.
		/// </summary>
		public virtual SQLConnection this[int Index]
		{
			get
            {
                SQLConnection retval = null;
                if (this.List.Count > 0 && Index > -1)
                {
                    retval = (SQLConnection)this.List[Index];
                }
                return retval;
            }
		}

        /// <summary>
        /// returns specified SqlConnection using string name
        /// </summary>
        /// <param name="ServerName"></param>
        /// <returns></returns>
        public virtual SQLConnection this[string ServerName]
        {
            get
            {
                foreach (SQLConnection sc in this.List)
                {
                    if (sc.sqlConnection.DataSource.ToLower().Equals(ServerName.ToLower()))
                    {
                        return sc;
                    }
                }
                return null;
            }
        }
	}

    /// <summary>
    /// SQLConnection class wraps SqlConnection class to add additional data elements
    /// </summary>
    public class SQLConnection 
    {
        private System.Data.SqlClient.SqlConnection _sqlConnection;
        private string _connectionString;
        private string _password;
        private bool _savePassword;

        private SQLConnection()
        {
            _sqlConnection = new System.Data.SqlClient.SqlConnection();
        }

        /// <summary>
        /// Constructor with connection string param
        /// </summary>
        /// <param name="connectionString"></param>
        public SQLConnection(String connectionString)
        {
            _sqlConnection = new System.Data.SqlClient.SqlConnection();
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Constructor with connection string param, and a save the password param
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="savePassword"></param>
        public SQLConnection(String connectionString, bool savePassword)
        {
            SavePassword = savePassword;
            _sqlConnection = new System.Data.SqlClient.SqlConnection();
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Property to get the SqlConnection
        /// </summary>
        public virtual SqlConnection sqlConnection
        {
            get { return _sqlConnection; }
        }

        /// <summary>
        /// Property to get the Connectionstring
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
            set 
            { 
                _connectionString = value;
                if (_sqlConnection != null)
                {
                    if (_savePassword && _password != null && _connectionString != null && _connectionString.Trim().Length > 0 && !_connectionString.ToLower().Contains("password") && _connectionString.ToLower().Contains("integrated security=false"))
                    {
                        _sqlConnection.ConnectionString += string.Format("; Password='{0}'", _password);
                    }
                    else if (_savePassword && _connectionString != null && _connectionString.Trim().Length > 0 && _connectionString.ToLower().Contains("password"))
                    {
                        string[] strings = _connectionString.Split(new char[] { ';' });
                        foreach (string s in strings)
                        {
                            if (s.ToLower().Contains("password"))
                            {
                                string[] words = s.Split(new char[] { '=' });
                                if (words.Length > 1)
                                {
                                    _password = words[1].Replace("'", "");
                                    break;
                                }
                            }
                        }
                    }
                    _sqlConnection.ConnectionString = _connectionString;
                }
            }
        }

        /// <summary>
        /// Property to get the security type used by the connection
        /// </summary>
        public virtual SecurityType securityType
        {
            get
            {
                string[] strings = _connectionString.Split(new char[] { ';' });
                foreach (string s in strings)
                {
                    if (s.ToLower().Contains("security"))
                    {
                        string[] words = s.Split(new char[] { '=' });
                        if (words.Length > 1)
                        {
                            return words[1].ToLower().Replace("'", "").Equals("false") ? SecurityType.Mixed : SecurityType.Integrated;
                        }
                    }
                }
                return SecurityType.NULL;
            }
        }

        /// <summary>
        /// Property to get the user set on the connection
        /// </summary>
        public virtual string User
        {
            get
            {
                string[] strings = _connectionString.Split(new char[] { ';' });
                foreach (string s in strings)
                {
                    if (s.ToLower().Contains("user"))
                    {
                        string[] words = s.Split(new char[] { '=' });
                        if (words.Length > 1)
                        {
                            return words[1].Replace("'", "");
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Property to get the server name 
        /// </summary>
        public virtual string Server
        {
            get
            {
                string[] strings = _connectionString.Split(new char[] { ';' });
                foreach (string s in strings)
                {
                    if (s.ToLower().Contains("source"))
                    {
                        string[] words = s.Split(new char[] { '=' });
                        if (words.Length > 1)
                        {
                            return words[1].Replace("'", "");
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Property to get the SavePassword setting
        /// </summary>
        public virtual bool SavePassword
        {
            get 
            {
                return _savePassword; 
            }
            set { _savePassword = value; }
        }

        /// <summary>
        /// Property to get the saved password
        /// </summary>
        public virtual string Password
        {
            get { return _password; }
            set
            {
                if (SavePassword)
                {
                    _password = value;
                    if (_sqlConnection != null)
                    {
                        if (_password != null && _sqlConnection.ConnectionString != null && _sqlConnection.ConnectionString.Trim().Length > 0 && !_sqlConnection.ConnectionString.ToLower().Contains("Password") && _sqlConnection.ConnectionString.ToLower().Contains("Integrated Security=false"))
                        {
                            _sqlConnection.ConnectionString += string.Format("; Password='{0}'", _password);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Property to get the encrypted connection string - used when saving the connection settings
        /// </summary>
        public virtual string EncryptedConnectionString
        {
            get { return Security.EncryptString(SavePassword ? _connectionString : _sqlConnection.ConnectionString, "LLEWIS55"); }
        }
    }
}
