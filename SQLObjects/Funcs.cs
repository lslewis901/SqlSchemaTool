using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Summary description for Funcs class which inherits from a DataSet class.
	/// </summary>
    [Serializable()]
    public class Funcs : BaseObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Funcs"/> class.
        /// </summary>
        public Funcs() : base() { }

        /// <summary>
        /// Main Entry point of the Funcs object which inherits from a DataSet class
        /// </summary>
        /// <param name="DBName">The string name of the Database.</param>
        public Funcs(string DBName) : base(DBName) { }

        /// <summary>
        /// Retrieves the Function objects from the specified SQL server DB
        /// </summary>
        /// <param name="_connection">The SQL Connection used for the SQL server.</param>
        /// <param name="args"></param>
        public override void GetObject<Funcs>(SqlConnection _connection, params object[] args)
        {
            string cmd_GetFuncsList = string.Format(SqlQueryStrings.GetFuncsList, _connection.Database);
            using (SqlCommand _command_Funcs = new SqlCommand(cmd_GetFuncsList, _connection))
            {
                _command_Funcs.Prepare();
                using (SqlDataAdapter _sdaTables = new SqlDataAdapter(_command_Funcs))
                {
                    _sdaTables.Fill(this);
                }
            }
            if (this.Tables.Count > 0 && this.Tables[1].Rows.Count > 0)
            {
                // RENAME SECOND TABLE
                this.Tables[1].TableName = "FUNC";

                // add default tables 
                this.Tables.Add("FUNC_TEXT");
                this.Tables["FUNC_TEXT"].Columns.Add("FUNC_NAME");
                this.Tables.Add("FUNC_DEPENDS");
                this.Tables["FUNC_DEPENDS"].Columns.Add("FUNC_NAME");

                DataRow[] _arTables = new DataRow[this.Tables[1].Rows.Count];

                this.Tables[1].Rows.CopyTo(_arTables, 0);
                foreach (DataRow _dr in _arTables)
                {
                    DataSet _dsFields = null;
                    string _tableName = _dr["FUNC_NAME"].ToString();
                    string cmd_GetFuncSchema = string.Format(SqlQueryStrings.GetFuncSchema, _connection.Database, _tableName);
                    using (SqlCommand _command_Text = new SqlCommand(cmd_GetFuncSchema, _connection))
                    {
                        _command_Text.Prepare();
                        using (SqlDataAdapter _sdaFields = new SqlDataAdapter(_command_Text))
                        {
                            _dsFields = new DataSet("FUNC_SCHEMA");
                            _sdaFields.Fill(_dsFields);
                            _dsFields.AcceptChanges();
                        }
                    }
                    if (_dsFields != null && _dsFields.Tables.Count >= 1)
                    {
                        // rename returned tables from table[0] .. table[n]
                        // to the following values
                        _dsFields.Tables[0].TableName = "FUNC_TEXT";

                        if (_dsFields.Tables.Count >= 2)
                            _dsFields.Tables[1].TableName = "FUNC_DEPENDS";

                        AddMissingTableColumn(ref _dsFields, "FUNC_TEXT", "FUNC_NAME", false);
                        AddMissingTableColumn(ref _dsFields, "FUNC_DEPENDS", "FUNC_NAME", false);

                        try
                        {
                            foreach (DataTable _dt in _dsFields.Tables)
                            {
                                if (this.Tables[_dt.TableName] != null)
                                {
                                    if (_dt.Rows.Count == 0)
                                    {
                                        _dt.Rows.Add(_dt.NewRow());
                                    }
                                    DataColumn _dc = this.Tables[_dt.TableName].Columns["FUNC_NAME"];
                                    _dc.DefaultValue = _tableName;
                                    this.Merge(_dt, true, MissingSchemaAction.Add);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                this.Relations.Add("FuncsCreateTexts",
                    this.Tables["FUNC"].Columns["FUNC_NAME"],
                    this.Tables["FUNC_TEXT"].Columns["FUNC_NAME"]).Nested = true;
                this.Relations.Add("FuncsDepends",
                    this.Tables["FUNC"].Columns["FUNC_NAME"],
                    this.Tables["FUNC_DEPENDS"].Columns["FUNC_NAME"]).Nested = true;

                string source = string.Format("[{0}].[{1}]", _connection.DataSource, _connection.Database);
                logger.Debug("\n{1}, Catalog Schema, number of functions: {0}", this.Tables[1].Rows.Count, source);
            }
        }
    }
}
