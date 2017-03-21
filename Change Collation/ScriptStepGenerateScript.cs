using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace AlterCollation
{
    /// <summary>
    /// special script step that will select the results of executing a script and treat 
    /// each selected row as a SQL script
    /// </summary>
    public class ScriptStepGenerateScript : ScriptStep
    {
        private ScriptStepCollection script;

        public ScriptStepGenerateScript(string resourceIdentifier)
            : base(resourceIdentifier)
        {
        }
        public ScriptStepGenerateScript(ScriptStep basedOn)
            : base(basedOn.CommandText)
        {
        }

        protected override void ExecuteCommand(SqlCommand command)
        {
            script = new ScriptStepCollection();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                //this only selects the value in the first column
                while (reader.Read())
                    script.Add(new ScriptStep(reader.GetString(0)));
            }
            finally
            {
                reader.Close();
            }
        }
        public ScriptStepCollection Script
        {
            get { return script; }
        }

    }
}
