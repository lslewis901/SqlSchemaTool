using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Summary description for UDDTs class which inherits from a DataSet class.
	/// </summary>
	[Serializable()]
	public class UDDTs : BaseObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UDDTs"/> class.
		/// </summary>
		public UDDTs() : base() {}

		/// <summary>
		/// Main Entry point of the UDDTs object which inherits from a DataSet class.
		/// </summary>
		/// <param name="DBName">The string name of the Database.</param>
		public UDDTs(string DBName) : base (DBName) {}

		/// <summary>
		/// Retrieves the UDDT objects from the specified SQL server DB
		/// </summary>
		/// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
        public override void GetObject<UDDTs>(SqlConnection _connection, params object[] args)
		{
			string cmd_GetUDDTsList = string.Format(SqlQueryStrings.GetUDDTsList, _connection.Database);
            using (SqlCommand _command_UDDTs = new SqlCommand(cmd_GetUDDTsList, _connection))
            {
                _command_UDDTs.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_UDDTs))
                {
                    _sdaTables.Fill(this);
                }
            }
			if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
			{
				// RENAME SECOND TABLE
				this.Tables[1].TableName = "UDDT";

				// add UDDT tables 
				this.Tables.Add("UDDT_TEXT");
				this.Tables["UDDT_TEXT"].Columns.Add("UDDT_NAME");

				DataRow [] _arTables = new DataRow [this.Tables[1].Rows.Count];

				this.Tables[1].Rows.CopyTo(_arTables, 0);
				foreach(DataRow _dr in _arTables)
				{
					string _tableName = _dr["UDDT_NAME"].ToString();
					DataSet _dsFields = new DataSet("UDDT_SCHEMA");
					_dsFields.Tables.Add("UDDT_TEXT");
					_dsFields.Tables["UDDT_TEXT"].Columns.Add("UDDT_NAME");
					_dsFields.Tables["UDDT_TEXT"].Columns.Add("Text");

                    AddMissingTableColumn(ref _dsFields, "UDDT_TEXT", "UDDT_NAME", true);

					try
					{
						string nulls = ( _dr["allownulls"].ToString().ToLower() == "true" ? "NULL" : "NONULL" );
						string Rule = string.Format(_dr["rule"].ToString() != "" ?  "\nEXEC sp_bindrule N'[dbo].[{1}]', N'{0}'" : "" , _dr["UDDT_NAME"].ToString(), _dr["rule"].ToString() );
						string binddefault = string.Format( _dr["default"].ToString() != "" ? "\nEXEC sp_bindefault N'[dbo].[{1}]', N'{0}'" : "" , _dr["UDDT_NAME"].ToString(), _dr["default"].ToString() );
						string Result = string.Empty; //"use {8}\n\nif exists (select * from dbo.systypes where name = N'{0}')\n" + 
						//"    EXEC sp_droptype N'{0}'\n";
						if ( no_precision_types.Contains(_dr["type"].ToString().ToLower()))
						{
							Result += "EXEC sp_addtype N'{0}', N'{1}', N'{4}', N'{5}'\n";
						}
						else
						{
							if ( _dr["scale"].ToString().Length > 0 )
							{
								Result += "EXEC sp_addtype N'{0}', N'{1} ({2},{3})', N'{4}', N'{5}' ";
							}
							else
							{
								Result += "EXEC sp_addtype N'{0}', N'{1} ({2})', N'{4}', N'{5}' ";
							}
						}
						string create_text = string.Format( Result, _dr["UDDT_NAME"].ToString(), _dr["type"].ToString(), _dr["prec"].ToString(), _dr["scale"].ToString(), nulls, _dr["USER_NAME"].ToString(), _dr["default"].ToString(), _connection.Database ) + binddefault + Rule; ;

						_dsFields.Tables[0].Rows.Add(new object [] {_dr["UDDT_NAME"].ToString(), create_text});
						this.Merge(_dsFields.Tables[0], true, MissingSchemaAction.Add);
					}
					catch(Exception ex)
					{
						throw ex;
					}
				}
				this.Relations.Add("UDDTsCreateTexts", 
					this.Tables["UDDT"].Columns["UDDT_NAME"],
					this.Tables["UDDT_TEXT"].Columns["UDDT_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of UDDTs: {0}", this.Tables[1].Rows.Count, source);
			}
		}
	}
}
