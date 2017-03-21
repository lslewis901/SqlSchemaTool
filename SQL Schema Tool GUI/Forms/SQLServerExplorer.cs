using Lewis.SST.AsyncMethods;
using Lewis.SST.Controls;
using Lewis.SST.SQLMethods;
using Lewis.Xml.Converters;
using Lewis.Xml;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

#region change history
/// 08-22-2008: C01: LLEWIS:  added message box display about connection err
/// 03-09-2009: C02: LLEWIS: changes to handle errors if DataTable is null from 
///                          GetData method
#endregion

namespace Lewis.SST.Gui
{
    public partial class SQLServerExplorer : ToolWindow
    {
        /// <summary>
        /// Event handler declaration for the selected SQL server.
        /// </summary>
        public event EventHandler<ServerTreeEventArgs> SQLServerSelected;
        public event EventHandler<DBTreeEventArgs> DBSelected;
        public event EventHandler<DBTreeEventArgs> DBUnSelected;
        public event EventHandler<TableTreeEventArgs> TableSelected;
        public event EventHandler<TableTreeEventArgs> TableUnSelected;
        public event EventHandler<TableTreeEventArgs> TableCompareStarted;
        public event EventHandler<DBTreeEventArgs> SchemaGenerated;
        public event EventHandler<DBTreeEventArgs> SchemaGenerationStarted;
        public event EventHandler<SchemaGeneratedEventArgs> SchemaCompareStarted;
        public event EventHandler<SchemaGeneratedEventArgs> SchemaCompared;

        private string[] _servers = null;
        private SQLConnections sqlConnections;
        private TreeNode toplevel = null;

        private DBTreeNode[] selectedDBs = new DBTreeNode[2];
        private TableTreeNodes selectedTables = new TableTreeNodes();
        private int lastImageIndex = 0;

        private string xmlSourceFileName = null;
        private string xmlDestFileName = null;
        private string _lastDirectory = null;
        private byte _compareOptions = 0xff;
        private bool startCompare = false;
        private StringFormat stringFormat = null;
        private DockPane main = null;
        private bool expandFlag = false;
        private string _customSchemaXSLT = null;
        private string _customDataXSLT = null;

        private static Logger logger = LogManager.GetLogger("Lewis.SST.Gui.SQLServerExplorer");

        public enum SelectedTypes
        {
            DBs,
            XML,
            Tables,
            ALL
        }

        public SQLServerExplorer()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            treeView1.HideSelection = false;
            this.option1ToolStripMenuItem.Click += new EventHandler(mi_Refresh_Click);
            treeView1.Nodes.Clear();
            // add top level node.  This is used to start the tree
            toplevel = treeView1.Nodes.Add("SQL Servers");
            toplevel.Name = "TopLevel";
            toplevel.ImageIndex = 10;
            ContextMenuStrip toplevelMenu = new ContextMenuStrip();
            toplevelMenu.Items.Add(this.refreshTreeToolStripMenuItem);
            toplevelMenu.Items.Add(this.addSQLServerToolStripMenuItem);
            toplevel.ContextMenuStrip = toplevelMenu;
            toplevel.Nodes.Add("...");

            //treeView1.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            //treeView1.DrawNode += new DrawTreeNodeEventHandler(treeView1_DrawNode);
            //treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeView1_NodeMouseClick);
            //treeView1.ShowLines = false;
            //treeView1.CheckBoxes = true;
            stringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.LineLimit);
        }

        #region code not in use
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null && e.Node.Parent.Parent != null)
            {
                if (e.Node.IsSelected)
                {
                    treeView1.SelectedNode = e.Node;
                }
                if (e.Node.Checked)
                {
                    selectDBforCompare();
                }
                else
                {
                    deselectDB();
                }
            }
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // get bounds
            Rectangle bounds = e.Bounds;
            bounds.Inflate(-1, -1);
            //Rectangle newBounds = bounds;

            // get colors
            Color backColor = treeView1.BackColor;
            Color foreColor = e.Node.ForeColor;
            int idx = e.Node.ImageIndex < 0 ? 0 : e.Node.ImageIndex;

            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
            }
            else if ((e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot)
            {
                backColor = SystemColors.HotTrack;
                foreColor = SystemColors.HighlightText;
            }
            else
            {
                backColor = treeView1.BackColor;
                foreColor = e.Node.ForeColor;
            }
            if (e.Node.Parent == null || e.Node.Parent.Parent == null)
            {
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
                int left = 0;
                int left2 = 22;
                if (e.Node.Parent == null)
                {
                    left = 4;
                }
                else
                {
                    left = 30;
                    left2 += left;
                }
                e.Graphics.DrawImage(this.imageList1.Images[idx], new Point(e.Bounds.X + left, e.Bounds.Y));
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    e.Graphics.DrawString(e.Node.Text, treeView1.Font, brush, new Point(e.Bounds.X + left2, e.Bounds.Y), stringFormat);
                }
                if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
                {
                    ControlPaint.DrawFocusRectangle(e.Graphics, bounds);
                }
                e.DrawDefault = false;
            }
            else
            {
                e.DrawDefault = true;
            }
        }
        #endregion

        #region overrides

        protected override void OnRightToLeftLayoutChanged(EventArgs e)
        {
            treeView1.RightToLeftLayout = RightToLeftLayout;
        }

        #endregion

        #region public Methods

        public void addServer()
        {
            SQLSecuritySettings sss = SetSelectedServerSecurity(null);
        }

        public void RefreshSQLConnections()
        {
            Cursor.Current = Cursors.WaitCursor;
            string trimString = "'";
            char[] trimChar = trimString.ToCharArray();
            if (sqlConnections != null && sqlConnections.Count > 0)
            {
                foreach (TreeNode tn in toplevel.Nodes)
                {
                    #region If ServerTreeNodes Exists
                    if (typeof(ServerTreeNode).IsInstanceOfType(tn))
                    {
                        ServerTreeNode stn = (ServerTreeNode)tn;
                        if (stn.SQLServerConnection == null || stn.SQLServerConnection.State == ConnectionState.Closed)
                        {
                            if (sqlConnections[stn.Text] != null)
                            {
                                stn.SQLServerConnection = sqlConnections[stn.Text].sqlConnection;
                            }
                        }
                        if (stn.SQLServerConnection != null && (stn.SQLServerConnection.State == ConnectionState.Closed || stn.SQLServerConnection.State == ConnectionState.Broken))
                        {
                            try
                            {
                                string uid = null;
                                string security = null;
                                string password = null;
                                SecurityType securitysetting = SecurityType.Integrated;
                                string[] connections = stn.SQLServerConnection.ConnectionString.Split(';');
                                foreach (string s in connections)
                                {
                                    if (s.ToLower().Contains("integrated security"))
                                    {
                                        string[] words = s.Split(new char[] { '=' });
                                        if (words.Length > 1)
                                        {
                                            security = words[1];
                                        }
                                        if (security.ToLower().Equals("false"))
                                        {
                                            securitysetting = SecurityType.Mixed;
                                        }
                                    }
                                    if (s.ToLower().Contains("user id"))
                                    {
                                        string[] words = s.Split(new char[] { '=' });
                                        if (words.Length > 1)
                                        {
                                            uid = words[1].Trim(trimChar);
                                        }
                                    }
                                    if (s.ToLower().Contains("password"))
                                    {
                                        string[] words = s.Split(new char[] { '=' });
                                        if (words.Length > 1)
                                        {
                                            password = words[1].Trim(trimChar);
                                        }
                                    }
                                }
                                stn.UID = uid;
                                stn.Pwd = password;
                                if (password == null && sqlConnections != null && sqlConnections[stn.Text] != null)
                                {
                                    stn.Pwd = sqlConnections[stn.Text].Password;
                                }
                                if (stn.Pwd != null && stn.Pwd.Length > 0)
                                {
                                    stn.SavePWD = true;
                                }
                                stn.Security = securitysetting;
                            }
                            catch { }
                        }
                    }
                    #endregion
                }
            }
            Cursor.Current = Cursors.Default;
        }

        public void RefreshTreeView(bool Force)
		{
			Cursor.Current = Cursors.WaitCursor;
            treeView1.CollapseAll();
            //if (Force) getServers();

            TreeNode[] tnc = null;
            if (toplevel.Nodes.Count > 0 )
            {
                for (int ii = 0; ii < toplevel.Nodes.Count; ii++)
                {
                    if (!typeof(ServerTreeNode).IsInstanceOfType(toplevel.Nodes[ii]))
                    {
                        toplevel.Nodes[ii].Remove();
                    }
                }
                // save off existing child tree nodes
                tnc = new TreeNode[toplevel.Nodes.Count];
                if (toplevel.Nodes.Count > 0)
                {
                    toplevel.Nodes.CopyTo(tnc, 0);
                }
            }
            toplevel.Nodes.Clear();

			// populate SQL server tree view
            for (int zz = 0; sqlConnections != null && zz < sqlConnections.Count; zz++)
            {
                bool addFlag = true;
                if (_servers != null)
                {
                    foreach (string server in _servers)
                    {
                        // Add persisted SQL servers that are not a part of the Server list returned above
                        if (sqlConnections[zz].Server.ToLower().Equals(server.ToLower()))
                        {
                            addFlag = false;
                            break;
                        }
                    }
                }
                if (addFlag)
                {
                    ServerTreeNode sn = new ServerTreeNode(sqlConnections[zz].Server);
                    sn.UID = sqlConnections[zz].User;
                    sn.Pwd = sqlConnections[zz].Password;
                    sn.SavePWD = sqlConnections[zz].SavePassword;
                    sn.Connected = sqlConnections[zz].sqlConnection.State == ConnectionState.Open;
                    sn.Security = sqlConnections[zz].securityType;
                    sn.SQLServerConnection = sqlConnections[zz].sqlConnection;
                    sn.Nodes.Clear();
                    if (sn.SQLServerConnection != null && sn.SQLServerConnection.State == ConnectionState.Open)
                    {
                        sn.Nodes.Add("...");
                        sn.Connected = true;
                        sn.ImageIndex = 11;
                        sn.SelectedImageIndex = 11;
                        populateChildNodes((ExtTreeNode)sn);
                    }
                    else
                    {
                        sn.Nodes.Add("...");
                        sn.ImageIndex = 12;
                        sn.SelectedImageIndex = 12;
                        sn.Nodes[0].ImageIndex = 16;
                    }
                    sn.ContextMenuStrip = this.ServerMenuStrip;
                    // add all the servers to the top level node
                    toplevel.Nodes.Add(sn);
                }
            }
			if (_servers != null)
			{
				foreach(string server in _servers)
				{
                    string uid = null;
                    string pwd = null;
                    bool save = false;
                    bool connected = false;
                    SecurityType security = SecurityType.Integrated;
                    if (tnc != null)
                    {
                        for (int ii = 0; ii < tnc.Length; ii ++ )
                        {
                            ServerTreeNode tn = (ServerTreeNode)tnc[ii];
                            if (tn != null && tn.Text.ToLower().Equals(server.ToLower()))
                            {
                                uid = tn.UID;
                                pwd = tn.Pwd;
                                save = tn.SavePWD;
                                connected = tn.Connected;
                                security = tn.Security;
                                tnc[ii] = null;
                                break;
                            }
                        }
                    }
                    ServerTreeNode sn = new ServerTreeNode(server);
                    sn.UID = uid;
                    sn.Pwd = pwd;
                    sn.SavePWD = save;
                    sn.Connected = connected;
                    sn.Security = security;
                    if (sqlConnections != null && sqlConnections[server] != null)
                    {
                        sn.SQLServerConnection = sqlConnections[server].sqlConnection;
                    }
                    else
                    {
                        OpenSQLConnection(sn, false);
                    }
                    if (sn.SQLServerConnection != null && sn.SQLServerConnection.State == ConnectionState.Open)
                    {
                        sn.Nodes.Add("...");
                        sn.Connected = true;
                        sn.ImageIndex = 11;
                        sn.SelectedImageIndex = 11;
                        populateChildNodes((ExtTreeNode)sn);
                    }
                    else
                    {
                        sn.Nodes.Add("...");
                        sn.ImageIndex = 12;
                        sn.SelectedImageIndex = 12;
                        sn.Nodes[0].ImageIndex = 16;
                    }
                    sn.ContextMenuStrip = this.ServerMenuStrip;
                    // add all the servers to the top level node
                    toplevel.Nodes.Add(sn);
				}
			}
            if (tnc != null)
            {
                foreach (ServerTreeNode tn in tnc)
                {
                    if (tn != null)
                    {
                        if (toplevel.Nodes[tn.Text] == null)
                        {
                            toplevel.Nodes.Add(tn);
                        }
                    }
                }
            }
            tnc = null;
            if (Force)
            {
                RefreshSQLConnections();
            }

			Cursor.Current = Cursors.Default;
		}

        public SQLSecuritySettings SetSelectedServerSecurity(ServerTreeNode stn)
        {
            SQLSecuritySettings sss = null;
            bool addToTreeFlag = false;
            if (stn == null)
            {
                stn = new ServerTreeNode("Enter Server Name...");
                stn.Security = SecurityType.Integrated;
                ArrayList arl = new ArrayList();
                arl.Add("Enter Server Name...");
                if (_servers != null) arl.AddRange(_servers);
                sss = new SQLSecuritySettings((string[])arl.ToArray(typeof(string)), true);
                addToTreeFlag = true;
            }
            else if (stn.LastMouseButton != MouseButtons.Right)
            {
                return null;
            }
            else if (stn.Security == SecurityType.Integrated)
            {
                sss = new SQLSecuritySettings(stn.Text);
            }
            else
            {
                sss = new SQLSecuritySettings(stn.Text, stn.UID, stn.SavePWD ? stn.Pwd : "", stn.SavePWD);
            }
            DialogResult dr = sss.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                if (!stn.Text.Equals(sss.SelectedSQLServer))
                {
                    stn.Text = sss.SelectedSQLServer;
                }
                stn.SavePWD = sss.SavePwd;
                stn.Security = sss.SecurityMode();
                if (sss.SecurityMode() == SecurityType.Mixed)
                {
                    stn.UID = sss.User;
                    stn.Pwd = sss.PWD;
                }
                else
                {
                    stn.UID = null;
                    stn.Pwd = null;
                }
                if (OpenSQLConnection(stn, true))
                {
                    stn.ImageIndex = 11;
                    stn.SelectedImageIndex = 11;
                    populateChildNodes((ExtTreeNode)stn);
                    OnSQLServerSelected(stn);
                }
                else
                {
                    // C01: LLEWIS:  added message box display about connection err
                    MessageBox.Show(string.Format("Unable to connect to server: {0}", stn.Text), "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    stn.Nodes.Clear();
                    stn.Nodes.Add("...");
                    stn.ImageIndex = 12;
                    stn.SelectedImageIndex = 12;
                    stn.Nodes[0].ImageIndex = 16;
                }
                stn.ContextMenuStrip = this.ServerMenuStrip;
                if (addToTreeFlag)
                {
                    toplevel.Nodes.Add(stn);
                    treeView1.SelectedNode = stn;
                    treeView1.Sort();
                }
            }
            return sss;
        }

        public void OpenDTSSerializer(ServerTreeNode node)
        {
            if (node.Connected)
            {
                if (main == null)
                {
                    main = (DockPane)this.Parent;
                }
                IDockContent[] idca = main.DockPanel.GetDocuments();
                foreach (IDockContent idc in idca)
                {
                    if (idc.DockHandler.TabText.Equals(node.Text))
                    {
                        idc.DockHandler.Show(main.DockPanel);
                        return;
                    }
                }
                DTSDoc dtsDoc = new DTSDoc(node.Text);
                //dtsDoc.RightToLeftLayout = RightToLeftLayout;
                dtsDoc.Show(main.DockPanel);
                dtsDoc.RefreshGrid(node.SQLServerConnection);
            }
        }

        public bool StartAsyncSchemaGeneration(DBTreeNode DBnode, bool translate)
        {
            bool retval = false;
            ServerTreeNode node = (ServerTreeNode)DBnode.Parent;
            if (node.Connected)
            {
                string fileName = null;
                string title = null;
                string filter = null;
                if (translate)
                {
                    fileName = node.Text.ToLower().Replace("\\", "_").Replace(":", "-") + "_" + DBnode.Text.ToLower().Replace("\\", "_").Replace(":", "-") + "_schema.sql";
                    title = "Save SQL Schema File as...";
                    filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                }
                else
                {
                    fileName = node.Text.ToLower().Replace("\\", "_").Replace(":", "-") + "_schema.sql";
                    title = "Save XML Schema Snapshot File as...";
                    filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                }
                ArrayList arl = ShowSaveFileDialog(fileName, title, filter);
                DialogResult dr = (DialogResult)arl[0];

                if (dr == DialogResult.OK)
                {
                    Cursor.Current = Cursors.AppStarting;
                    AsyncSerializeDB asyncSerializeDB = new AsyncSerializeDB(this, node.Text, DBnode.Text, node.UID, node.Pwd, (string)arl[1], translate, false, _compareOptions, _customSchemaXSLT);
                    asyncSerializeDB.CompleteSerializeDB += new AsyncSerializeDB.CompleteSerializeDBDelegate(asyncSerializeDB_CompleteSerializeDB);
                    asyncSerializeDB.Start();
                    OnSchemaGenerationStarted(DBnode);
                    retval = true;
                }
            }
            return retval;
        }

        public void StartAsyncSchemaCompare(DBTreeNode[] SelectedDBs, string[] xmlSnapShotFileNames)
        {
            selectedDBs = SelectedDBs;
            StartAsyncSchemaCompare<object>(xmlSnapShotFileNames);
        }

        public void StartAsyncSchemaCompare(DBTreeNode[] SelectedDBs)
        {
            selectedDBs = SelectedDBs;
            StartAsyncSchemaCompare<object>(null);
        }

        public void StartAsyncSchemaCompare<T>(object obj)
        {
            string[] xmlSnapShotFileNames = (string[])obj;
            bool DBsOnly = (selectedDBs[0] != null && selectedDBs[1] != null && (xmlSnapShotFileNames == null || (xmlSnapShotFileNames != null && xmlSnapShotFileNames.Length > 1 && xmlSnapShotFileNames[0] == null && xmlSnapShotFileNames[1] == null)));
            bool XSnpShtOnly = (xmlSnapShotFileNames != null && xmlSnapShotFileNames.Length > 1 && xmlSnapShotFileNames[0] != null && xmlSnapShotFileNames[1] != null);
            bool DBSrcXSnpShtDest = (selectedDBs[0] != null && selectedDBs[1] == null && xmlSnapShotFileNames != null && xmlSnapShotFileNames.Length > 1 && xmlSnapShotFileNames[0] == null && xmlSnapShotFileNames[1] != null);
            bool XSnpShtSrcDBDest = (selectedDBs[0] == null && selectedDBs[1] != null && xmlSnapShotFileNames != null && xmlSnapShotFileNames.Length > 1 && xmlSnapShotFileNames[0] != null && xmlSnapShotFileNames[1] == null);
            bool truthTable = DBsOnly || XSnpShtOnly || DBSrcXSnpShtDest || XSnpShtSrcDBDest;
            if (truthTable)
            {
                string source = null;
                string dest = null;
                string diffXML = null;
                string sqlFileOutput = null;
                string m_DialogTitle = "Save SQL File As...";
                string m_DialogTypeFilter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                ServerTreeNode sourceNode = null;
                ServerTreeNode destNode = null;

                if (DBsOnly)
                {
                    sourceNode = (ServerTreeNode)selectedDBs[0].Parent;
                    destNode = (ServerTreeNode)selectedDBs[1].Parent;
                    source = string.Format(SQLSchemaTool._OUTPUTFILE, sourceNode.Text.Replace("\\", "_").Replace(":", "-"), selectedDBs[0].Text).ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                    dest = string.Format(SQLSchemaTool._OUTPUTFILE, destNode.Text.Replace("\\", "_").Replace(":", "-"), selectedDBs[1].Text).ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                }
                else if (XSnpShtOnly)
                {
                    FileInfo fiSource = new FileInfo(xmlSnapShotFileNames[0]);
                    FileInfo fiDest = new FileInfo(xmlSnapShotFileNames[1]);
                    xmlSourceFileName = xmlSnapShotFileNames[0];
                    xmlDestFileName = xmlSnapShotFileNames[1];
                    source = fiSource.Name.ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                    dest = fiDest.Name.ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                }
                else if (DBSrcXSnpShtDest)
                {
                    sourceNode = (ServerTreeNode)selectedDBs[0].Parent;
                    FileInfo fiDest = new FileInfo(xmlSnapShotFileNames[1]);
                    xmlDestFileName = xmlSnapShotFileNames[1];
                    source = string.Format(SQLSchemaTool._OUTPUTFILE, sourceNode.Text.Replace("\\", "_").Replace(":", "-"), selectedDBs[0].Text).ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                    dest = fiDest.Name.ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                }
                else if (XSnpShtSrcDBDest)
                {
                    FileInfo fiSource = new FileInfo(xmlSnapShotFileNames[0]);
                    xmlSourceFileName = xmlSnapShotFileNames[0];
                    destNode = (ServerTreeNode)selectedDBs[1].Parent;
                    source = fiSource.Name.ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                    dest = string.Format(SQLSchemaTool._OUTPUTFILE, destNode.Text.Replace("\\", "_").Replace(":", "-"), selectedDBs[1].Text).ToLower().Replace("_schema", "").Replace(".xml", "").Replace("\\", "_").Replace(":", "-");
                }
                diffXML = string.Format(SQLSchemaTool._DIFFFILE, source, dest);
                sqlFileOutput = diffXML.ToLower().Replace("xml", "sql");

                ArrayList arl = ShowSaveFileDialog(sqlFileOutput, m_DialogTitle, m_DialogTypeFilter);
                DialogResult dr = (DialogResult)arl[0];

                if (dr == DialogResult.OK)
                {
                    startCompare = true;

                    if (DBsOnly)
                    {
                        AsyncSerializeDB asyncSourceSerializeDB = new AsyncSerializeDB(this, sourceNode.Text, selectedDBs[0].Text, sourceNode.UID, sourceNode.Pwd, null, false, false, _compareOptions, _customSchemaXSLT);
                        asyncSourceSerializeDB.CompleteSerializeDB += new AsyncSerializeDB.CompleteSerializeDBDelegate(asyncSourceSerializeDB_CompleteSerializeDB);
                        asyncSourceSerializeDB.Start();
                        OnSchemaGenerationStarted(selectedDBs[0]);

                        AsyncSerializeDB asyncDestSerializeDB = new AsyncSerializeDB(this, destNode.Text, selectedDBs[1].Text, destNode.UID, destNode.Pwd, null, false, false, _compareOptions, _customSchemaXSLT);
                        asyncDestSerializeDB.CompleteSerializeDB += new AsyncSerializeDB.CompleteSerializeDBDelegate(asyncDestSerializeDB_CompleteSerializeDB);
                        asyncDestSerializeDB.Start();
                        OnSchemaGenerationStarted(selectedDBs[1]);

                        // wait for both serializations to complete, this is the painful part
                        while (!asyncSourceSerializeDB.IsDone && !asyncDestSerializeDB.IsDone)
                        {
                            Application.DoEvents();
                        }
                    }
                    else if (DBSrcXSnpShtDest)
                    {
                        AsyncSerializeDB asyncSourceSerializeDB = new AsyncSerializeDB(this, sourceNode.Text, selectedDBs[0].Text, sourceNode.UID, sourceNode.Pwd, null, false, false, _compareOptions, _customSchemaXSLT);
                        asyncSourceSerializeDB.CompleteSerializeDB += new AsyncSerializeDB.CompleteSerializeDBDelegate(asyncSourceSerializeDB_CompleteSerializeDB);
                        asyncSourceSerializeDB.Start();
                        OnSchemaGenerationStarted(selectedDBs[0]);

                        // wait for serialization to complete, this is the painful part
                        while (!asyncSourceSerializeDB.IsDone)
                        {
                            Application.DoEvents();
                        }
                    }
                    else if (XSnpShtSrcDBDest)
                    {
                        AsyncSerializeDB asyncDestSerializeDB = new AsyncSerializeDB(this, destNode.Text, selectedDBs[1].Text, destNode.UID, destNode.Pwd, null, false, false, _compareOptions, _customSchemaXSLT);
                        asyncDestSerializeDB.CompleteSerializeDB += new AsyncSerializeDB.CompleteSerializeDBDelegate(asyncDestSerializeDB_CompleteSerializeDB);
                        asyncDestSerializeDB.Start();
                        OnSchemaGenerationStarted(selectedDBs[1]);

                        // wait for serialization to complete, this is the painful part
                        while (!asyncDestSerializeDB.IsDone)
                        {
                            Application.DoEvents();
                        }
                    }
                    while (xmlSourceFileName == null || xmlDestFileName == null)
                    {
                        Application.DoEvents();
                    }

                    Cursor.Current = Cursors.AppStarting;
                    sqlFileOutput = (string)arl[1];
                    AsyncCompareDB compareDB = new AsyncCompareDB(this, xmlSourceFileName, xmlDestFileName, diffXML, sqlFileOutput, false, false, true, _compareOptions, _customSchemaXSLT);
                    compareDB.CompleteCompareDB += new AsyncCompareDB.CompleteCompareDBDelegate(compareDB_CompleteCompareDB);
                    compareDB.Start();
                    string[] filenames = new string[2];
                    filenames[0] = diffXML;
                    filenames[1] = sqlFileOutput;
                    OnSchemaCompareStarted(filenames);

                    startCompare = false;
                }
            }
        }

        public void ClearSelected(SelectedTypes st)
        {
            if (st == SelectedTypes.ALL || st == SelectedTypes.XML)
            {
                xmlDestFileName = null;
                xmlSourceFileName = null;
            }
            if (st == SelectedTypes.ALL || st == SelectedTypes.DBs)
            {
                selectedDBs[0] = null;
                selectedDBs[1] = null;
            }
            if (st == SelectedTypes.Tables || st == SelectedTypes.ALL)
            {
                selectedTables.Remove(selectedTables[0]);
                selectedTables.InsertAt(0, new TableTreeNode());
                selectedTables.Remove(selectedTables[1]);
                selectedTables.InsertAt(1, new TableTreeNode());
            }
        }

        public void deselectDBTables()
        {
            if (selectedTables[0] != null)
            {
                TreeNode tableNode = selectedTables[0];
                tableNode.ForeColor = System.Drawing.Color.Black;
                tableNode.BackColor = System.Drawing.Color.White;
                tableNode.ImageIndex = 23;
                tableNode.SelectedImageIndex = 23;
                tableNode.Checked = false;
                tableNode.Tag = null;
                OnTableUnSelected((TableTreeNode)selectedTables[0]);
            }
            if (selectedTables[1] != null)
            {
                TreeNode tableNode = selectedTables[1];
                tableNode.ForeColor = System.Drawing.Color.Black;
                tableNode.BackColor = System.Drawing.Color.White;
                tableNode.ImageIndex = 23;
                tableNode.SelectedImageIndex = 23;
                tableNode.Checked = false;
                tableNode.Tag = null;
                OnTableUnSelected((TableTreeNode)selectedTables[1]);
            }
            clearTableNodes(treeView1.Nodes);
            selectedTables.Clear();
        }

        public void clearTableNodes(TreeNodeCollection tnc)
        {
            foreach (TreeNode ttn in tnc)
            {
                if (ttn is TableTreeNode)
                {
                    ttn.ForeColor = System.Drawing.Color.Black;
                    ttn.BackColor = System.Drawing.Color.White;
                    ttn.ImageIndex = 23;
                    ttn.SelectedImageIndex = 23;
                    ttn.Checked = false;
                    ttn.Tag = null;
                }
                else if (ttn.Nodes.Count > 0)
                {
                    clearTableNodes(ttn.Nodes);
                }
            }
        }

        public string ExportSelectedTable(Export.ExportFormat format)
        {
            String fileName = string.Empty;
            TableTreeNode[] tables = null;
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode) &&
                ((TableTreeNode)treeView1.SelectedNode).SelectedForCompare && selectedTables.Tables.Length > 0)
            {
                tables = selectedTables.Tables;
            }
            else
            {
                tables = new TableTreeNode[] { (TableTreeNode)treeView1.SelectedNode };
            }
            // TODO:  add code to display message for one file output per selected table
            // when multiple selected
            //if (tables.Length > 1)
            //{
            //    DialogResult drQuestion = MessageBox.Show("Return all selected in one file?", "Single File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            //    if (drQuestion == DialogResult.Yes)
            //    {
            //        string name = ((DBTreeNode)treeView1.SelectedNode.Parent).FullDBPath.Replace("\\", "_").Replace(":", "-");
            //        string title = string.Empty;
            //        string filter = string.Empty;
            //        switch (format)
            //        {
            //            case Export.ExportFormat.Excel:
            //                {
            //                    fileName = name + ".xls";
            //                    title = "Save Excel Data File as...";
            //                    filter = "XLS files (*.xls)|*.xls|All files (*.*)|*.*";
            //                }
            //                break;
            //            case Export.ExportFormat.CSV:
            //                {
            //                    fileName = name + ".csv";
            //                    title = "Save CSV Data File as...";
            //                    filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            //                }
            //                break;
            //            case Export.ExportFormat.XML:
            //                {
            //                    fileName = name + ".xml";
            //                    title = "Save XML Data File as...";
            //                    filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            //                }
            //                break;
            //        }
            //        DataSet ds = new DataSet();
            //        DataTableCollection dtc = ds.Tables;
            //        foreach (TableTreeNode ttn in tables)
            //        {
            //            SqlConnection connection = ((ServerTreeNode)ttn.Parent.Parent).SQLServerConnection;
            //            DataTable dt = SQLData.GetData("select * from " + ttn.FullTablePath, connection);
            //            dtc.Add(dt);
            //        }
            //        ArrayList arl = ShowSaveFileDialog(fileName, title, filter);
            //        DialogResult dr = (DialogResult)arl[0];
            //        if (dr == DialogResult.OK)
            //        {
            //            fileName = arl[1].ToString();
            //            Export exp = new Export("Win");
            //            if (format == Export.ExportFormat.Excel)
            //            {
            //                exp.ExportDataSetToExcel(ds, fileName);
            //            }
            //            else
            //            {
            //                exp.ExportData(dtc, format, fileName);
            //            }
            //        }
            //        return fileName;
            //    }
            //    else if (drQuestion == DialogResult.Cancel)
            //    {
            //        return null;
            //    }
            //}
            foreach(TableTreeNode ttn in tables)
            {
                switch (format)
                {
                    case Export.ExportFormat.Excel:
                        {
                            fileName = ttn.FullTablePath.Replace("\\", "_").Replace(":", "-") + ".xls";
                            String title = "Save Excel Data File as...";
                            String filter = "XLS files (*.xls)|*.xls|All files (*.*)|*.*";
                            ArrayList arl = ShowSaveFileDialog(fileName, title, filter);
                            DialogResult dr = (DialogResult)arl[0];

                            if (dr == DialogResult.OK)
                            {
                                fileName = arl[1].ToString();
                                SqlConnection connection = ((ServerTreeNode)ttn.Parent.Parent).SQLServerConnection;
                                DataTable dt = SQLData.GetData("select * from " + ttn.FullTablePath, connection);
                                if (dt != null)
                                {
                                    Export exp = new Export("Win");
                                    exp.ExportData(dt, Export.ExportFormat.Excel, fileName);
                                }
                            }
                            break;
                        }
                    case Export.ExportFormat.CSV:
                        {
                            fileName = ttn.FullTablePath.Replace("\\", "_").Replace(":", "-") + ".csv";
                            String title = "Save CSV Data File as...";
                            String filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                            ArrayList arl = ShowSaveFileDialog(fileName, title, filter);
                            DialogResult dr = (DialogResult)arl[0];

                            if (dr == DialogResult.OK)
                            {
                                fileName = arl[1].ToString();
                                SqlConnection connection = ((ServerTreeNode)ttn.Parent.Parent).SQLServerConnection;
                                DataTable dt = SQLData.GetData("select * from " + ttn.FullTablePath, connection);
                                if (dt != null)
                                {
                                    Export exp = new Export("Win");
                                    exp.ExportData(dt, Export.ExportFormat.CSV, fileName);
                                }
                            }
                            break;
                        }
                    case Export.ExportFormat.XML:
                        {
                            fileName = ttn.FullTablePath.Replace("\\", "_").Replace(":", "-") + ".xml";
                            string querySource = "select * from " + ttn.FullTablePath;
                            String title = "Save XML Data File as...";
                            String filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                            ArrayList arl = ShowSaveFileDialog(fileName, title, filter);
                            DialogResult dr = (DialogResult)arl[0];

                            if (dr == DialogResult.OK)
                            {
                                fileName = arl[1].ToString();
                                SqlConnection connection = ((ServerTreeNode)ttn.Parent.Parent).SQLServerConnection;
                                XmlDataDocument original = new XmlDataDocument();
                                string xmlData = SQLData.GetXMLData(querySource, connection);
                                // add transformation using attrib to element xslt.
                                if (_customDataXSLT != null)
                                {
                                    try
                                    {
                                        FileInfo fi = new FileInfo(_customDataXSLT);
                                        File.WriteAllText(fileName, XsltHelper.Transform(xmlData, fi));
                                        LoadXMLDoc(fileName);
                                        logger.Info("\nThe Custom XSLT {0}, has been applied and saved as {1}.", _customDataXSLT, fileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                                        MessageBox.Show(string.Format("Unable to process output with custom XSLT {0}\nError was caused by {1}", _customDataXSLT, ex.Message), "XSLT Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    original.LoadXml(XsltHelper.Transform(xmlData, XsltHelper.ATTRSPACEXSLT));
                                    original.Save(fileName);
                                    LoadXMLDoc(fileName);
                                }
                            }
                        }
                        break;
                }
            }
            return fileName;
        }

        public void getServers()
        {
            Cursor.Current = Cursors.WaitCursor;
            _servers = SQLMethods.SQLServers.GetSQLServers();
            Cursor.Current = Cursors.Default;
        }

        public void GenerateTableSchema()
        {
            string tableNames = null;
            TableTreeNode[] tables = null;
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode) &&
                ((TableTreeNode)treeView1.SelectedNode).SelectedForCompare && selectedTables.Tables.Length > 0)
            {
                tables = selectedTables.Tables;
            }
            else
            {
                tables = new TableTreeNode[] { (TableTreeNode)treeView1.SelectedNode };
            }
            foreach (TableTreeNode ttn in tables)
            {
                tableNames += "'" + ttn.Text + "',";
            }
            tableNames = string.IsNullOrEmpty(tableNames) ? null : " (" + tableNames.TrimEnd(',') + ")";
            // generate Table level Schema as XML and ...
            {
                TableTreeNode ttn = (TableTreeNode)tables[0];
                DBTreeNode dbNode = (DBTreeNode)ttn.Parent;
                ServerTreeNode sNode = (ServerTreeNode)dbNode.Parent;
                string fileName = dbNode.FullDBPath.Replace("\\", "_").Replace(":", "-") + "_" + tableNames.Replace(",", "_") + ".XML";
                string fileNames = SQLSchemaTool.SerializeDB(sNode.Text, dbNode.Text, sNode.UID, sNode.Pwd, fileName, true, true, null, Convert.ToByte(SQLSchemaTool._NodeType.TABLE), _customSchemaXSLT, tableNames);

                if (fileNames.Split(',').Length > 0)
                {
                    LoadXMLDoc(fileNames.Split(',')[0]);
                }
                if (fileNames.Split(',').Length > 1)
                {
                    LoadSQLDoc(fileNames);
                }
            }
        }

		#endregion

		#region delegate event handler methods

        /// <summary>
		/// Called when [SQL server selected].
		/// </summary>
		/// <param name="SqlServer">The SQL server eventarg.</param>
		private void OnSQLServerSelected(ServerTreeNode node)
		{
            ServerTreeEventArgs args = new ServerTreeEventArgs();
            args.ServerTreeNode = node;

            EventHandler<ServerTreeEventArgs> handler = SQLServerSelected;
            if (handler != null) 
			{
				// raises the event. 
                handler(this, args);
			}
		}

        private void OnDBSelected(DBTreeNode node)
        {
            DBTreeEventArgs args = new DBTreeEventArgs();
            args.DBTreeNode = node;

            EventHandler<DBTreeEventArgs> handler = DBSelected;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnDBUnSelected(DBTreeNode node)
        {
            DBTreeEventArgs args = new DBTreeEventArgs();
            args.DBTreeNode = node;

            EventHandler<DBTreeEventArgs> handler = DBUnSelected;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnTableSelected(TableTreeNode node)
        {
            TableTreeEventArgs args = new TableTreeEventArgs();
            args.TableTreeNode = node;

            EventHandler<TableTreeEventArgs> handler = TableSelected;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnTableUnSelected(TableTreeNode node)
        {
            TableTreeEventArgs args = new TableTreeEventArgs();
            args.TableTreeNode = node;

            EventHandler<TableTreeEventArgs> handler = TableUnSelected;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnTableCompareStarted(TableTreeNode node)
        {
            TableTreeEventArgs args = new TableTreeEventArgs();
            args.TableTreeNode = node;

            EventHandler<TableTreeEventArgs> handler = TableCompareStarted;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSchemaGenerationStarted(DBTreeNode node)
        {
            DBTreeEventArgs args = new DBTreeEventArgs();
            args.DBTreeNode = node;

            EventHandler<DBTreeEventArgs> handler = SchemaGenerationStarted;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSchemaGenerated(DBTreeNode node)
        {
            DBTreeEventArgs args = new DBTreeEventArgs();
            args.DBTreeNode = node;

            EventHandler<DBTreeEventArgs> handler = SchemaGenerated;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSchemaCompareStarted(string[] Filenames)
        {
            SchemaGeneratedEventArgs args = new SchemaGeneratedEventArgs();
            args.Filenames = Filenames;

            EventHandler<SchemaGeneratedEventArgs> handler = SchemaCompareStarted;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSchemaCompared(string[] Filenames)
        {
            SchemaGeneratedEventArgs args = new SchemaGeneratedEventArgs();
            args.Filenames = Filenames;
            xmlSourceFileName = null;
            xmlDestFileName = null;
            EventHandler<SchemaGeneratedEventArgs> handler = SchemaCompared;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        #endregion

		#region local control Event handlers

        private void hideListOfDataTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                deselectDBTables();
                treeView1.SelectedNode.Nodes.Clear();
            }
        }

        private void removeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (typeof(ServerTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                DialogResult dr = MessageBox.Show("Are you sure you want to remove the server: [" + treeView1.SelectedNode.Text + "]?", "Remove Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    ServerTreeNode stn = (ServerTreeNode)treeView1.SelectedNode;
                    sqlConnections.Remove(stn.Text);
                    treeView1.SelectedNode.Remove();
                }
            }
        }

        private void addSQLServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addServer();
        }

        private void mi_Refresh_Click(object sender, EventArgs e)
		{
            getServers();
            RefreshTreeView(true);
        }

		private void mi_Security_Click(object sender, EventArgs e)
		{
            if (typeof(ServerTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                ServerTreeNode stn = (ServerTreeNode)treeView1.SelectedNode;
                SetSelectedServerSecurity(stn);
            }
		}

        private void selectForCompareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectDBforCompare();
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            lastImageIndex = e.Node.ImageIndex;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // this event is fired when the treeview node is selected via normal treeview stuff by single click
            // we are selecting our nodes using a dbl click which requires some extra footwork
            if (e.Node == null) return;
            // need this to maintain image state
            e.Node.SelectedImageIndex = lastImageIndex;
            if (e.Action == TreeViewAction.Expand)
            {
            }
            else 
            {
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (expandFlag) return;
            expandFlag = true;
            try
            {
                // if the selected node was a server then show children Objects
                if (typeof(ExtTreeNode).IsInstanceOfType(e.Node))
                {
                    treeView1.SelectedNode = e.Node;
                    populateChildNodes((ExtTreeNode)e.Node);
                }
                else
                {
                    if (e.Node.Name.Equals("TopLevel"))
                    {
                        RefreshTreeView(false);
                    }
                }
            }
            catch { }
            finally
            {
                expandFlag = false;
            }
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
		{
            MouseButtons mouseButton = e.Button;
			Point p = new Point(e.X, e.Y);
            TreeNode tn = CheckNodeForMouseUp(toplevel, p, mouseButton);
            if (tn != null)
            {
                if (mouseButton == MouseButtons.Right)
                {
                    if (tn.ContextMenuStrip != null)
                    {
                        tn.ContextMenuStrip.Show();
                    }
                }
            }
		}

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseButtons mouseButton = e.Button;
            Point p = new Point(e.X, e.Y);
            foreach (TreeNode tn in toplevel.Nodes)
            {
                Rectangle bounds = tn.Bounds;
                bounds.Inflate(100, 0);
                if (bounds.Contains(p))
                {
                    treeView1.SelectedNode = tn;
                    ((ExtTreeNode)tn).LastMouseButton = mouseButton;
                    break;
                }
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                selectDBforCompare();
            }
            else if (treeView1.SelectedNode == toplevel)
            {
                if (_servers == null && toplevel.Nodes.Count == 0)
                {
                    RefreshTreeView(true);
                }
                treeView1.TopNode.Expand();
            }
            else if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                selectDBTable();
            }
        }

        private void DBMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                if (treeView1.SelectedNode.Nodes.Count > 0)
                {
                    DBMenuStrip.Items["getDataTablesToolStripMenuItem"].Visible = false;
                    DBMenuStrip.Items["hideListOfDataTablesToolStripMenuItem"].Visible = true;
                }
                else
                {
                    DBMenuStrip.Items["getDataTablesToolStripMenuItem"].Visible = true;
                    DBMenuStrip.Items["hideListOfDataTablesToolStripMenuItem"].Visible = false;
                }
                if (((DBTreeNode)treeView1.SelectedNode).SelectedForCompare)
                {
                    DBMenuStrip.Items["selectedDBToolStripMenuItem"].Visible = false;
                    DBMenuStrip.Items["deselectDBToolStripMenuItem"].Visible = true;
                }
                else
                {
                    DBMenuStrip.Items["selectedDBToolStripMenuItem"].Visible = true;
                    DBMenuStrip.Items["deselectDBToolStripMenuItem"].Visible = false;
                }
            }
            else if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                TableMenuStrip.Items["getDataAsXLSToolStripMenuItem"].Text = "Export Table as ...";
                TableMenuStrip.Items["compareSelectedMenuItem"].Visible = false;
                TableMenuStrip.Items["deselectAllMenuItem"].Visible = false;
                if (((TableTreeNode)treeView1.SelectedNode).SelectedForCompare)
                {
                    if (selectedTables.Tables.Length > 0)
                    {
                        TableMenuStrip.Items["getDataAsXLSToolStripMenuItem"].Text = "Export Selected Table(s) as ...";
                    }
                    TableMenuStrip.Items["selectTableMenuItem"].Visible = false;
                    TableMenuStrip.Items["deselectTableMenuItem"].Visible = true;
                }
                else
                {
                    TableMenuStrip.Items["selectTableMenuItem"].Visible = true;
                    TableMenuStrip.Items["deselectTableMenuItem"].Visible = false;
                }
                if (TableMenuStrip.Items["deselectTableMenuItem"].Visible)
                {
                    if (selectedTables.Tables.Length > 1)
                    {
                        TableMenuStrip.Items["compareSelectedMenuItem"].Visible = true;
                        TableMenuStrip.Items["deselectAllMenuItem"].Visible = true;
                    }
                }
            }
        }

        private void deselectDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deselectDB();
        }

        private void getDTSPackagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (typeof(ServerTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                ServerTreeNode stn = (ServerTreeNode)treeView1.SelectedNode;
                OpenDTSSerializer(stn);
            }
        }

        private void ServerMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (typeof(ServerTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                ServerTreeNode stn = (ServerTreeNode)treeView1.SelectedNode;
                if (stn.Connected)
                {
                    this.getDTSPackagesToolStripMenuItem.Enabled = true;
                }
                else
                {
                    this.getDTSPackagesToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void btnRefreshServers_Click(object sender, EventArgs e)
        {
            mi_Refresh_Click(sender, e);
        }

        private void btnAddServer_Click(object sender, EventArgs e)
        {
            addSQLServerToolStripMenuItem_Click(sender, e);
        }

        private void generateSQLScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                DBTreeNode dbTn = (DBTreeNode)treeView1.SelectedNode;
                StartAsyncSchemaGeneration(dbTn, true);
            }
        }

        private void asyncSerializeDB_CompleteSerializeDB(string FileName)
        {
            if (Cursor.Current != Cursors.Default)
            {
                Cursor.Current = Cursors.Default;
            }
            LoadXMLDoc(FileName);
            if (FileName.Split(',').Length > 1)
            {
                LoadSQLDoc(FileName);
            }
            DBTreeNode node = null;
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                node = (DBTreeNode)treeView1.SelectedNode;
            }
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                node = (DBTreeNode)treeView1.SelectedNode.Parent;
            }
            if (startCompare)
            {
                node = selectedDBs[0];
            }
            OnSchemaGenerated(node);
        }

        private void asyncSourceSerializeDB_CompleteSerializeDB(string FileName)
        {
            if (Cursor.Current != Cursors.Default)
            {
                Cursor.Current = Cursors.Default;
            }
            LoadXMLDoc(FileName);
            xmlSourceFileName = XMLFileName(FileName);

            if (FileName.Split(',').Length > 1)
            {
                LoadSQLDoc(FileName);
            }
            DBTreeNode node = null;
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                node = (DBTreeNode)treeView1.SelectedNode;
            }
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                node = (DBTreeNode)treeView1.SelectedNode.Parent;
            }
            if (startCompare)
            {
                node = selectedDBs[0];
            }
            OnSchemaGenerated(node);
        }

        private void asyncDestSerializeDB_CompleteSerializeDB(string FileName)
        {
            if (Cursor.Current != Cursors.Default)
            {
                Cursor.Current = Cursors.Default;
            }
            LoadXMLDoc(FileName);
            xmlDestFileName = XMLFileName(FileName);

            if (FileName.Split(',').Length > 1)
            {
                LoadSQLDoc(FileName);
            }
            OnSchemaGenerated(selectedDBs[1]);
        }

        private void compareDB_CompleteCompareDB(string FileName)
        {
            if (Cursor.Current != Cursors.Default)
            {
                Cursor.Current = Cursors.Default;
            }
            LoadXMLDoc(FileName);
            if (FileName.Split(',').Length > 1)
            {
                LoadSQLDoc(FileName);
            }
            // update tree to deselect DB nodes
            foreach(TreeNode tn in toplevel.Nodes)
            {
                if (typeof(ServerTreeNode).IsInstanceOfType(tn))
                {
                    foreach (TreeNode tn2 in tn.Nodes)
                    {
                        if (typeof(DBTreeNode).IsInstanceOfType(tn2))
                        {
                            DBTreeNode dbNode = (DBTreeNode)tn2;
                            if (dbNode.SelectedForCompare)
                            {
                                dbNode.SelectedForCompare = false;
                                dbNode.ForeColor = System.Drawing.Color.Black;
                                dbNode.BackColor = System.Drawing.Color.White;
                                dbNode.ImageIndex = 20;//13;
                                dbNode.SelectedImageIndex = 20;//13;
                            }
                        }
                    }
                }
            }
            xmlDestFileName = null;
            xmlSourceFileName = null;
            selectedDBs[0] = null;
            selectedDBs[1] = null;
            OnSchemaCompared(FileName.Split(','));
        }

        private void getDataTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getDBTables(treeView1.SelectedNode);
        }

        private void selectTableMenuItem_Click(object sender, EventArgs e)
        {
            // persist selected dbtable
            selectDBTable();
        }

        private void deselectTableMenuItem_Click(object sender, EventArgs e)
        {
            // persist deselected dbtable
            deselectDBTable();
        }

        private void excelMenuItem_Click(object sender, EventArgs e)
        {
            // excel export
            ExportSelectedTable(Export.ExportFormat.Excel);
        }

        private void cvsMenuItem_Click(object sender, EventArgs e)
        {
            // cvs export
            ExportSelectedTable(Export.ExportFormat.CSV);
        }

        private void xMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // XML export
            ExportSelectedTable(Export.ExportFormat.XML);
        }

        private void refreshDatabasesMenuItem_Click(object sender, EventArgs e)
        {
            // TODO  add refresh for databases
            if (typeof(ServerTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                ServerTreeNode stn = (ServerTreeNode)treeView1.SelectedNode;
                populateChildNodes((ExtTreeNode)stn);
            }
        }

        private void compareSelectedMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedTables.Tables.Length > 0)
            {
                OnTableCompareStarted((TableTreeNode)selectedTables[0]);
            }
        }

        private void deselectAllMenuItem_Click(object sender, EventArgs e)
        {
            deselectDBTables();
        }

        private void generateTableSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateTableSchema();
        }

        #endregion

        #region private helper methods

        private void LoadSQLDoc(string fileName)
        {
            try
            {
                if (main == null)
                {
                    main = (DockPane)this.Parent;
                }
                SQLDoc sqlDoc = new SQLDoc();
                sqlDoc.InitialDirectory = LastDirectory;
                sqlDoc.RightToLeftLayout = RightToLeftLayout;
                sqlDoc.FileName = SQLFileName(fileName);
                sqlDoc.Show(main.DockPanel);
                LastDirectory = sqlDoc.InitialDirectory;
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }
        }

        private void LoadXMLDoc(string fileName)
        {
            try
            {
                if (main == null)
                {
                    main = (DockPane)this.Parent;
                }
                XMLDoc xmlDoc = new XMLDoc();
                xmlDoc.InitialDirectory = LastDirectory;
                xmlDoc.RightToLeftLayout = RightToLeftLayout;
                xmlDoc.FileName = XMLFileName(fileName);
                xmlDoc.Show(main.DockPanel);
                LastDirectory = xmlDoc.InitialDirectory;
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }
        }

        private void getDBTables(TreeNode dbtn)
        {
            // call getdatatables function to add tables nodes
            if (typeof(DBTreeNode).IsInstanceOfType(dbtn))
            {
                dbtn.Nodes.Clear();
                ArrayList al = SQLData.GetDBTableNames(dbtn.Text, ((ServerTreeNode)dbtn.Parent).SQLServerConnection);
                for (int ii = 0; ii < al.Count; ii++)
                {
                    
                    TableTreeNode ttn = new TableTreeNode(al[ii].ToString());
                    // TODO hookup images and context menu for table nodes
                    ttn.ImageIndex = 23;
                    ttn.SelectedImageIndex = 23;
                    ttn.ContextMenuStrip = TableMenuStrip;
                    dbtn.Nodes.Add(ttn);
                    if (selectedTables[0] != null && selectedTables[0].FullTablePath.Equals(ttn.FullTablePath))
                    {
                        ttn.Checked = true;
                        ttn.ImageIndex = 26;
                        ttn.SelectedImageIndex = 26;
                        ttn.Tag = 0;
                        ttn.ForeColor = System.Drawing.Color.OrangeRed;
                        ttn.BackColor = System.Drawing.Color.LightGray;
                        treeView1.SelectedNode = ttn;
                        selectedTables[0] = ttn;
                    }
                    else if (selectedTables[1] != null && selectedTables[1].FullTablePath.Equals(ttn.FullTablePath))
                    {
                        ttn.Checked = true;
                        ttn.ImageIndex = 26;
                        ttn.SelectedImageIndex = 26;
                        ttn.Tag = 1;
                        ttn.ForeColor = System.Drawing.Color.OrangeRed;
                        ttn.BackColor = System.Drawing.Color.LightGray;
                        treeView1.SelectedNode = ttn;
                        selectedTables[1] = ttn;
                    }
                }
                if (dbtn.Nodes.Count > 0)
                {
                    dbtn.Expand();
                }
            }
        }

        // TODO: handle more than two tables for export purposes
        private void selectDBTable()
        {
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                TableTreeNode tableNode = (TableTreeNode)treeView1.SelectedNode;
                if (tableNode.Tag == null)
                {
                    tableNode.Checked = true;
                    tableNode.ImageIndex = 26;
                    tableNode.SelectedImageIndex = 26;
                    if (selectedTables[0] == null)
                    {
                        tableNode.Tag = 0;
                        selectedTables[0] = tableNode;
                        tableNode.ForeColor = System.Drawing.Color.OrangeRed;
                        tableNode.BackColor = System.Drawing.Color.LightGray;
                        OnTableSelected(tableNode);
                        return;
                    }
                    else if (selectedTables[1] == null)
                    {
                        if (!selectedTables[0].FullTablePath.Equals(tableNode.FullTablePath))
                        {
                            tableNode.Tag = 1;
                            selectedTables[1] = tableNode;
                            tableNode.ForeColor = System.Drawing.Color.BlueViolet;
                            tableNode.BackColor = System.Drawing.Color.LightGray;
                            tableNode.Checked = true;
                            OnTableSelected(tableNode);
                        }
                        return;
                    }
                    else if (selectedTables[selectedTables.Count] == null)
                    {
                        tableNode.Tag = selectedTables.Count;
                        selectedTables[selectedTables.Count] = tableNode;
                        return;
                    }
                }
                else
                {
                    deselectDBTable();
                }
            }
        }

        private void deselectDBTable()
        {
            if (typeof(TableTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                TableTreeNode tableNode = (TableTreeNode)treeView1.SelectedNode;
                if (tableNode.Tag != null && tableNode.Checked)
                {
                    tableNode.ForeColor = System.Drawing.Color.Black;
                    tableNode.BackColor = System.Drawing.Color.White;
                    tableNode.ImageIndex = 23;
                    tableNode.SelectedImageIndex = 23;
                    tableNode.Checked = false;

                    int Index = (int)tableNode.Tag;
                    tableNode.Tag = null;

                    if (selectedTables[0] != null && selectedTables[0].FullTablePath.Equals(tableNode.FullTablePath))
                    {
                        selectedTables.Remove(selectedTables[0]);
                        selectedTables.InsertAt(0, new TableTreeNode());
                        OnTableUnSelected((TableTreeNode)treeView1.SelectedNode);
                    }
                    if (selectedTables[1] != null && selectedTables[1].FullTablePath.Equals(tableNode.FullTablePath))
                    {
                        selectedTables.Remove(selectedTables[1]);
                        selectedTables.InsertAt(1, new TableTreeNode());
                        OnTableUnSelected((TableTreeNode)treeView1.SelectedNode);
                    }
                    else
                    {
                        selectedTables.Remove(selectedTables[Index]);
                    }
                }
            }
        }

        private string XMLFileName(string composite)
        {
            return composite.Split(',').Length > 1 ? composite.Split(',')[0] : composite;
        }

        private string SQLFileName(string composite)
        {
            return composite.Split(',').Length > 1 ? composite.Split(',')[1] : null;
        }

        private TreeNode CheckNodeForMouseUp(TreeNode tnParent, Point p, MouseButtons mouseButton)
        {
            foreach (TreeNode tn in tnParent.Nodes)
            {
                if (tn.Bounds.Contains(p))
                {
                    treeView1.SelectedNode = tn;
                    if (typeof(ExtTreeNode).IsInstanceOfType(tn))
                    {
                        ((ExtTreeNode)tn).LastMouseButton = mouseButton;
                    }
                    return tn;
                }
                TreeNode tn2 = CheckNodeForMouseUp(tn, p, mouseButton);
                if (tn2 != null)
                {
                    return tn2;
                }
            }
            return null;
        }

        private void deselectDB()
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                int Index = 0;
                // TODO: make sure to use the selectedDBs tree nodes instead of the current selected node
                // as this code could visually wind up out of sync with whats really selected
                DBTreeNode dbNode = (DBTreeNode)treeView1.SelectedNode;
                if (selectedDBs[0] != null && selectedDBs[0].FullDBPath.Equals(dbNode.FullDBPath))
                {
                    Index = 0;
                    selectedDBs[0] = null;
                }
                if (selectedDBs[1] != null && selectedDBs[1].FullDBPath.Equals(dbNode.FullDBPath))
                {
                    Index = 1;
                    selectedDBs[1] = null;
                }
                dbNode.SelectedForCompare = false;
                dbNode.ForeColor = System.Drawing.Color.Black;
                dbNode.BackColor = System.Drawing.Color.White;
                dbNode.ImageIndex = 20;//13;
                dbNode.SelectedImageIndex = 20;//13;
                dbNode.Checked = false;
                treeView1.SelectedNode.Tag = Index;
                OnDBUnSelected((DBTreeNode)treeView1.SelectedNode);
            }
        }

        private void selectDBforCompare()
        {
            if (typeof(DBTreeNode).IsInstanceOfType(treeView1.SelectedNode))
            {
                int Index = 0;
                DBTreeNode dbNode = (DBTreeNode)treeView1.SelectedNode;
                if (selectedDBs[0] != null && selectedDBs[1] != null && (!selectedDBs[0].FullDBPath.Equals(dbNode.FullDBPath) && !selectedDBs[1].FullDBPath.Equals(dbNode.FullDBPath)))
                {
                    MessageBox.Show("You can only select two DBs to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    dbNode.Checked = false;
                    return ;
                }
                dbNode.SelectedForCompare = true;
                dbNode.ImageIndex = 25;
                dbNode.SelectedImageIndex = 25;
                dbNode.Checked = true;
                if (selectedDBs[0] == null && xmlSourceFileName == null)
                {
                    dbNode.Tag = Index;
                    selectedDBs[0] = dbNode;
                    dbNode.ForeColor = System.Drawing.Color.OrangeRed;
                    dbNode.BackColor = System.Drawing.Color.LightGray;
                    OnDBSelected(dbNode);
                    return ;
                }
                else if (selectedDBs[1] == null && xmlDestFileName == null)
                {
                    if (selectedDBs[0] != null && !selectedDBs[0].FullDBPath.Equals(dbNode.FullDBPath))
                    {
                        Index = 1;
                        dbNode.Tag = Index;
                        selectedDBs[1] = dbNode;
                        dbNode.ForeColor = System.Drawing.Color.BlueViolet;
                        dbNode.BackColor = System.Drawing.Color.LightGray;
                        dbNode.Checked = true;
                        OnDBSelected(dbNode);
                        return ;
                    }
                }
                if (selectedDBs[0] != null && selectedDBs[0].FullDBPath.Equals(dbNode.FullDBPath)) 
                {
                    deselectDB();
                }
                else if (selectedDBs[1] != null && selectedDBs[1].FullDBPath.Equals(dbNode.FullDBPath))
                {
                    deselectDB();
                }
            }
        }

        /// <summary>
        /// recursive procedure that populates child tree nodes or displays dialog that then calls this procedure
        /// </summary>
        /// <param name="tn"></param>
        private void populateChildNodes(ExtTreeNode tn)
        {
            if (typeof(ServerTreeNode).IsInstanceOfType(tn))
            {
                if (((ServerTreeNode)tn).Connected && (tn.FirstNode == null || tn.FirstNode.Text.Equals("...")))
                {
                    tn.Nodes.Clear();
                    tn.Nodes.AddRange(getDBNames((ServerTreeNode)tn));
                    tn.Expand();
                    treeView1.SelectedNode = tn.Nodes[0];
                }
                else
                {
                    if (tn.FirstNode == null || tn.FirstNode.Text.Equals("..."))
                    {
                        ((ExtTreeNode)tn).LastMouseButton = MouseButtons.Right;
                        mi_Security_Click(this, null);
                    }
                }
            }
            else if (typeof(DBTreeNode).IsInstanceOfType(tn))
            {
                getDBTables(tn);
            }
        }

        /// <summary>
        /// adds DB names to treeview
        /// </summary>
        /// <param name="stn">ServerTreeNode</param>
        /// <returns>DBTreeNode[]</returns>
        private DBTreeNode[] getDBNames(ServerTreeNode stn)
        {
            System.Collections.ArrayList arl = new System.Collections.ArrayList();
            if (stn.Connected)
            {
                string[] dbNames = SQLServers.GetDBNames(stn.SQLServerConnection);
                foreach (string dbName in dbNames)
                {
                    DBTreeNode dbtn = new DBTreeNode(dbName);
                    dbtn.Server = stn.Text;
                    dbtn.ContextMenuStrip = this.DBMenuStrip;
                    dbtn.ImageIndex = 20;
                    //dbtn.Nodes.Add("...");
                    arl.Add(dbtn);
                }
            }
            return (DBTreeNode[])arl.ToArray(typeof(DBTreeNode));
        }

        private bool OpenSQLConnection(ServerTreeNode stn, bool openConnection)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (sqlConnections == null || (sqlConnections[stn.Text] == null && stn.SQLServerConnection == null))
                {
                    if (sqlConnections == null || sqlConnections.Count == 0)
                    {
                        if (stn.UID == null && stn.Pwd == null)
                        {
                            sqlConnections = new SQLConnections(stn.Text, "Master");
                        }
                        else
                        {
                            sqlConnections = new SQLConnections(stn.Text, "Master", stn.UID, stn.Pwd, stn.SavePWD);
                        }
                    }
                    else
                    {
                        if (stn.UID == null && stn.Pwd == null)
                        {
                            sqlConnections.Add(stn.Text, "Master");
                        }
                        else
                        {
                            sqlConnections.Add(stn.Text, "Master", stn.UID, stn.Pwd, stn.SavePWD);
                        }
                    }
                    stn.SQLServerConnection = sqlConnections[stn.Text].sqlConnection;
                    if (openConnection) 
                    {
                        stn.SQLServerConnection.Open(); 
                    }
                }
                else
                {
                    // we'd better check the passed in connection settings again
                    stn.SQLServerConnection = null;
                    if (sqlConnections[stn.Text] != null)
                    {
                        sqlConnections.Remove(stn.Text);
                    }
                    if (stn.UID == null && stn.Pwd == null)
                    {
                        sqlConnections.Add(stn.Text, "Master");
                    }
                    else
                    {
                        sqlConnections.Add(stn.Text, "Master", stn.UID, stn.Pwd, stn.SavePWD);
                    }
                    stn.SQLServerConnection = sqlConnections[stn.Text].sqlConnection;
                    if (openConnection)
                    {
                        stn.SQLServerConnection.Open();
                    }
                }
            }
            catch 
            {
                // TODO: change SQL server icon to indicate not connected
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            if (stn.SQLServerConnection.State == ConnectionState.Open)
            {
                stn.Connected = true;
            }
            return true;
        }

        #endregion

        #region exposed custom properties

        public int SelectedTablesCount
        {
            get { return selectedTables.Count; }
        }

        public string CustomSchemaXSLT
        {
            get { return _customSchemaXSLT; }
            set { _customSchemaXSLT = value; }
        }

        public string CustomDataXSLT
        {
            get { return _customDataXSLT; }
            set { _customDataXSLT = value; }
        }

        public string LastDirectory
        {
            get 
            {
                if (string.IsNullOrEmpty(_lastDirectory))
                {
                    _lastDirectory = Application.LocalUserAppDataPath;
                }
                return _lastDirectory;
            }
            set { _lastDirectory = value; }
        }

        public byte CompareOptions
        {
            set { _compareOptions = value; }
        }

        public TreeNode SelectedTreeNode
        {
            get { return treeView1.SelectedNode; }
        }

        public SQLConnections SQLConnections
        {
            get { return sqlConnections; }
            set
            {
                sqlConnections = value; 
                treeView1.Sort();
            }
        }

        public string[] Servers
        {
            get 
            {
                if (_servers == null)
                {
                    getServers();
                }
                return _servers;
            }
        }

        #endregion

    }

    public class SchemaGeneratedEventArgs : EventArgs
    {
        public string[] Filenames;
    }
}