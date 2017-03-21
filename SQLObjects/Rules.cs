using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Summary description for Rules class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class Rules : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Rules"/> class.
		/// </summary>
		public Rules() : base() { }

		/// <summary>
		/// Main Entry point of the Rules object which inherits from a DataSet class.
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public Rules(string DBName) : base (DBName) { }

		/// <summary>
		/// Retrieves the Rule objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
		public override void GetObject<Rules>(SqlConnection _connection, params object[] args)
		{
			string cmd_GetRulesList = string.Format(SqlQueryStrings.GetRulesList, _connection.Database);
            using (SqlCommand _command_Rules = new SqlCommand(cmd_GetRulesList, _connection))
            {
                _command_Rules.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Rules))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "RULE";

				// add default tables 
				this.Tables.Add("RULE_TEXT");
				this.Tables["RULE_TEXT"].Columns.Add("RULE_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
                    DataSet _dsFields = null;
					string _tableName = _dr["RULE_NAME"].ToString();
                    string cmd_getRulesSchema = string.Format(SqlQueryStrings.GetRulesSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Text = new SqlCommand(cmd_getRulesSchema, _connection))
                    {
                        _command_Text.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Text))
                        {
                            _dsFields = new DataSet("RULE_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
					if (_dsFields != null && _dsFields.Tables.Count >= 1)
					{
						// rename returned tables from table[0] .. table[n]
						// to the following values
						_dsFields.Tables[0].TableName = "RULE_TEXT";

                        AddMissingTableColumn(ref _dsFields, "RULE_TEXT", "RULE_NAME", false);

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
									DataColumn _dc = this.Tables[_dt.TableName].Columns["RULE_NAME"];
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
				this.Relations.Add("RuleCreateTexts", 
					this.Tables["RULE"].Columns["RULE_NAME"],
					this.Tables["RULE_TEXT"].Columns["RULE_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of rules: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
