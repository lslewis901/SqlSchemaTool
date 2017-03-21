using Lewis.OptionsDialog;
using Lewis.SST;
using Lewis.SST.AsyncMethods;
using Lewis.SST.Controls;
using Lewis.SST.Help;
using Lewis.SST.SQLMethods;
using Lewis.Xml;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

#region change history
/// 08-22-2008: C01: LLEWIS: added check to close all SQL connections, at close of main
#endregion

namespace Lewis.SST.Gui
{
    public partial class Main : Form
    {
        #region class var declarations

        private static Logger logger = LogManager.GetLogger("Lewis.SST.Gui");
        private SQLServerExplorer m_serverExplorer = new SQLServerExplorer();
        private XmlNodeExplorer m_xmlNodeExplorer = new XmlNodeExplorer();
        private DeserializeDockContent m_deserializeDockContent = null;
        private ServerTreeNode m_currentServerTreeNode = null;
        private DBTreeNode m_currentSelectedDBTreeNode = null;
        private DBTreeNode m_currentCompareDBTreeNode1 = null;
        private DBTreeNode m_currentCompareDBTreeNode2 = null;
        private TableTreeNode m_currentSelectedTableTreeNode = null;
        private string m_xmlSnapShotFile1 = null;
        private string m_xmlSnapShotFile2 = null;
        private string lastFindValue = string.Empty;
        private string optionsSettings = string.Empty;
        private string optionsFileName = "options.config";
        private string configFileName = "dockPanel.config";
        private string windowsSettingsFile = "windows.config";
        private optionsFormProperties ofp;
        private string lastFileType = string.Empty;
        private string sourceTableName = string.Empty;
        private string targetTableName = string.Empty;
        private TableTreeNode[] selectedTables = new TableTreeNode[2];
        private string _lastDirectory;
        private string _customDataXSLTFile;
        private string _customSchemaXSLTFile;
        private bool _runningCompare = false;
        private int _runningDualSchemas = 0;

        #endregion

        #region public methods

        public Main()
        {
            InitializeComponent();

            MainMenuStrip.MdiWindowListItem = windowToolStripMenuItem;
            m_xmlNodeExplorer.RightToLeftLayout = RightToLeftLayout;
            m_serverExplorer.RightToLeftLayout = RightToLeftLayout;
            m_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
            m_serverExplorer.SQLServerSelected += new EventHandler<ServerTreeEventArgs>(m_serverExplorer_SQLServerSelected);
            m_serverExplorer.DBSelected += new EventHandler<DBTreeEventArgs>(m_serverExplorer_DBSelected);
            m_serverExplorer.DBUnSelected += new EventHandler<DBTreeEventArgs>(m_serverExplorer_DBUnSelected);
            m_serverExplorer.SchemaGenerationStarted += new EventHandler<DBTreeEventArgs>(m_serverExplorer_SchemaGenerationStarted);
            m_serverExplorer.SchemaCompareStarted += new EventHandler<SchemaGeneratedEventArgs>(m_serverExplorer_SchemaCompareStarted);
            m_serverExplorer.SchemaGenerated += new EventHandler<DBTreeEventArgs>(m_serverExplorer_SchemaGenerated);
            m_serverExplorer.SchemaCompared += new EventHandler<SchemaGeneratedEventArgs>(m_serverExplorer_SchemaCompared);
            m_serverExplorer.TableSelected += new EventHandler<TableTreeEventArgs>(m_serverExplorer_TableSelected);
            m_serverExplorer.TableUnSelected += new EventHandler<TableTreeEventArgs>(m_serverExplorer_TableUnSelected);
            m_serverExplorer.TableCompareStarted += new EventHandler<TableTreeEventArgs>(m_serverExplorer_TableCompareStarted);
            dockPanel.ContentAdded += new EventHandler<DockContentEventArgs>(dockPanel_ContentAdded);
            dockPanel.ContentRemoved += new EventHandler<DockContentEventArgs>(dockPanel_ContentRemoved);
            dockPanel.ActiveDocumentChanged += new EventHandler(dockPanel_ActiveDocumentChanged);
            dockPanel.ActiveContentChanged += new EventHandler(dockPanel_ActiveContentChanged);
            windowToolStripMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(windowToolStripMenuItem_DropDownItemClicked);

            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            optionsFileName = Path.Combine(appPath, "options.config");
            configFileName = Path.Combine(appPath, "dockPanel.config");
            windowsSettingsFile = Path.Combine(appPath, "windows.config");

            btnHTML.Enabled = true;

            this.progressIndicator.Visible = false;
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
            set
            {
                _lastDirectory = value;
                m_serverExplorer.LastDirectory = _lastDirectory;
            }
        }

        #endregion

        #region event handler methods

        private void iconsAndTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainToolbar.SuspendLayout();
            ActionToolbar.SuspendLayout();
            foreach (ToolStripItem button in mainToolbar.Items)
            {
                buttonLayout(button);
            }
            foreach (ToolStripItem button in ActionToolbar.Items)
            {
                buttonLayout(button);
            }
            mainToolbar.ResumeLayout();
            ActionToolbar.ResumeLayout();
        }

        private void m_serverExplorer_TableCompareStarted(object sender, TableTreeEventArgs e)
        {
            this.selectedTablesToolStripMenuItem_Click(sender, e);
        }

        private void m_serverExplorer_TableUnSelected(object sender, TableTreeEventArgs e)
        {
            // update display of selected table names
            m_currentSelectedTableTreeNode = null;
            if (sourceTableName.Equals(e.TableTreeNode.FullTablePath))
            {
                sourceTableName = string.Empty;
                selectedTables[0] = null;
                selectedTablesMenuItem.Enabled = false;
                this.toolStripStatusLabel1.Text = string.Empty;
            }
            if (targetTableName.Equals(e.TableTreeNode.FullTablePath))
            {
                targetTableName = string.Empty;
                selectedTables[1] = null;
                selectedTablesMenuItem.Enabled = false;
                this.toolStripStatusLabel2.Text = string.Empty;
            }
            if (sourceTableName == string.Empty && targetTableName == string.Empty)
            {
                m_serverExplorer.ClearSelected(SQLServerExplorer.SelectedTypes.Tables);
            }
        }

        private void m_serverExplorer_TableSelected(object sender, TableTreeEventArgs e)
        {
            // display selected tables for compare
            m_currentSelectedTableTreeNode = e.TableTreeNode;
            if (sourceTableName == string.Empty)
            {
                sourceTableName = e.TableTreeNode.FullTablePath;
                selectedTables[0] = e.TableTreeNode;
                this.toolStripStatusLabel1.Text = "Source Table: " + sourceTableName + "...";
            }
            else if (targetTableName == string.Empty)
            {
                targetTableName = e.TableTreeNode.FullTablePath;
                selectedTables[1] = e.TableTreeNode;
                this.toolStripStatusLabel2.Text = "Target Table: " + targetTableName + "...";
            }
            if (sourceTableName != string.Empty && targetTableName != string.Empty)
            {
                selectedTablesMenuItem.Enabled = true;
            }
        }

        private void windowToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text.ToLower() == "&options") return;

            foreach (Document doc in dockPanel.Documents)
            {
                if (doc.TabText.Equals(e.ClickedItem.Text))
                {
                    doc.Show(dockPanel);
                    break;
                }
            }
        }

        private void m_serverExplorer_SchemaCompared(object sender, SchemaGeneratedEventArgs e)
        {
            string work = "Finished Processing Schema.";
            _runningCompare = false;
            m_currentCompareDBTreeNode1 = null;
            m_currentCompareDBTreeNode2 = null;
            m_xmlSnapShotFile1 = null;
            m_xmlSnapShotFile2 = null;
            foreach (Document doc in dockPanel.Documents)
            {
                if (typeof(XMLDoc).IsInstanceOfType(doc))
                {
                    ((XMLDoc)doc).IsSelectedCompare = false;
                }
            }
            this.toolStripStatusLabel2.Text = string.Empty;
            this.toolStripStatusLabel1.Text = work;
            disableProgressIndicator();
        }

        private void m_serverExplorer_SchemaGenerated(object sender, DBTreeEventArgs e)
        {
            if ((e.DBTreeNode == null) || (e.DBTreeNode.Tag == null))
            {
                string work = "Finished Processing Schema: " + e.DBTreeNode.FullDBPath;
                this.toolStripStatusLabel2.Text = string.Empty;
                this.toolStripStatusLabel1.Text = work;
                _runningDualSchemas--;
            }
            else if ( (int)e.DBTreeNode.Tag == 0 )
            {
                string work = "Finished Processing Schema: " + e.DBTreeNode.FullDBPath;
                this.toolStripStatusLabel1.Text = work;
                _runningDualSchemas--;
            }
            else if ((int)e.DBTreeNode.Tag == 1)
            {
                string work = "Finished Processing Schema: " + e.DBTreeNode.FullDBPath;
                this.toolStripStatusLabel2.Text = work;
                _runningDualSchemas--;
            }
            disableProgressIndicator();
        }

        private void m_serverExplorer_SchemaCompareStarted(object sender, SchemaGeneratedEventArgs e)
        {
            string source = m_currentCompareDBTreeNode1 == null ? m_xmlSnapShotFile1 == null ? "unknown" : m_xmlSnapShotFile1 : m_currentCompareDBTreeNode1.Text;
            string dest = m_currentCompareDBTreeNode2 == null ? m_xmlSnapShotFile2 == null ? "unknown" : m_xmlSnapShotFile2 : m_currentCompareDBTreeNode2.Text;
            string work = string.Format("Processing DB Compare: {0} - to - {1}", source, dest);
            this.toolStripStatusLabel2.Text = "";
            this.toolStripStatusLabel1.Text = work;
            _runningCompare = true;
            this.timer1.Enabled = true;
            this.progressIndicator.Visible = true;
        }

        private void m_serverExplorer_SchemaGenerationStarted(object sender, DBTreeEventArgs e)
        {
            string work = "Processing [" + e.DBTreeNode.FullDBPath + "]...";
            if (e.DBTreeNode.Tag == null)
            {
                this.toolStripStatusLabel2.Text = string.Empty;
                this.toolStripStatusLabel1.Text = work;
            }
            else if ((int)e.DBTreeNode.Tag == 0)
            {
                this.toolStripStatusLabel1.Text = work;
            }
            else if ((int)e.DBTreeNode.Tag == 1)
            {
                this.toolStripStatusLabel2.Text = work;
            }
            _runningDualSchemas++;
            this.timer1.Enabled = true;
            this.progressIndicator.Visible = true;
        }

        private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            btnSyncXml.Enabled = false;
            //btnHTML.Enabled = false;
            btnSelect.Enabled = false;
            btnRefreshServers.Enabled = false;
            btnGenDatabase.Enabled = false;
            btnGenXml.Enabled = false;

            if (dockPanel.ActiveContent != null && typeof(SQLServerExplorer).IsInstanceOfType(dockPanel.ActiveContent))
            {
                // deactivate select for button
                // deactivate sync button
                btnRefreshServers.Enabled = true;
                if (typeof(DBTreeNode).IsInstanceOfType(m_serverExplorer.SelectedTreeNode))
                {
                    btnGenXml.Enabled = true;
                }
            }
            else if (dockPanel.ActiveContent != null && typeof(XmlNodeExplorer).IsInstanceOfType(dockPanel.ActiveContent))
            {
                // deactivate select for button
                // deactivate sync button
            }
            else if (dockPanel.ActiveContent != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveContent))
            {
                if (dockPanel.ActiveDocument != null)
                {
                    Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                    if (typeof(XMLDoc).IsInstanceOfType(doc))
                    {
                        if (((XMLDoc)doc).IsDatabaseSchema)
                        {
                            // activate select for button
                            btnSelect.Enabled = true;
                        }
                        if (((XMLDoc)doc).IsDTSPackage)
                        {
                            // activate create DTS on server button
                            btnCreateDTSPackage.Enabled = true;
                        }
                        // activate sync button
                        btnSyncXml.Enabled = true;
                    }
                    else if (typeof(SQLDoc).IsInstanceOfType(doc))
                    {
                        // activate generate DB button
                        ((SQLDoc)doc).ServerExplorer = m_serverExplorer;
                        btnGenDatabase.Enabled = true;
                    }
                    else if (typeof(DTSDoc).IsInstanceOfType(doc))
                    {
                        btnGenXml.Enabled = true;
                    }
                    doc.Activate();
                }
            }
        }

        private void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            // new doc shown in dock panel - needed to hook up event handler for that document to track its changed state
            btnSyncXml.Enabled = false;
            //btnHTML.Enabled = false;
            btnSelect.Enabled = false;
            btnRefreshServers.Enabled = false;
            btnGenDatabase.Enabled = false;
            btnGenXml.Enabled = false;

            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument))
            {
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.DocumentChanged -= new EventHandler<EventArgs>(doc_DocumentChanged);
                doc.DocumentChanged += new EventHandler<EventArgs>(doc_DocumentChanged);
                if (typeof(XMLDoc).IsInstanceOfType(doc))
                {
                    if (((XMLDoc)doc).IsDatabaseSchema)
                    {
                        // activate select for button
                        btnSelect.Enabled = true;
                        //btnHTML.Enabled = true;
                    }
                    if (((XMLDoc)doc).IsDTSPackage)
                    {
                        // activate create DTS on server button
                        btnCreateDTSPackage.Enabled = true;
                    }
                    // activate sync button
                    btnSyncXml.Enabled = true;
                }
                else if (typeof(SQLDoc).IsInstanceOfType(doc))
                {
                    // activate generate DB button
                    btnGenDatabase.Enabled = true;
                }
                else if (typeof(DTSDoc).IsInstanceOfType(doc))
                {
                    btnGenXml.Enabled = true;
                }
                doc.Activate();
            }
        }

        private void doc_DocumentChanged(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument))
            {
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                if (doc.TextChanged)
                {
                    System.Windows.Forms.ToolStripItem windowToolStripMenuItemDropDownItem = windowToolStripMenuItem.DropDownItems[doc.TabText.Replace("*", "")];
                    if (windowToolStripMenuItemDropDownItem != null)
                    {
                        windowToolStripMenuItemDropDownItem.Text = doc.TabText;
                    }
                    if (doc.TabText.Replace("*", "").Length > 0)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(doc.TabText.Replace("*", ""));
                            if (fi != null)
                            {
                                lastFileType = fi.Extension;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    // TODO: Edit menu items disabled
                }
            }
        }

        private void dockPanel_ContentRemoved(object sender, DockContentEventArgs e)
        {
            // mdi window menu item remove dropdown items
            if (typeof(Document).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                windowToolStripMenuItem.DropDownItems.RemoveByKey(e.Content.DockHandler.TabText.Replace("*", ""));
                if (windowToolStripMenuItem.DropDownItems.Count == 2)
                {
                    windowToolStripMenuItem.DropDownItems.RemoveAt(1);
                }
            }
            if (typeof(XMLDoc).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)e.Content.DockHandler.Form;
                if (m_xmlNodeExplorer.Filename != null && doc != null && doc.FileName != null && doc.FileName.ToLower().Equals(m_xmlNodeExplorer.Filename.ToLower()) && !this.Disposing)
                {
                    m_xmlNodeExplorer.Clear();
                }
            }
        }

        private void dockPanel_ContentAdded(object sender, DockContentEventArgs e)
        {
            // mdi window menu item add dropdown items
            if (typeof(Document).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                if (windowToolStripMenuItem.DropDownItems.Count < 2)
                {
                    windowToolStripMenuItem.DropDownItems.Add("-");
                }
                windowToolStripMenuItem.DropDownItems.Add(e.Content.DockHandler.TabText);
                windowToolStripMenuItem.DropDownItems[windowToolStripMenuItem.DropDownItems.Count - 1].Name = e.Content.DockHandler.TabText;
            }
            if (typeof(DTSDoc).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                generateXMLOutputToolStripMenuItem.Enabled = true;
                dTSPackageSnapshotToolStripMenuItem.Enabled = true;
                btnGenXml.Enabled = true;
            }
            if (typeof(XMLDoc).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                Cursor.Current = Cursors.WaitCursor;
                XMLDoc doc = (XMLDoc)e.Content.DockHandler.Form;
                doc.SelectedForCompare -= new EventHandler<XMLDocEventArgs>(doc_SelectedForCompare); 
                doc.SelectedForCompare += new EventHandler<XMLDocEventArgs>(doc_SelectedForCompare);
                doc.UnSelectedForCompare -= new EventHandler<XMLDocEventArgs>(doc_UnSelectedForCompare);
                doc.UnSelectedForCompare += new EventHandler<XMLDocEventArgs>(doc_UnSelectedForCompare);
                doc.SyncWithTree -= new EventHandler<XMLDocEventArgs>(doc_SyncWithTree);
                doc.SyncWithTree += new EventHandler<XMLDocEventArgs>(doc_SyncWithTree);
                doc.ReCreateDTS -= new EventHandler<XMLDocEventArgs>(doc_ReCreateDTS);
                doc.ReCreateDTS += new EventHandler<XMLDocEventArgs>(doc_ReCreateDTS);
                doc.FormClosing -= new FormClosingEventHandler(doc_FormClosing);
                doc.FormClosing += new FormClosingEventHandler(doc_FormClosing);
                // this is what syncs the xml node explorer currently
                // only done on the initial load of said doc.
                // so xml node explorer only displays the last xml file loaded
                m_xmlNodeExplorer.OpenFile(doc.FileName);
                LastDirectory = doc.InitialDirectory;
                Cursor.Current = Cursors.Default;
            }
            if (typeof(SQLDoc).IsInstanceOfType(e.Content.DockHandler.Form))
            {
                SQLDoc doc = (SQLDoc)e.Content.DockHandler.Form;
                LastDirectory = doc.InitialDirectory;
                // TODO: enable buttons/menus if its a SQL doc
            }
            // added this to activate the event handler for activepanel and activedocument changed
            e.Content.DockHandler.Form.Focus();
        }

        void doc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                if (m_xmlNodeExplorer.Filename != null && doc != null && doc.FileName != null && doc.FileName.ToLower().Equals(m_xmlNodeExplorer.Filename.ToLower()) && !this.Disposing)
                {
                    m_xmlNodeExplorer.Clear();
                }
            }
        }

        private void doc_ReCreateDTS(object sender, XMLDocEventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                if (doc.IsDTSPackage)
                {
                    doc.ReHydrateDTS(m_serverExplorer.Servers);
                }
            }
        }

        private void doc_SyncWithTree(object sender, XMLDocEventArgs e)
        {
            if (e.selectedFileAndIndex.Length > 1)
            {
                FileInfo fi = new FileInfo(e.selectedFileAndIndex[0]);
                m_xmlNodeExplorer.OpenFile(fi.FullName);
            }
        }

        private void doc_UnSelectedForCompare(object sender, XMLDocEventArgs e)
        {
            // TODO: hookup status text and remove persisted xml file variable 
            if (e.selectedFileAndIndex.Length > 1)
            {
                if (e.selectedFileAndIndex[1] == "0" && m_currentCompareDBTreeNode1 == null)
                {
                    m_xmlSnapShotFile1 = null;
                    this.toolStripStatusLabel1.Text = string.Empty;
                }
                else if (e.selectedFileAndIndex[1] == "1" && m_currentCompareDBTreeNode2 == null)
                {
                    m_xmlSnapShotFile2 = null;
                    this.toolStripStatusLabel2.Text = string.Empty;
                }
                else if (e.selectedFileAndIndex[1] == "0" && m_currentCompareDBTreeNode1 != null) //hmmm...
                {
                    m_xmlSnapShotFile2 = null;
                    this.toolStripStatusLabel2.Text = string.Empty;
                }
            }
            if (m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null)
            {
                m_serverExplorer.ClearSelected(SQLServerExplorer.SelectedTypes.XML);
            }
        }

        private void doc_SelectedForCompare(object sender, XMLDocEventArgs e)
        {
            // hookup status text and persist selected xml file
            if (e.selectedFileAndIndex.Length > 1)
            {
                FileInfo fi = new FileInfo(e.selectedFileAndIndex[0]);
                if (e.selectedFileAndIndex[1] == "0" && m_currentCompareDBTreeNode1 == null)
                {
                    m_xmlSnapShotFile1 = e.selectedFileAndIndex[0];
                    this.toolStripStatusLabel1.Text = "Source XML Snapshot: " + fi.Name + "...";
                }
                else if (e.selectedFileAndIndex[1] == "1" && m_currentCompareDBTreeNode2 == null)
                {
                    m_xmlSnapShotFile2 = e.selectedFileAndIndex[0];
                    this.toolStripStatusLabel2.Text = "Target XML Snapshot: " + fi.Name + "...";
                }
                else if (e.selectedFileAndIndex[1] == "0" && m_currentCompareDBTreeNode1 != null) //hmmm...
                {
                    m_xmlSnapShotFile2 = e.selectedFileAndIndex[0];
                    this.toolStripStatusLabel2.Text = "Target XML Snapshot: " + fi.Name + "...";
                }
            }
        }

        private void m_serverExplorer_DBUnSelected(object sender, DBTreeEventArgs e)
        {
            int Index = e.DBTreeNode.Tag == null ? -1 : (int)e.DBTreeNode.Tag;
            if (Index == 0 && m_xmlSnapShotFile1 == null)
            {
                selectedDatabasesToolStripMenuItem.Enabled = false;
                m_currentCompareDBTreeNode1 = null;
                m_currentSelectedDBTreeNode = null;
                this.toolStripStatusLabel1.Text = "";
            }
            else if (Index == 1 && m_xmlSnapShotFile2 == null)
            {
                selectedDatabasesToolStripMenuItem.Enabled = false;
                m_currentCompareDBTreeNode2 = null;
                m_currentSelectedDBTreeNode = null;
                this.toolStripStatusLabel2.Text = "";
            }
            else if (Index == 0 && m_xmlSnapShotFile1 != null) // hmmm...
            {
                selectedDatabasesToolStripMenuItem.Enabled = false;
                m_currentCompareDBTreeNode2 = null;
                m_currentSelectedDBTreeNode = null;
                this.toolStripStatusLabel2.Text = "";
            }
            if (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null)
            {
                databaseSnapshotToolStripMenuItem.Enabled = false;
                selectedDatabaseToolStripMenuItem.Enabled = false;
                xMLSnapshotAndDatabaseToolStripMenuItem.Enabled = false;
                m_serverExplorer.ClearSelected(SQLServerExplorer.SelectedTypes.DBs);
            }
        }

        private void m_serverExplorer_DBSelected(object sender, DBTreeEventArgs e)
        {
            generateXMLOutputToolStripMenuItem.Enabled = true;
            databaseSnapshotToolStripMenuItem.Enabled = true;
            selectedDatabaseToolStripMenuItem.Enabled = true;
            selectedDatabasesToolStripMenuItem.Enabled = true;
            xMLSnapshotAndDatabaseToolStripMenuItem.Enabled = true;
            btnGenXml.Enabled = true;

            int Index = e.DBTreeNode.Tag == null ? -1 : (int)e.DBTreeNode.Tag;
            if (Index == 0 && e.DBTreeNode.SelectedForCompare && m_xmlSnapShotFile1 == null)
            {
                m_currentCompareDBTreeNode1 = e.DBTreeNode;
                this.toolStripStatusLabel1.Text = "Source DB: " + e.DBTreeNode.Server + "\\" + e.DBTreeNode.Text;
            }
            else if (Index == 1 && e.DBTreeNode.SelectedForCompare && m_xmlSnapShotFile2 == null)
            {
                m_currentCompareDBTreeNode2 = e.DBTreeNode;
                this.toolStripStatusLabel2.Text = "Target DB: " + e.DBTreeNode.Server + "\\" + e.DBTreeNode.Text;
            }
            else if (Index == 0 && e.DBTreeNode.SelectedForCompare && m_xmlSnapShotFile1 != null) // hmmm...
            {
                m_currentCompareDBTreeNode2 = e.DBTreeNode;
                this.toolStripStatusLabel2.Text = "Target DB: " + e.DBTreeNode.Server + "\\" + e.DBTreeNode.Text;
            }
            m_currentSelectedDBTreeNode = e.DBTreeNode;
        }

        private void m_serverExplorer_SQLServerSelected(object sender, ServerTreeEventArgs e)
        {
            setSecurityToolStripMenuItem.Enabled = true;
            btnSetSecurity.Enabled = true;
            m_currentServerTreeNode = e.ServerTreeNode;
            if (m_currentServerTreeNode.Connected)
            {
                getDTSPackageToolStripMenuItem.Enabled = true;
                btnGetDTSPackages.Enabled = true;
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            bool reOpenLast = false;
            object obj = OptionValues.GetValue("ReOpenLast", optionsSettings);
            if (obj != null)
            {
                CheckState checkSave = (CheckState)obj;
                if (checkSave == CheckState.Checked)
                {
                    reOpenLast = true;
                }
            }
            if (persistString == typeof(SQLServerExplorer).ToString())
                return m_serverExplorer;
            else if (persistString == typeof(XmlNodeExplorer).ToString())
                return m_xmlNodeExplorer;
            else 
            {
                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length < 3)
                    return null;

                if (parsedStrings[0] == typeof(XMLDoc).ToString() && reOpenLast)
                {
                    XMLDoc xmlDoc = new XMLDoc();
                    if (parsedStrings[1] != string.Empty)
                    {
                        xmlDoc.FileName = parsedStrings[1];
                        LastDirectory = xmlDoc.InitialDirectory;
                    }
                    if (parsedStrings[2] != string.Empty)
                        xmlDoc.Text = parsedStrings[2];

                    return xmlDoc;
                }
                else if (parsedStrings[0] == typeof(SQLDoc).ToString() && reOpenLast)
                {
                    SQLDoc sqlDoc = new SQLDoc();
                    if (parsedStrings[1] != string.Empty)
                    {
                        sqlDoc.FileName = parsedStrings[1];
                        LastDirectory = sqlDoc.InitialDirectory;
                    }
                    if (parsedStrings[2] != string.Empty)
                        sqlDoc.Text = parsedStrings[2];

                    return sqlDoc;
                }
                else
                {
                    return null;
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //launch the splash screen on a seperate thread
            //The first value passed in disables the timer that automatically closes the form.
            //The second value passed is the fade constant, which determines the change in form opacity for every interval of
            //the fadeTimer
            //The third value is the interval for the fadeTimer in ms
            int splashTimeout = 0;
            double fadeConstant = 0.05;
            int fadeTimer = 35;
            string initialmsg = "SST Starting, loading settings...";

            string licenseString = ResourceReader.ReadFromResource("Lewis.SST.Resources.Legal.License.rtf");
            logger.Info(initialmsg);
            // load settings
            if (File.Exists(optionsFileName))
            {
                optionsSettings = File.ReadAllText(optionsFileName);
            }
            else
            {
                optionsSettings = ResourceReader.ReadFromResource("Lewis.SST.Forms.Options.optionsControls.xml");
            }

            object splash = OptionValues.GetValue("ShowSplash", optionsSettings);
            if (splash != null && (CheckState)splash == CheckState.Checked)
            {
                SplashForm.Showasyncsplash(splashTimeout, fadeConstant, fadeTimer);
                SplashForm.StatusText = initialmsg;
            }
            setOptions(optionsSettings);

            if (File.Exists(configFileName))
            {
                dockPanel.LoadFromXml(configFileName, m_deserializeDockContent);
            }

            m_xmlNodeExplorer.Show(dockPanel);
            m_serverExplorer.Show(dockPanel);
            m_serverExplorer.SQLConnections = new SQLConnections();

            SplashForm.StatusText = "Reading form settings...";
            Settings.CtrlSettings cs = Settings.XmlSettings.ReadXml(windowsSettingsFile);
            if (cs != null && cs.Type != null)
            {
                Form f = (Form)this;
                Settings.XmlSettings.SetSettings(ref f, cs);

                if (cs.ChildCtrlsToPersist.Length > 0)
                {
                    foreach (Settings.CtrlSettings cs1 in cs.ChildCtrlsToPersist)
                    {
                        if (typeof(optionsFormProperties).ToString().Equals(cs1.Type))
                        {
                            if (ofp == null)
                            {
                                ofp = new optionsFormProperties();
                            }
                            ofp.Location = cs1.Location;
                            ofp.Size = cs1.Size;
                            break;
                        }
                    }
                }
            }
            if (ofp == null)
            {
                ofp = new optionsFormProperties();
            }
            // do sanity check for main form
            if (this.Location.X < 0 && this.Location.Y < 0)
            {
                this.Location = new Point(0, 0);
            }
            // write out license text
            if (licenseString != null)
            {
                File.WriteAllText("License.txt", licenseString);
            }
            //start the splash screen closing
            System.Threading.Thread.Sleep(2500);
            SplashForm.closeAsyncSplash();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.toolStripStatusLabel2.Text = "";
            this.toolStripStatusLabel1.Text = "Exiting Application...";

            object DeleteSettings = OptionValues.GetValue("DeleteSettings", optionsSettings);
            if (DeleteSettings != null && (CheckState)DeleteSettings != CheckState.Checked)
            {
                // save options
                XmlDocument xDocSettings = new XmlDocument();
                xDocSettings.LoadXml(optionsSettings);
                xDocSettings.Save(optionsFileName);

                // displose of mdi window menu and dropdown items click event
                windowToolStripMenuItem.DropDownItemClicked -= new ToolStripItemClickedEventHandler(windowToolStripMenuItem_DropDownItemClicked);
                windowToolStripMenuItem.Dispose();

                if (m_xmlNodeExplorer.Filename != null && m_xmlNodeExplorer.Filename.Length > 0)
                {
                    m_xmlNodeExplorer.Clear();
                }

                // check settings for save sql settings
                object obj = OptionValues.GetValue("SaveWindowsSettings", optionsSettings);
                if (obj != null)
                {
                    CheckState checkSave = (CheckState)obj;
                    if (checkSave == CheckState.Checked)
                    {
                        dockPanel.SaveAsXml(configFileName);
                        // write out various settings to application settings file                
                        Settings.CtrlSettings cs = Settings.XmlSettings.GetSettings(this);
                        ArrayList arl = new ArrayList(cs.ChildCtrlsToPersist);
                        arl.Add(Settings.XmlSettings.GetSettings(ofp));
                        cs.ChildCtrlsToPersist = (Lewis.SST.Settings.CtrlSettings[])arl.ToArray(typeof(Lewis.SST.Settings.CtrlSettings));
                        Settings.XmlSettings.WriteXml(windowsSettingsFile, cs);
                    }
                }
                else
                {
                    logger.Error(new Exception("The 'SaveWindowsSettings' was not found in the optionsSetting string"));
                }
            }
            else if (DeleteSettings != null && (CheckState)DeleteSettings == CheckState.Checked)
            {
                try
                {
                    File.Delete(configFileName);
                    File.Delete(optionsFileName);
                    File.Delete(windowsSettingsFile);
                }
                catch { logger.Error(new Exception("Cannot remove settings file.")); }
            }
            
            // C01: LLEWIS: close all connections
            foreach (SQLConnection sqlConn in this.m_serverExplorer.SQLConnections)
            {
                try
                {
                    sqlConn.sqlConnection.Close();
                    sqlConn.sqlConnection.Dispose();
                }
                catch { } //NOP, we don't care about any errors from this
            }

            closeAllToolStripMenuItem_Click(sender, new EventArgs());
            Cursor.Current = Cursors.Default;
            logger.Info("SST GUI Ending, saving options settings.");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string message = string.Format(Help.HelpText.SSTUsageString, System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion);
            About frmAbout = new About(a, message);
            frmAbout.Text = "About The Sql Schema Tool";
            frmAbout.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Save(false);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocumentPane.ActiveContent != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocumentPane.ActiveContent.DockHandler.Form))
            {
                Document doc = (Document)dockPanel.ActiveDocumentPane.ActiveContent.DockHandler.Form;
                doc.Save(true);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            this.Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog.OptionsDialog od = new OptionsDialog.OptionsDialog(optionsSettings);
            if (ofp != null)
            {
                od.Location = ofp.Location;
                od.Size = ofp.Size;
            }
            od.ShowDialog();
            if (od.DialogResult == DialogResult.OK)
            {
                optionsSettings = od.ControlsValues;
            }
            ofp = new optionsFormProperties(od);
            setOptions(optionsSettings);
        }

        private void serverExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_serverExplorer.Show(dockPanel);
        }

        private void xmlExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_xmlNodeExplorer.Show(dockPanel);
        }

        private void setSecurityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_serverExplorer.SetSelectedServerSecurity(m_currentServerTreeNode);
        }

        private void getDTSPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_serverExplorer.OpenDTSSerializer(m_currentServerTreeNode);
        }

        private void xMLFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // open xml document
            XMLDoc xDoc = new XMLDoc();
            xDoc.InitialDirectory = LastDirectory;
            if (xDoc.Open(dockPanel))
            {
                LastDirectory = xDoc.InitialDirectory;
            }
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // open sql document
            SQLDoc sDoc = new SQLDoc();
            sDoc.InitialDirectory = LastDirectory;
            if (sDoc.Open(dockPanel))
            {
                LastDirectory = sDoc.InitialDirectory;
            }
        }

        private void compareXMLSpanshotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // choose two XML db snapshot files to compare
            if (m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null)
            {
                MessageBox.Show("You need to select two Xml Snapshot Files to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            m_serverExplorer.StartAsyncSchemaCompare<object>(new string[] { m_xmlSnapShotFile1, m_xmlSnapShotFile2 });
        }

        private void xMLSnapshotAndDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // choose two XML db snapshot files to compare
            bool errFlag = false;
            if (m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null && m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null)
            {
                errFlag = true;
            }
            if (m_xmlSnapShotFile1 == null && m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null)
            {
                errFlag = true;
            }
            if (m_xmlSnapShotFile2 == null && m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null)
            {
                errFlag = true;
            }
            if (m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null && m_currentCompareDBTreeNode1 == null)
            {
                errFlag = true;
            }
            if (m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null && m_currentCompareDBTreeNode2 == null)
            {
                errFlag = true;
            }
            if (errFlag)
            {
                MessageBox.Show("You need to select a combination of a source Xml Snapshot File or DB and a target Xml Snapshot File or DB to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            m_serverExplorer.StartAsyncSchemaCompare(new DBTreeNode[] { m_currentCompareDBTreeNode1, m_currentCompareDBTreeNode2 }, new string[] { m_xmlSnapShotFile1, m_xmlSnapShotFile2 });
        }

        private void selectedDatabasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // compare selected SQL databases and generate XML diff and SQL diff files
            m_serverExplorer.StartAsyncSchemaCompare(new DBTreeNode[] { m_currentCompareDBTreeNode1, m_currentCompareDBTreeNode2 });
        }

        private void selectedDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate selected SQL database schema xml snapshot and SQL script files
			// LLEWIS: 05-18-2010
			if (m_currentSelectedDBTreeNode != null)
			{
				this.toolStripStatusLabel1.Text = "Generating SQL Schema From: " + m_currentSelectedDBTreeNode.Server + "\\" + m_currentSelectedDBTreeNode.Text;
				m_serverExplorer.StartAsyncSchemaGeneration(m_currentSelectedDBTreeNode, true);
			}
			else
			{
				this.toolStripStatusLabel1.Text = "No Database Selected!";
			}
        }

        private void chooseXMLSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate SQL Schema from single XML file snapshot and open the resulting SQL doc
            // use current xml doc if it is of a database schema type
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                this.toolStripStatusLabel1.Text = "Generating SQL Schema From: " + dockPanel.ActiveDocument.DockHandler.Form.Text;
                if (!SQLTransformation((XMLDoc)dockPanel.ActiveDocument.DockHandler.Form))
                {   
                    // open file dialog to get XML snapshot
                    SQLTransformation(null);
                }
            } 
            else 
            { 
                // else open file dialog to get XML snapshot
                SQLTransformation(null);
            }
        }

        private void trans_CompleteTransformSQL(string FileName)
        {
            SQLDoc sqlDoc = new SQLDoc();
            sqlDoc.InitialDirectory = LastDirectory;
            sqlDoc.RightToLeftLayout = RightToLeftLayout;
            sqlDoc.FileName = FileName;
            sqlDoc.Show(this.dockPanel);
            LastDirectory = sqlDoc.InitialDirectory;
            this.toolStripStatusLabel1.Text = "Completed XML Snapshot to SQL Transformation.";
        }

        private void databaseSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate database snapshot xml file from the currently selected database
			if (m_currentSelectedDBTreeNode != null) // LLEWIS: 05-18-2010
			{
				this.toolStripStatusLabel1.Text = "Generating DTS XML Package From: " + m_currentSelectedDBTreeNode.Server + "\\" + m_currentSelectedDBTreeNode.Text;
				m_serverExplorer.StartAsyncSchemaGeneration(m_currentSelectedDBTreeNode, false);
			}
			else
			{
				this.toolStripStatusLabel1.Text = "No Database Selected!";
			}
        }

        private void dTSPackageSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate dts xml file from the currently selected server's dts package
            // the DTS doc window must be open and have a selected record.
            if (dockPanel.ActiveDocument != null && typeof(DTSDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                DTSDoc dtsDoc = (DTSDoc)dockPanel.ActiveDocument.DockHandler.Form;
                this.toolStripStatusLabel1.Text = "Generating DTS XML Package From: " + dtsDoc.TabText;
                dtsDoc.GenerateXMLPackage();
            }
        }

        private void xMLFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // rehydrate DTS package from DTS XML file
            bool openNewFlag = true;
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc xmlDoc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                if (xmlDoc.IsDTSPackage)
                {
                    this.toolStripStatusLabel1.Text = "Generating DTS Package From: " + xmlDoc.TabText;
                    xmlDoc.ReHydrateDTS(m_serverExplorer.Servers);
                    openNewFlag = false;
                }
                else
                {
                    openNewFlag = true;
                }
            }
            if (openNewFlag) // open new window
            {
                XMLDoc xmlDoc = new XMLDoc();
                xmlDoc.InitialDirectory = LastDirectory;
                if (!xmlDoc.Open(dockPanel))
                {
                    xmlDoc.Close();
                    xmlDoc.Dispose();
                    return;
                }
                else
                {
                    LastDirectory = xmlDoc.InitialDirectory;
                }
                if (xmlDoc.IsDTSPackage)
                {
                    this.toolStripStatusLabel1.Text = "Generating DTS Package From: " + xmlDoc.TabText;
                    xmlDoc.ReHydrateDTS(m_serverExplorer.Servers);
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // closes the active document - asking to save if there are pending changes
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Close();
            }
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save all docs that have pending changes without asking - 
            // if there is no filename then a dialog should appear
            foreach (IDockContent dc in dockPanel.Contents)
            {
                if (typeof(Document).IsInstanceOfType(dc.DockHandler.Form))
                {
                    Document doc = (Document)dc.DockHandler.Form;
                    if (doc.TextChanged)
                    {
                        doc.Save(false);
                        LastDirectory = doc.InitialDirectory;
                    }
                }
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_xmlNodeExplorer.Filename != null && m_xmlNodeExplorer.Filename.Length > 0)
            {
                m_xmlNodeExplorer.Clear();
            }

            // Closes all open documents - asking to save if there are pending changes
            Cursor.Current = Cursors.WaitCursor;
            // have to get an array copy of docs since we are disposing of each window
            // and that causes an error because of the changed collection
            IDockContent[] idc = new IDockContent[dockPanel.Contents.Count];
            dockPanel.Contents.CopyTo(idc, 0);
            for(int ii = 0; ii < idc.Length; ii++)
            {
                IDockContent dc = idc[ii];
                if (typeof(Document).IsInstanceOfType(dc.DockHandler.Form))
                {
                    Document doc = (Document)dc.DockHandler.Form;
                    doc.Close();
                }
            }
            btnRedo.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            btnPaste.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            Cursor.Current = Cursors.Default;
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // find method
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Find(true);
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // replace method
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Replace();
            }
        }

        private void findAndReplaceAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // replace method
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.ReplaceAll();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // copy method
                btnPaste.Enabled = true;
                pasteToolStripMenuItem.Enabled = true;
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Copy();
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // paste method
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Paste();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // cut method
                btnPaste.Enabled = true;
                pasteToolStripMenuItem.Enabled = true;
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Cut();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // undo method
                btnRedo.Enabled = true;
                redoToolStripMenuItem.Enabled = true;
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Undo();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(Document).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                // redo method
                Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
                doc.Redo();
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            saveAllToolStripMenuItem_Click(sender, e);
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            btnPaste.Enabled = true;
            pasteToolStripMenuItem.Enabled = true;
            copyToolStripMenuItem_Click(sender, e);
        }

        private void btnCut_Click(object sender, EventArgs e)
        {
            btnPaste.Enabled = true;
            pasteToolStripMenuItem.Enabled = true;
            cutToolStripMenuItem_Click(sender, e);
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            btnRedo.Enabled = true;
            redoToolStripMenuItem.Enabled = true;
            undoToolStripMenuItem_Click(sender, e);
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            redoToolStripMenuItem_Click(sender, e);
        }

        private void btnServerExplorer_Click(object sender, EventArgs e)
        {
            serverExplorerToolStripMenuItem_Click(sender, e);
        }

        private void btnXmlExplorer_Click(object sender, EventArgs e)
        {
            xmlExplorerToolStripMenuItem_Click(sender, e);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            aboutToolStripMenuItem_Click(sender, e);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: add print report menu function
        }

        private void sqlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem1_Click(sender, e);
        }

        private void xMLFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xMLFileToolStripMenuItem1_Click(sender, e);
        }

        private void btnOpenFile_ButtonClick(object sender, EventArgs e)
        {
            // open file doc of last type opened or currently active doc type, or default to SQL
            if (lastFileType.ToLower().Contains("sql"))
            {
                fileToolStripMenuItem1_Click(sender, e);
            }
            if (lastFileType.ToLower().Contains("xml"))
            {
                xMLFileToolStripMenuItem1_Click(sender, e);
            }
            else
            {
                Random r = new Random(System.DateTime.Now.Millisecond);
                int i = 0; 
                Math.DivRem(r.Next(), 2, out i);
                if (i == 0)
                {
                    xMLFileToolStripMenuItem1.PerformClick();
                    lastFileType = "xml";
                }
                else
                {
                    fileToolStripMenuItem1.PerformClick();
                    lastFileType = "sql";
                }
            }
        }

        private void btnSetSecurity_Click(object sender, EventArgs e)
        {
            if (m_currentServerTreeNode != null)
            {
                m_currentServerTreeNode.LastMouseButton = MouseButtons.Right;
                setSecurityToolStripMenuItem_Click(sender, e);
            }
        }

        private void btnAddServer_Click(object sender, EventArgs e)
        {
            addServerToolStripMenuItem_Click(sender, e);
        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_serverExplorer.addServer();
        }


        private void btnCompareSelected_Click(object sender, EventArgs e)
        {
            bool DBsOnly = (m_currentCompareDBTreeNode1 != null && m_currentCompareDBTreeNode2 != null && m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null);
            bool XSnpShtOnly = (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 != null && m_xmlSnapShotFile2 != null);
            bool DBSrcXSnpShtDest = (m_currentCompareDBTreeNode1 != null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 != null);
            bool XSnpShtSrcDBDest = (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 != null && m_xmlSnapShotFile1 != null && m_xmlSnapShotFile2 == null);
            bool truthTable = DBsOnly || XSnpShtOnly || DBSrcXSnpShtDest || XSnpShtSrcDBDest;
            bool compareData = sourceTableName != string.Empty && targetTableName != string.Empty;
            if (truthTable)
            {
                m_serverExplorer.StartAsyncSchemaCompare(new DBTreeNode[] { m_currentCompareDBTreeNode1, m_currentCompareDBTreeNode2 }, new string[] { m_xmlSnapShotFile1, m_xmlSnapShotFile2 });
            }
            else if (!truthTable && compareData)
            {
                CompareData();
                sourceTableName = string.Empty;
                targetTableName = string.Empty;
                selectedTables[0] = null;
                selectedTables[1] = null;
                selectedTablesMenuItem.Enabled = false;
                this.toolStripStatusLabel1.Text = "Finished Data Table Compare.";
                this.toolStripStatusLabel2.Text = string.Empty;
            }
            else if (!truthTable && !compareData)
            {
                MessageBox.Show("You need to select some combination of a source Xml Snapshot File or DB and a target Xml Snapshot File or DB to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnGenerateSchema_Click(object sender, EventArgs e)
        {
            // generate SQL Schema from single XML file snapshot and open the resulting SQL doc
            // use current xml doc if it is of a database schema type
            if (m_currentSelectedDBTreeNode != null)
            {
                // generate selected SQL database schema xml snapshot and SQL script files
                this.toolStripStatusLabel1.Text = "Generating SQL Schema From: " + m_currentSelectedDBTreeNode.Server + "\\" + m_currentSelectedDBTreeNode.Text;
                bool create = m_serverExplorer.StartAsyncSchemaGeneration(m_currentSelectedDBTreeNode, true);
                if (!create)
                {
                    this.toolStripStatusLabel1.Text = "";
                }
            }
            else if (m_currentSelectedTableTreeNode != null)
            {
                m_serverExplorer.GenerateTableSchema();
            }
            else if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                this.toolStripStatusLabel1.Text = "Generating SQL Schema From: " + ((Document)dockPanel.ActiveDocument.DockHandler.Form).TabText;
                if (!SQLTransformation((XMLDoc)dockPanel.ActiveDocument.DockHandler.Form))
                {
                    // open file dialog to get XML snapshot
                    SQLTransformation(null);
                }
            }
        }

        private void btnGenXml_Click(object sender, EventArgs e)
        {
            // generate dts xml file from the currently selected server's dts package
            // the DTS doc window must be open and have a selected record.
            if (dockPanel.ActiveDocument != null && typeof(DTSDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                DTSDoc dtsDoc = (DTSDoc)dockPanel.ActiveDocument.DockHandler.Form;
                this.toolStripStatusLabel1.Text = "Generating DTS XML Package From: " + dtsDoc.TabText;
                dtsDoc.GenerateXMLPackage();
            }
            else if (m_currentSelectedDBTreeNode != null)
            {
                // generate database snapshot xml file from the currently selected database
                this.toolStripStatusLabel1.Text = "Generating DTS XML Package From: " + m_currentSelectedDBTreeNode.Server + "\\" + m_currentSelectedDBTreeNode.Text;
                m_serverExplorer.StartAsyncSchemaGeneration(m_currentSelectedDBTreeNode, false);
            }
        }

        private void btnCreateDTSPackage_Click(object sender, EventArgs e)
        {
            xMLFileToolStripMenuItem_Click(sender, e);
        }

        private void getDTSPackages_Click(object sender, EventArgs e)
        {
            getDTSPackageToolStripMenuItem_Click(sender, e);
        }

        private void btnSyncXml_Click(object sender, EventArgs e)
        {
            // add sync with XML node explorer from active XML doc
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                FileInfo fi = new FileInfo(doc.FileName);
                m_xmlNodeExplorer.OpenFile(fi.FullName);
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            // add select for compare from active XML Schema doc
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                doc.IsSelectedCompare = !doc.IsSelectedCompare;
            }
        }

        private void btnGenDatabase_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null && typeof(SQLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                SQLDoc doc = (SQLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                doc.ServerExplorer = m_serverExplorer;
                doc.RunSQLScript();
            }
        }

        private void btnRefreshServers_Click(object sender, EventArgs e)
        {
            // add refresh from SQLServerExplorer
            m_serverExplorer.getServers();
            m_serverExplorer.RefreshTreeView(true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.progressIndicator.Visible)
            {
                this.progressIndicator.PerformStep();
                if (this.progressIndicator.Value >= this.progressIndicator.Maximum)
                {
                    this.progressIndicator.Value = this.progressIndicator.Minimum;
                }
            }
        }

        private void btnHTML_Click(object sender, EventArgs e)
        {
            openHTML(false);
        }

        private void standardViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openHTML(true);
        }

        private void createCommandlineBatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create batch file for compare operations
            bool DBsOnly = (m_currentCompareDBTreeNode1 != null && m_currentCompareDBTreeNode2 != null && m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null);
            bool XSnpShtOnly = (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 != null && m_xmlSnapShotFile2 != null);
            bool DBSrcXSnpShtDest = (m_currentCompareDBTreeNode1 != null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 != null);
            bool XSnpShtSrcDBDest = (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 != null && m_xmlSnapShotFile1 != null && m_xmlSnapShotFile2 == null);
            bool truthTable = DBsOnly || XSnpShtOnly || DBSrcXSnpShtDest || XSnpShtSrcDBDest;
            DirectoryInfo di = Directory.GetParent(Application.StartupPath);
            FileInfo[] fi = di.GetFiles("sstcl.exe", SearchOption.AllDirectories);
            string sstclexe = "[Enter the application path here]sstcl.exe";
            if (fi.Length > 0)
            {
                sstclexe = fi[0].FullName;
            }
            if (truthTable)
            {
                string batchFileText = string.Empty;
                string sourceSecurity = string.Empty;
                string sourceServerDatabase = string.Empty;
                string destSecurity = string.Empty;
                string destServerDatabase = string.Empty;
                if (DBsOnly)
                {
                    if (m_currentCompareDBTreeNode1.Parent != null)
                    {
                        Lewis.SST.Controls.ServerTreeNode server = ((ServerTreeNode)m_currentCompareDBTreeNode1.Parent);
                        if (server.Security == SecurityType.Integrated)
                        {
                            sourceSecurity = Resources.StaticStrings.sst_SourceSecurityTrusted;
                        }
                        else
                        {
                            string uid = server.UID;
                            string pwd = server.Pwd;
                            sourceSecurity = string.Format(Resources.StaticStrings.sst_SourceSecurityPassword, uid, pwd);
                        }
                        sourceServerDatabase = string.Format(Resources.StaticStrings.sst_SourceDatabase,m_currentCompareDBTreeNode1.Parent.Text, m_currentCompareDBTreeNode1.Text);
                    }
                    if (m_currentCompareDBTreeNode2.Parent != null)
                    {
                        Lewis.SST.Controls.ServerTreeNode server = ((ServerTreeNode)m_currentCompareDBTreeNode1.Parent);
                        if (server.Security == SecurityType.Integrated)
                        {
                            destSecurity = Resources.StaticStrings.sst_DestSecurityTrusted;
                        }
                        else
                        {
                            string uid = server.UID;
                            string pwd = server.Pwd;
                            destSecurity = string.Format(Resources.StaticStrings.sst_DestSecurityPassword, uid, pwd);
                        }
                        destServerDatabase = string.Format(Resources.StaticStrings.sst_SourceDatabase, m_currentCompareDBTreeNode2.Parent.Text, m_currentCompareDBTreeNode2.Text);
                    }
                }
                else if (XSnpShtOnly)
                {
                    if (m_xmlSnapShotFile1 != null)
                    {
                        sourceServerDatabase = m_xmlSnapShotFile1;
                        sourceSecurity = string.Empty;
                    }
                    if (m_xmlSnapShotFile2 != null)
                    {
                        destServerDatabase = m_xmlSnapShotFile2;
                        destSecurity = string.Empty;
                    }
                }
                else if (DBSrcXSnpShtDest)
                {
                    if (m_currentCompareDBTreeNode1.Parent != null)
                    {
                        Lewis.SST.Controls.ServerTreeNode server = ((ServerTreeNode)m_currentCompareDBTreeNode1.Parent);
                        if (server.Security == SecurityType.Integrated)
                        {
                            sourceSecurity = Resources.StaticStrings.sst_SourceSecurityTrusted;
                        }
                        else
                        {
                            string uid = server.UID;
                            string pwd = server.Pwd;
                            sourceSecurity = string.Format(Resources.StaticStrings.sst_SourceSecurityPassword, uid, pwd);
                        }
                        sourceServerDatabase = string.Format(Resources.StaticStrings.sst_SourceDatabase, m_currentCompareDBTreeNode1.Parent.Text, m_currentCompareDBTreeNode1.Text);
                    }
                    if (m_xmlSnapShotFile2 != null)
                    {
                        destServerDatabase = m_xmlSnapShotFile2;
                        destSecurity = string.Empty;
                    }
                }
                else if (XSnpShtSrcDBDest)
                {
                    if (m_xmlSnapShotFile1 != null)
                    {
                        sourceServerDatabase = m_xmlSnapShotFile1;
                        sourceSecurity = string.Empty;
                    }
                    if (m_currentCompareDBTreeNode2.Parent != null)
                    {
                        Lewis.SST.Controls.ServerTreeNode server = ((ServerTreeNode)m_currentCompareDBTreeNode1.Parent);
                        if (server.Security == SecurityType.Integrated)
                        {
                            destSecurity = Resources.StaticStrings.sst_DestSecurityTrusted;
                        }
                        else
                        {
                            string uid = server.UID;
                            string pwd = server.Pwd;
                            destSecurity = string.Format(Resources.StaticStrings.sst_DestSecurityPassword, uid, pwd);
                        }
                        destServerDatabase = string.Format(Resources.StaticStrings.sst_SourceDatabase, m_currentCompareDBTreeNode2.Parent.Text, m_currentCompareDBTreeNode2.Text);
                    }
                }
                batchFileText = string.Format(Resources.StaticStrings.sst_CommandLineCompare, sstclexe, sourceServerDatabase, sourceSecurity, destServerDatabase, destSecurity);
                BATDoc bDoc = new BATDoc();
                bDoc.InitialDirectory = LastDirectory;
                bDoc.BatchText = batchFileText;
                bDoc.Show(this.dockPanel);
                LastDirectory = bDoc.InitialDirectory;
            }
            else
            {
                MessageBox.Show("You must select a source and a target database/XML Snapshot to generate the batch file.", "BATCH FILE!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void createCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create batch file for compare operations
            bool DBsOnly = (m_currentCompareDBTreeNode1 != null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 == null && m_xmlSnapShotFile2 == null);
            bool XSnpShtOnly = (m_currentCompareDBTreeNode1 == null && m_currentCompareDBTreeNode2 == null && m_xmlSnapShotFile1 != null && m_xmlSnapShotFile2 != null);
            bool truthTable = DBsOnly || XSnpShtOnly ;
            DirectoryInfo di = Directory.GetParent(Application.StartupPath);
            FileInfo[] fi = di.GetFiles("sstcl.exe", SearchOption.AllDirectories);
            string sstclexe = "[Enter the application path here]sstcl.exe";
            if (fi.Length > 0)
            {
                sstclexe = fi[0].FullName;
            }
            if (truthTable)
            {
                string batchFileText = string.Empty;
                string sourceSecurity = string.Empty;
                string sourceServerDatabase = string.Empty;
                string xmlfileName = string.Empty;
                if (DBsOnly)
                {
                    if (m_currentCompareDBTreeNode1.Parent != null)
                    {
                        Lewis.SST.Controls.ServerTreeNode server = ((ServerTreeNode)m_currentCompareDBTreeNode1.Parent);
                        if (server.Security == SecurityType.Integrated)
                        {
                            sourceSecurity = Resources.StaticStrings.sst_SourceSecurityTrusted;
                        }
                        else
                        {
                            string uid = server.UID;
                            string pwd = server.Pwd;
                            sourceSecurity = string.Format(Resources.StaticStrings.sst_SourceSecurityPassword, uid, pwd);
                        }
                        sourceServerDatabase = string.Format(Resources.StaticStrings.sst_SourceDatabase, m_currentCompareDBTreeNode1.Parent.Text, m_currentCompareDBTreeNode1.Text);
                        xmlfileName = @"/Translate";
                    }
                }
                else if (XSnpShtOnly)
                {
                    if (m_xmlSnapShotFile1 != null)
                    {
                        xmlfileName = string.Format(@"/CreateXMLFile={0}", m_xmlSnapShotFile1);
                        sourceSecurity = string.Empty;
                    }
                }
                batchFileText = string.Format(Resources.StaticStrings.sst_CommandLineGenerate, sstclexe, sourceServerDatabase, sourceSecurity, xmlfileName);
                BATDoc bDoc = new BATDoc();
                bDoc.InitialDirectory = LastDirectory;
                bDoc.BatchText = batchFileText;
                bDoc.Show(this.dockPanel);
                LastDirectory = bDoc.InitialDirectory;
            }
            else
            {
                MessageBox.Show("You must select a source database/XML Snapshot to generate the batch file.", "BATCH FILE!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void createCommandlineBatchFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO create batch file for re-hydrating XML to DTS Package
            // also need to do one for creating xml dts snapshot
            Document doc = (Document)dockPanel.ActiveDocument.DockHandler.Form;
            if (typeof(XMLDoc).IsInstanceOfType(doc))
            {
                if (((XMLDoc)doc).IsDTSPackage)
                {
                }
            }
        }

        private void selectedTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // compare table data
            bool compareData = sourceTableName != string.Empty && targetTableName != string.Empty;
            if (compareData)
            {
                CompareData();
                sourceTableName = string.Empty;
                targetTableName = string.Empty;
                selectedTables[0] = null;
                selectedTables[1] = null;
                selectedTablesMenuItem.Enabled = false;
                this.toolStripStatusLabel1.Text = "Finished Data Table Compare.";
                this.toolStripStatusLabel2.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("You need to select a source Data Table and a target Data Table to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region private methods used by event handler methods

        private void buttonLayout(ToolStripItem button)
        {
            if (button is ToolStripButton || button is ToolStripSplitButton)
            {
                if (iconsAndTextToolStripMenuItem.Checked)
                {
                    button.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    button.ImageAlign = ContentAlignment.TopCenter;
                    button.TextAlign = ContentAlignment.BottomCenter;
                    button.TextImageRelation = TextImageRelation.ImageAboveText;
                    button.Tag = button.Text;
                    button.ToolTipText = button.Text;
                    string first = button.Text.Split(' ')[0].Replace("...", "");
                    button.Text = first;
                }
                else
                {
                    button.DisplayStyle = ToolStripItemDisplayStyle.Image;
                    button.ImageAlign = ContentAlignment.MiddleCenter;
                    button.TextAlign = ContentAlignment.MiddleCenter;
                    button.TextImageRelation = TextImageRelation.Overlay;
                    button.Text = Convert.ToString(button.Tag);
                }
            }
        }

        private void openHTML(bool defaultView)
        {
            // load html report
            String fileName = null;
            HTMLDoc hDoc = new HTMLDoc("Report");
            hDoc.InitialDirectory = LastDirectory;
            hDoc.DefaultView = defaultView;
            hDoc.DocumentLoaded += new EventHandler<WebBrowserDocumentCompletedEventArgs>(hDoc_DocumentLoaded);
            
            if (dockPanel.ActiveDocument != null && typeof(XMLDoc).IsInstanceOfType(dockPanel.ActiveDocument.DockHandler.Form))
            {
                XMLDoc doc = (XMLDoc)dockPanel.ActiveDocument.DockHandler.Form;
                if (doc.IsDatabaseDiff || doc.IsDatabaseSchema)
                {
                    hDoc.DefaultView = false;
                }
                else
                {
                    hDoc.DefaultView = true;
                }
                fileName = doc.FileName;
            }
            disableProgressIndicator();
            if (fileName != null)
            {
                hDoc.FileName = fileName;
                hDoc.Show(dockPanel);
                LastDirectory = hDoc.InitialDirectory;
            }
            else
            {
                // BUG FIX 08-02-2007 reported: awittig
                if (!hDoc.Open(dockPanel))
                {
                    return;
                }
                else
                {
                    LastDirectory = hDoc.InitialDirectory;
                }
            }
            this.toolStripStatusLabel1.Text = string.Format("Loading Report: {0}", fileName);
            this.timer1.Enabled = true;
            this.progressIndicator.Visible = true;
            this.progressIndicator.Value = 0;
        }

        private void hDoc_DocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Finished Loading Report.";
            disableProgressIndicator();
        }

        private void CompareData()
        {
            if (selectedTables[0] != null && selectedTables[1] != null)
            {
                if (m_serverExplorer.SelectedTablesCount > 2)
                {
                    MessageBox.Show("Only first two Tables are used for compare!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                string sourceQuery = "select * from " + selectedTables[0].FullTablePath;
                SqlConnection source = ((ServerTreeNode)selectedTables[0].Parent.Parent).SQLServerConnection;
                string targetQuery = "select * from " + selectedTables[1].FullTablePath;
                SqlConnection target = ((ServerTreeNode)selectedTables[1].Parent.Parent).SQLServerConnection;
                string htmlReportName = "TableDiffReport.html";
                HTMLDoc hDoc = new HTMLDoc(htmlReportName);
                hDoc.InitialDirectory = LastDirectory;
                string m_DialogTitle = "Save HTML File As...";
                string m_DialogTypeFilter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
                string m_fileName = htmlReportName;
                ArrayList arl = hDoc.ShowSaveFileDialog(m_fileName, m_DialogTitle, m_DialogTypeFilter);
                DialogResult dr = (DialogResult)arl[0];
                if (htmlReportName != null && htmlReportName != string.Empty)
                {
                    if (dr == DialogResult.OK)
                    {
                        htmlReportName = SQLData.CompareData(sourceQuery, targetQuery, source, target, false);
                        if (htmlReportName != null && htmlReportName != string.Empty)
                        {
                            m_fileName = (string)arl[1];
                            File.WriteAllText(m_fileName, File.ReadAllText(htmlReportName));
                            hDoc.DefaultView = true;
                            //hDoc.HTMLText = File.ReadAllText(htmlReportName);
                            hDoc.FileName = m_fileName;
                            hDoc.Show(dockPanel);
                            LastDirectory = hDoc.InitialDirectory;
                        }
                    }
                }
                else
                {
                    hDoc.Close();
                    hDoc.Dispose();
                }
            }
            m_serverExplorer.deselectDBTables();
        }

        private bool SQLTransformation(XMLDoc xmlDoc)
        {
            bool retval = false;
            if (xmlDoc == null)
            {
                xmlDoc = new XMLDoc();
                xmlDoc.InitialDirectory = LastDirectory;
                if (!xmlDoc.Open(dockPanel))
                {
                    xmlDoc.Close();
                    xmlDoc.Dispose();
                    this.toolStripStatusLabel1.Text = "";
                }
                else
                {
                    LastDirectory = xmlDoc.InitialDirectory;
                }
            }
            if (xmlDoc.IsDatabaseSchema)
            {
                string m_DialogTitle = "Save SQL File As...";
                string m_DialogTypeFilter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                string m_fileName = xmlDoc.FileName.ToLower().Replace("xml", "sql");
                ArrayList arl = xmlDoc.ShowSaveFileDialog(m_fileName, m_DialogTitle, m_DialogTypeFilter);
                LastDirectory = xmlDoc.InitialDirectory;
                DialogResult dr = (DialogResult)arl[0];
                if (dr == DialogResult.OK)
                {
                    m_fileName = (string)arl[1];
                    AsyncTransformSQL trans = null;
                    if (xmlDoc.IsDatabaseDiff)
                    {
                        trans = new AsyncTransformSQL(this, xmlDoc.FileName, m_fileName, AsyncTransformSQL.TransformType.Diff);
                        trans.CompleteTransformSQL += new AsyncTransformSQL.CompleteTransformDelegate(trans_CompleteTransformSQL);
                    }
                    else
                    {
                        trans = new AsyncTransformSQL(this, xmlDoc.FileName, m_fileName, AsyncTransformSQL.TransformType.Create);
                        trans.CompleteTransformSQL += new AsyncTransformSQL.CompleteTransformDelegate(trans_CompleteTransformSQL);
                    }
                    string work = "Processing XML Snapshot: " + xmlDoc.FileName + "...";
                    this.toolStripStatusLabel2.Text = string.Empty;
                    this.toolStripStatusLabel1.Text = work;
                    trans.Start();
                    retval = true;
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "";
                }
            }
            return retval;
        }

        private void setOptions(string optionsSettings)
        {
            object cTables = OptionValues.GetValue("CompareTables", optionsSettings);
            object cViews = OptionValues.GetValue("CompareViews", optionsSettings);
            object cSprocs = OptionValues.GetValue("CompareSprocs", optionsSettings);
            object cFuncs = OptionValues.GetValue("CompareFuncs", optionsSettings);
            object cTriggers = OptionValues.GetValue("CompareTriggers", optionsSettings);
            object cRules = OptionValues.GetValue("CompareRules", optionsSettings);
            object cDefaults = OptionValues.GetValue("CompareDefaults", optionsSettings);
            object cUDDTs = OptionValues.GetValue("CompareUDDTs", optionsSettings);
            byte compareOptions = 0x00;
            if (cTables != null && (CheckState)cTables == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.TABLE);
            }
            if (cViews != null && (CheckState)cViews == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.VIEW);
            }
            if (cSprocs != null && (CheckState)cSprocs == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.SPROC);
            }
            if (cFuncs != null && (CheckState)cFuncs == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.FUNCTION);
            }
            if (cTriggers != null && (CheckState)cTriggers == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.TRIGGER);
            }
            if (cRules != null && (CheckState)cRules == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.RULE);
            }
            if (cDefaults != null && (CheckState)cDefaults == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.DEFAULT);
            }
            if (cUDDTs != null && (CheckState)cUDDTs == CheckState.Checked)
            {
                compareOptions |= Convert.ToByte(SQLSchemaTool._NodeType.UDDT);
            }
            m_serverExplorer.CompareOptions = compareOptions;
            object ApplyCustomDataXSLT = OptionValues.GetValue("ApplyCustomDataXSLT", optionsSettings);
            if (ApplyCustomDataXSLT != null && (CheckState)ApplyCustomDataXSLT == CheckState.Checked)
            {
                _customDataXSLTFile = (string)OptionValues.GetValue("CustomDataXSLTFile", optionsSettings);
            }
            else
            {
                _customDataXSLTFile = null;
            }
            object ApplyCustomSchemaXSLT = OptionValues.GetValue("ApplyCustomSchemaXSLT", optionsSettings);
            if (ApplyCustomSchemaXSLT != null && (CheckState)ApplyCustomSchemaXSLT == CheckState.Checked)
            {
                _customSchemaXSLTFile = (string)OptionValues.GetValue("CustomSchemaXSLTFile", optionsSettings);
            }
            else
            {
                _customSchemaXSLTFile = null;
            }
            m_serverExplorer.CustomSchemaXSLT = _customSchemaXSLTFile;
            m_serverExplorer.CustomDataXSLT = _customDataXSLTFile;
        }

        private string[] getDirectories(String strPath)
        {
            return Directory.GetDirectories(strPath);
        }

        private String[] getDrives()
        {
            return Directory.GetLogicalDrives();
        }

        private void disableProgressIndicator()
        {
            if (!_runningCompare && _runningDualSchemas < 1)
            {
                this.timer1.Enabled = false;
                this.progressIndicator.Visible = false;
                this.progressIndicator.Value = 0;
                _runningDualSchemas = 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class used to (de)serialize form properties on exit and load
    /// </summary>
    public class optionsFormProperties
    {
        private Size _size;
        private Point _location;
        private string _name;

        public optionsFormProperties()
        {
        }

        public optionsFormProperties(Form f)
        {
            _name = f.Name;
            _size = f.Size;
            _location = f.Location;
        }

        public Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public Point Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}