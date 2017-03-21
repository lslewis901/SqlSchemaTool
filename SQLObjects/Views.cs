using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	///  Summary description for Views class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
    public class Views : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Views"/> class.
		/// </summary>
        public Views() : base() { }

		/// <summary>
		/// Main Entry point of the Views object which inherits from a DataSet class
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Views(string DBName) : base (DBName) { }

		/// <summary>
		/// Retrieves the View objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
        public override void GetObject<Views>(SqlConnection _connection, object[] args)
		{
			string cmd_GetViewsList = string.Format(SqlQueryStrings.GetViewsList, _connection.Database);
            using (SqlCommand _command_Views = new SqlCommand(cmd_GetViewsList, _connection))
            {
                _command_Views.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Views))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "VIEW";

				// add default tables 
				this.Tables.Add("CREATE_TEXT");
				this.Tables["CREATE_TEXT"].Columns.Add("VIEW_NAME");
				this.Tables.Add("VIEW_DEPENDS");
				this.Tables["VIEW_DEPENDS"].Columns.Add("VIEW_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
					string _tableName = _dr["VIEW_NAME"].ToString();
                    string cmd_GetViewSchema = string.Format(SqlQueryStrings.GetViewSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Schema = new SqlCommand(cmd_GetViewSchema, _connection))
                    {
                        _command_Schema.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Schema))
                        {
                            _dsFields = new DataSet("VIEW_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 1)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "CREATE_TEXT";

						if (_dsFields.Tables.Count >= 2)
							_dsFields.Tables[1].TableName = "VIEW_DEPENDS";

                        AddMissingTableColumn(ref _dsFields, "CREATE_TEXT", "VIEW_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "VIEW_DEPENDS", "VIEW_NAME", false);

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
									DataColumn _dc = this.Tables[_dt.TableName].Columns["VIEW_NAME"];
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
				this.Relations.Add("ViewsCreateTexts", 
					this.Tables["VIEW"].Columns["VIEW_NAME"],
					this.Tables["CREATE_TEXT"].Columns["VIEW_NAME"]).Nested = true;
				this.Relations.Add("ViewDepends", 
					this.Tables["VIEW"].Columns["VIEW_NAME"],
					this.Tables["VIEW_DEPENDS"].Columns["VIEW_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of Views: {0}", this.Tables[1].Rows.Count, source);
            }
		}
	}
}
