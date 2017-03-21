using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	///  Summary description for Sprocs class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class Sprocs : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Sprocs"/> class.
		/// </summary>
		public Sprocs() : base () { }

		/// <summary>
		/// Main Entry point of the Sprocs object which inherits from a DataSet class
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Sprocs(string DBName) : base(DBName) { }

		/// <summary>
		/// Retrieves the Stored procedure objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
		public override void GetObject<Sprocs>(SqlConnection _connection, params object[] args)
		{
			string cmd_GetSprocsList = string.Format(SqlQueryStrings.GetSprocsList, _connection.Database);
            using (SqlCommand _command_Sprocs = new SqlCommand(cmd_GetSprocsList, _connection))
            {
                _command_Sprocs.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Sprocs))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "SPROC";

				// add default tables 
				this.Tables.Add("SPROC_TEXT");
				this.Tables["SPROC_TEXT"].Columns.Add("SPROC_NAME");
				this.Tables.Add("SPROC_DEPENDS");
				this.Tables["SPROC_DEPENDS"].Columns.Add("SPROC_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
					string _tableName = _dr["SPROC_NAME"].ToString();
                    string cmd_GetSprocSchema = string.Format(SqlQueryStrings.GetSprocSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Text = new SqlCommand(cmd_GetSprocSchema, _connection))
                    {
                        _command_Text.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Text))
                        {
                            _dsFields = new DataSet("SPROC_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 1)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "SPROC_TEXT";

						if (_dsFields.Tables.Count >= 2)
							_dsFields.Tables[1].TableName = "SPROC_DEPENDS";

                        AddMissingTableColumn(ref _dsFields, "SPROC_TEXT", "SPROC_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "SPROC_DEPENDS", "SPROC_NAME", false);

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
									DataColumn _dc = this.Tables[_dt.TableName].Columns["SPROC_NAME"];
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
				this.Relations.Add("SprocsCreateTexts", 
					this.Tables["SPROC"].Columns["SPROC_NAME"],
					this.Tables["SPROC_TEXT"].Columns["SPROC_NAME"]).Nested = true;
				this.Relations.Add("SprocsDepends", 
					this.Tables["SPROC"].Columns["SPROC_NAME"],
					this.Tables["SPROC_DEPENDS"].Columns["SPROC_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of stored procedures: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
