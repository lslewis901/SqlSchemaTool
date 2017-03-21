using System;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace AlterCollation
{
    public class ScriptStep
    {
        private string commandText;
        private ScriptRunState runState;
        private SqlException exception;

        public ScriptStep(string commandText)
        {
            this.commandText = commandText;
        }

        public ScriptRunState RunState
        {
            get { return runState; }
        }

        public string CommandText
        {
            get { return commandText; }
        }

        public void Execute(SqlConnection connection, IScriptExecuteCallback callback)
        {
            
            if (runState != ScriptRunState.None)
                throw new InvalidOperationException("Already Run");

            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            runState = ScriptRunState.Running;
            try
            {
                ExecuteCommand(command);
                runState = ScriptRunState.Succeeded;
            }
            catch(SqlException ex)
            {
                exception = ex;
                callback.Error(this, ex);
                runState = ScriptRunState.Failed;
            }
        }

        /// <summary>
        /// override this if data should be processed
        /// </summary>
        /// <param name="command"></param>
        protected virtual void ExecuteCommand(SqlCommand command)
        {
            command.ExecuteNonQuery();
        }

        public SqlException Exception
        {
            get { return exception; }
        }
    }
}
