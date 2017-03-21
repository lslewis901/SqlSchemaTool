using NLog;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Summary description for Triggers class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class Triggers : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Triggers"/> class.
		/// </summary>
		public Triggers() : base() { }

		/// <summary>
		/// Main Entry point of the Triggers object which inherits from a DataSet class
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Triggers(string DBName) : base (DBName) { }

		/// <summary>
		/// Retrieves the Trigger objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
        public override void GetObject<Triggers>(SqlConnection _connection, params object[] args)
		{
			string cmd_getTriggersList = string.Format(SqlQueryStrings.GetTriggersList, _connection.Database);
            using (SqlCommand _command_Triggers = new SqlCommand(cmd_getTriggersList, _connection))
            {
                _command_Triggers.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Triggers))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "TRIGGER";

				// add default tables 
				this.Tables.Add("TRIGGER_TEXT");
				this.Tables["TRIGGER_TEXT"].Columns.Add("TRIGGER_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
					string _tableName = _dr["TRIGGER_NAME"].ToString();
                    string cmd_getTriggersSchema = string.Format(SqlQueryStrings.GetTriggersSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Text = new SqlCommand(cmd_getTriggersSchema, _connection))
                    {
                        _command_Text.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Text))
                        {
                            _dsFields = new DataSet("TRIGGER_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 1)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "TRIGGER_TEXT";

                        AddMissingTableColumn(ref _dsFields, "TRIGGER_TEXT", "TRIGGER_NAME", false);

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
									DataColumn _dc = this.Tables[_dt.TableName].Columns["TRIGGER_NAME"];
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
				this.Relations.Add("TriggerCreateTexts", 
					this.Tables["TRIGGER"].Columns["TRIGGER_NAME"],
					this.Tables["TRIGGER_TEXT"].Columns["TRIGGER_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of Triggers: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
