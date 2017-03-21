using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Class to serialize the DB tables which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class Tables : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Tables"/> class.
		/// </summary>
		public Tables() : base() { }

		/// <summary>
		/// Main Entry point of the Tables object which inherits from a DataSet class.
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Tables(string DBName) : base (DBName) { }

        /// <summary>
		/// Retrieves the Table objects from the specified SQL server DB.
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args">Flag to force use of the Primary FileGroup for the table.</param>
		public override void GetObject<Tables>(SqlConnection _connection, params object[] args)
		{
            bool Primary = args.Length > 0 ? (bool)args[0] : true;
            string tableNames = args.Length > 1 ? string.Format("and convert(sysname,o.name) IN {0}", (string)args[1]) : string.Empty;
            string cmd_GetTablesList = string.Format(SqlQueryStrings.GetTablesList, _connection.Database, tableNames);
            using (SqlCommand _command_Tables = new SqlCommand(cmd_GetTablesList, _connection))
            {
                _command_Tables.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Tables))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "TABLE";

				// add default tables 
				this.Tables.Add("COLUMN");
				this.Tables["COLUMN"].Columns.Add("TABLE_NAME");
				this.Tables.Add("TABLE_FILEGROUP");
				this.Tables.Add("TABLE_REFERENCE");
				this.Tables.Add("TABLE_INDEX");
				this.Tables.Add("TABLE_CONSTRAINTS");
				// there is also another output (sometimes) for referenced views

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
                    string _tableName = _dr["TABLE_NAME"].ToString();
					string _IndexGroup = Primary ? "'PRIMARY'" : "groupname from sysfilegroups where groupid = @groupid";
                    string sp_GetIndexes = string.Format(SqlQueryStrings.GetIndexes, _tableName, _IndexGroup);
                    string sp_GetColumns = string.Format(SqlQueryStrings.GetColumns, _tableName);
                    string sp_GetTableReferences = string.Format(SqlQueryStrings.GetTableReferences, _tableName);
                    string sp_GetTableFileGroups = Primary ? SqlQueryStrings.ForceTableFileGroups : string.Format(SqlQueryStrings.GetTableFileGroups, _tableName);
                    string cmd_GetTableSchema = string.Format(SqlQueryStrings.GetTableSchema, _connection.Database, _tableName, sp_GetColumns, sp_GetIndexes, sp_GetTableFileGroups, sp_GetTableReferences);
                    using (SqlCommand _command_Fields = new SqlCommand(cmd_GetTableSchema, _connection))
                    {
                        _command_Fields.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Fields))
                        {
                            _dsFields = new DataSet("TABLE_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 2)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "TABLE_NAME";
						_dsFields.Tables[1].TableName = "COLUMN";
						if (_dsFields.Tables.Count >= 3 )
							_dsFields.Tables[2].TableName = "TABLE_FILEGROUP";
						if (_dsFields.Tables.Count >= 4 )
							_dsFields.Tables[3].TableName = "TABLE_REFERENCE";
						if (_dsFields.Tables.Count >= 5 )
							_dsFields.Tables[4].TableName = "TABLE_INDEX";
						if (_dsFields.Tables.Count == 6 )
							_dsFields.Tables[5].TableName = "TABLE_CONSTRAINTS";
						// there is also another output (sometimes) for referenced views

						// add any missing tables and/or add the default column
                        AddMissingTableColumn(ref _dsFields, "TABLE_FILEGROUP", "TABLE_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "TABLE_REFERENCE", "TABLE_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "TABLE_INDEX", "TABLE_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "TABLE_CONSTRAINTS", "TABLE_NAME", false);

						try
						{
							foreach(DataTable _dt in _dsFields.Tables)
							{
								if (this.Tables[_dt.TableName] != null)
								{
									if (_dt.Rows.Count == 0)
									{
										_dt.Rows.Add(_dt.NewRow());
									}	
									DataColumn _dc = this.Tables[_dt.TableName].Columns["TABLE_NAME"];
									_dc.DefaultValue = _tableName;
									this.Merge(_dt, true, MissingSchemaAction.Add);
								}
							}
						}
						catch(Exception ex)
						{
							throw ex;
						}
					}
				}
				this.Relations.Add("TablesFields", 
					this.Tables["TABLE"].Columns["TABLE_NAME"],
					this.Tables["COLUMN"].Columns["TABLE_NAME"]).Nested = true;
				this.Relations.Add("TablesFileGroups", 
					this.Tables["TABLE"].Columns["TABLE_NAME"],
					this.Tables["TABLE_FILEGROUP"].Columns["TABLE_NAME"]).Nested = true;
				this.Relations.Add("TablesReferences", 
					this.Tables["TABLE"].Columns["TABLE_NAME"],
					this.Tables["TABLE_REFERENCE"].Columns["TABLE_NAME"]).Nested = true;
				this.Relations.Add("TablesIndexes", 
					this.Tables["TABLE"].Columns["TABLE_NAME"],
					this.Tables["TABLE_INDEX"].Columns["TABLE_NAME"]).Nested = true;
				this.Relations.Add("TablesConstraints", 
					this.Tables["TABLE"].Columns["TABLE_NAME"],
					this.Tables["TABLE_CONSTRAINTS"].Columns["TABLE_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of tables: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
