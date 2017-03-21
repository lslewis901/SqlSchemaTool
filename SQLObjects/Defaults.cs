using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Summary description for Defaults class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class Defaults : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Defaults"/> class.
		/// </summary>
		public Defaults() : base() { }

		/// <summary>
		/// Main Entry point of the Defaults object which inherits from a DataSet class
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Defaults(string DBName) : base (DBName) { }

		/// <summary>
		/// Retrieves the Default objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
		public override void GetObject<Defaults>(SqlConnection _connection, params object[] args)
		{
			string cmd_GetDefaultsList = string.Format(SqlQueryStrings.GetDefaultsList, _connection.Database);
            using (SqlCommand _command_Defaults = new SqlCommand(cmd_GetDefaultsList, _connection))
            {
                _command_Defaults.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Defaults))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "DEFAULT";

				// add default tables 
				this.Tables.Add("DEFAULT_TEXT");
				this.Tables["DEFAULT_TEXT"].Columns.Add("DEFAULT_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
					string _tableName = _dr["DEFAULT_NAME"].ToString();
                    string cmd_GetDefaultSchema = string.Format(SqlQueryStrings.GetDefaultsSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Text = new SqlCommand(cmd_GetDefaultSchema, _connection))
                    {
                        _command_Text.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Text))
                        {
                            _dsFields = new DataSet("DEFAULT_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 1)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "DEFAULT_TEXT";

                        AddMissingTableColumn(ref _dsFields, "DEFAULT_TEXT", "DEFAULT_NAME", false);

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
									DataColumn _dc = this.Tables[_dt.TableName].Columns["DEFAULT_NAME"];
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
				this.Relations.Add("DefaultsCreateTexts", 
					this.Tables["DEFAULT"].Columns["DEFAULT_NAME"],
					this.Tables["DEFAULT_TEXT"].Columns["DEFAULT_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of defaults: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
