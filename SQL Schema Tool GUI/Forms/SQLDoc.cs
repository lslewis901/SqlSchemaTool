using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

using Lewis.SST.Controls;
using Lewis.SST.SQLMethods;
using Lewis.Xml;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

#region change history
/// 08-22-2008: C01: LLEWIS: add test for GO statements to break up executequery statements
#endregion

namespace Lewis.SST.Gui
{
    public partial class SQLDoc : Document
    {
        private SQLServerExplorer m_serverExplorer;

        public SQLDoc()
        { 
            InitializeComponent();

            SetTextEditorDefaultProperties();
            SetupMenuIemSelectCompare();

            SQLToolResourceSyntaxModeProvider provider = new SQLToolResourceSyntaxModeProvider();
            ICSharpCode.TextEditor.Document.HighlightingManager.Manager.AddSyntaxModeFileProvider(provider);
            txtEditCtl.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("SQL");

            // TODO: replace with SQL folding strategy
            XmlFoldingStrategy folding = new XmlFoldingStrategy();

            txtEditCtl.Document.FoldingManager.FoldingStrategy = (IFoldingStrategy)folding;

            WireEvents();
        }

        private void SetupMenuIemSelectCompare()
        {
            menuItem3.Name = "GenDatabase";
            menuItem3.Checked = false;
            menuItem3.Text = "Generate Database on Selected Server";
            menuItem3.Click += new EventHandler(GenDataBase_Click);
            menuItem4.Name = "";
            menuItem4.Text = "";
            menuItem4.Visible = false;
            menuItem5.Name = "Close";
            menuItem5.Text = "Close This";
            menuItem5.Click += new EventHandler(closeThis_Click);
        }

        private void GenDataBase_Click(object sender, EventArgs e)
        {
            RunSQLScript();
        }

        private void closeThis_Click(object sender, EventArgs e)
        {
            // Close doc
            Close();
        }

        public override bool Open(DockPanel dockPanel)
        {
            m_DialogTitle = "Open SQL File...";
            m_DialogTypeFilter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
            bool result = base.Open(dockPanel);
            FileName = m_fileName;
            if (result)
            {
                Show(dockPanel);
            }
            return result;
        }

		public override string FileName
		{
			get	{ return m_fileName; }
			set
			{
                if (value != null && value != string.Empty)
				{
                    m_fileName = value;
                    if (File.Exists(m_fileName))
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        FileInfo fi = new FileInfo(m_fileName);
                        this.TabText = fi.Name;
                        this.Text = fi.Name;
                        this.ToolTipText = fi.Name;
                        txtEditCtl.LoadFile(m_fileName, true, true);
                        ForceFoldingUpdate(m_fileName);
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        m_fileName = null;
                    }
                }
			}
		}

        public override void Save(bool showDialog)
        {
            m_DialogTitle = "Save SQL File As...";
            m_DialogTypeFilter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
            base.Save(showDialog);
        }

        public SQLServerExplorer ServerExplorer
        {
            set { m_serverExplorer = value; }
        }

        public void RunSQLScript()
        {
            if (m_serverExplorer != null)
            {
                SQLSecuritySettings sss = new SQLSecuritySettings(m_serverExplorer);
                DialogResult dr = sss.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    string connectionstr = string.Format(SQLConnections._secureConnect, sss.SelectedDBName, sss.SelectedSQLServer);
                    if (sss.SecurityMode() == SecurityType.Mixed)
                    {
                        connectionstr = string.Format(SQLConnections._secureConnect, sss.SelectedDBName, sss.SelectedSQLServer, sss.User, sss.PWD);
                    }
                    using (SqlConnection s = new SqlConnection(connectionstr))
                    {
                        int rows = 0;
                        try
                        {
                            s.InfoMessage += new SqlInfoMessageEventHandler(s_InfoMessage);
                            s.Open();
                            using (SqlCommand sc = s.CreateCommand())
                            {
                                // C01: LLEWIS: add test for GO statements to break up executequery statements
                                string[] statements = GetText().ToLower().Split(new string[]{"go"}, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string statement in statements)
                                {
                                    sc.CommandText = statement;
                                    try { sc.Prepare(); }
                                    catch { } // NOP, don't care about errors from this
                                    rows = sc.ExecuteNonQuery();                                    
                                }
                            } // dispose of SqlCommand: sc
                            s.Close();
                            s.InfoMessage -= new SqlInfoMessageEventHandler(s_InfoMessage);
                            MessageBox.Show("The SQL command was successful.", "SQL Info!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (SqlException sqe)
                        {
                            s.Close();
                            string error = string.Format(SQLSchemaTool.ERRORFORMAT, sqe.Message, sqe.Source, sqe.StackTrace);
                            logger.Warn(SQLSchemaTool.ERRORFORMAT, sqe.Message, sqe.Source, sqe.StackTrace);
                            HTMLDoc hDoc = new HTMLDoc("SQL Error Report");
                            hDoc.HTMLText = error;
                            hDoc.Show(this.DockPanel);
                            MessageBox.Show("There was an error executing the SQL command:\n" + error, "SQL ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else if (dr == DialogResult.Abort)
                {
                    logger.Warn("You must set the server security first!");
                    MessageBox.Show("You must set the server security first!\nUse the SQL Server Explorer.", "SQL ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        void s_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            string info = string.Format(SQLSchemaTool.ERRORFORMAT, e.Message, e.Source, string.Empty);
            logger.Info(SQLSchemaTool.ERRORFORMAT, e.Message, e.Source, string.Empty);
            HTMLDoc hDoc = new HTMLDoc("SQL Query Execution Info Report");
            hDoc.HTMLText = info;
            hDoc.Show(this.DockPanel);
        }
    }

    public class SQLToolResourceSyntaxModeProvider : ISyntaxModeFileProvider
    {
        List<SyntaxMode> syntaxModes = null;

        public ICollection<SyntaxMode> SyntaxModes
        {
            get
            {
                return syntaxModes;
            }
        }

        public SQLToolResourceSyntaxModeProvider()
        {
            Assembly assembly = this.GetType().Assembly;
            Stream syntaxModeStream = assembly.GetManifestResourceStream("Lewis.SST.Resources.SyntaxModes.xml");
            if (syntaxModeStream != null)
            {
                syntaxModes = SyntaxMode.GetSyntaxModes(syntaxModeStream);
            }
            else
            {
                syntaxModes = new List<SyntaxMode>();
            }
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            Assembly assembly = this.GetType().Assembly;
            return new XmlTextReader(assembly.GetManifestResourceStream("Lewis.SST.Resources." + syntaxMode.FileName));
        }

        public void UpdateSyntaxModeList()
        {
            // resources don't change during runtime
        }
    }
}