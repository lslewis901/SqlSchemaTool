using NLog;

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Lewis.SST.SQLObjects
{
    /// <summary>
    /// Base object class for all SQL objects
    /// </summary>
    [Serializable()]
    public abstract class BaseObject : System.Data.DataSet
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        [NonSerialized()]
        protected static Logger logger = LogManager.GetLogger("Lewis.SST.SQLObjects.BaseObject");

        /// <summary>
        /// db name
        /// </summary>
		[NonSerialized()]
        protected string _dbName = string.Empty;

        /// <summary>
        /// an arraylist used by the UDDTs class
        /// </summary>
        [NonSerialized()]
        protected ArrayList no_precision_types = new ArrayList();

        /// <summary>
        /// default
        /// </summary>
        public BaseObject(){}  // default ctor

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="DBName"></param>
        public BaseObject(string DBName)
        {
            no_precision_types.Add("int");
            no_precision_types.Add("smallint");
            no_precision_types.Add("bit");
            no_precision_types.Add("text");
            no_precision_types.Add("ntext");
            no_precision_types.Add("tinyint");
            no_precision_types.Add("datetime");
            no_precision_types.Add("uniqueidentifier");
            no_precision_types.Add("float");
            no_precision_types.Add("real");
            no_precision_types.Add("smalldatetime");
            no_precision_types.Add("image");

            _dbName = DBName;

            this.DataSetName = "DataBase_Schema";
            DataTable dt = this.Tables.Add("Database");

            this.EnforceConstraints = false;
            this.MergeFailed += new MergeFailedEventHandler(_MergeFailed);
        }

        /// <summary>
        /// abstract generic class to be overridden on child class
        /// </summary>
        /// <typeparam name="t"></typeparam>
        /// <param name="_connection"></param>
        /// <param name="args"></param>
        public abstract void GetObject<t>(SqlConnection _connection, params object[] args);

        /// <summary>
        /// Adds the specified _data table.
        /// </summary>
        /// <param name="_dataTable">The _data table.</param>
        public virtual void Add(DataTable _dataTable)
        {
            this.Tables.Add(_dataTable);
        }

        /// <summary>
        /// Gets the <see cref="DataTable"/> at the specified index.
        /// </summary>
        /// <value>The integer Index to select a DataTable from the DataSet</value>
        public virtual DataTable this[int Index]
        {
            get { return (DataTable)this.Tables[Index]; }
        }

        /// <summary>
        /// Handles the MergeFailed event of the UDTs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Data.MergeFailedEventArgs"/> instance containing the event data.</param>
        private void _MergeFailed(object sender, MergeFailedEventArgs e)
        {
            logger.Error("\nMerge failed on {0}, cause: {1}", e.Table, e.Conflict);
        }

        /// <summary>
        /// Adds the missing table and the missing name column.
        /// <para>
        /// sql select statements don't always return the same number of tables so
        /// we have to add one if it wasn't returned,
        /// otherwise our schema is only good to the lowest number of tables 
        /// returned by the SQL statement.
        /// </para>
        /// </summary>
        /// <param name="_dsFields">The _DS fields.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">the new column Name</param>
        /// <param name="addText"></param>
        protected void AddMissingTableColumn(ref DataSet _dsFields, string tableName, string columnName, bool addText)
        {
            if (!_dsFields.Tables.Contains(tableName))
            {
                _dsFields.Tables.Add(tableName);
            }
            if (this.Tables[tableName].Columns.Count == 0)
            {
                this.Tables[tableName].Columns.Add(columnName);
                if (addText)
                {
                    this.Tables[tableName].Columns.Add("Text");
                }
            }
        }

    }
}
