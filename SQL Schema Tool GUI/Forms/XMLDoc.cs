using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

using Lewis.SST.Controls;
using Lewis.SST.DTSPackageClass;
using Lewis.Xml;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Gui
{
    public partial class XMLDoc : Document
    {
        private object _isDTS;
        private object _isXMLSchema;
        private object _isXMLDiff;
        private bool _isSelected = false;
        private string xmlText;

        public event EventHandler<XMLDocEventArgs> SelectedForCompare;
        public event EventHandler<XMLDocEventArgs> UnSelectedForCompare;
        public event EventHandler<XMLDocEventArgs> SyncWithTree;
        public event EventHandler<XMLDocEventArgs> ReCreateDTS;

        public XMLDoc()
        { 
            InitializeComponent();

            SetTextEditorDefaultProperties();
            XmlFormattingStrategy strategy = new XmlFormattingStrategy();
            XmlFoldingStrategy folding = new XmlFoldingStrategy();

            txtEditCtl.Document.FormattingStrategy = (IFormattingStrategy)strategy;
            txtEditCtl.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("XML");
            txtEditCtl.Document.FoldingManager.FoldingStrategy = (IFoldingStrategy)folding;

            WireEvents();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtEditCtl.Text = xmlText;
            ForceFoldingUpdate(m_fileName);
            SetupMenuIemSelectCompare();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            xmlText = XsltHelper.Transform(File.ReadAllText(m_fileName), XsltHelper.PRETTYXSLT);
        }

        private void OnUnSelectedForCompare(int index)
        {
            XMLDocEventArgs args = new XMLDocEventArgs();
            args.selectedFileAndIndex = string.Format("{0},{1}", m_fileName, index).Split(',');

            EventHandler<XMLDocEventArgs> handler = UnSelectedForCompare;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSelectedForCompare(int index)
        {
            XMLDocEventArgs args = new XMLDocEventArgs();
            args.selectedFileAndIndex = string.Format("{0},{1}", m_fileName, index).Split(',');

            EventHandler<XMLDocEventArgs> handler = SelectedForCompare;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnSyncWithTree()
        {
            XMLDocEventArgs args = new XMLDocEventArgs();
            args.selectedFileAndIndex = string.Format("{0},-1", m_fileName).Split(',');

            EventHandler<XMLDocEventArgs> handler = SyncWithTree;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void OnReCreateDTS()
        {
            XMLDocEventArgs args = new XMLDocEventArgs();
            args.selectedFileAndIndex = string.Format("{0},-1", m_fileName).Split(',');

            EventHandler<XMLDocEventArgs> handler = ReCreateDTS;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void SetupMenuIemSelectCompare()
        {
            if (IsDatabaseSchema)
            {
                menuItem3.Name = "SelectCompare";
                menuItem3.Checked = false;
                menuItem3.Text = "Select for Schema Compare";
                menuItem3.Click += new EventHandler(SelectCompare_Click);
            }
            else if (IsDTSPackage)
            {
                menuItem3.Name = "createServerDTSPackage";
                menuItem3.Text = "Recreate DTS Package From This";
                menuItem3.Click += new EventHandler(createDTS_Click);
            }
            else
            {
                menuItem3.Visible = false;
            }
            menuItem4.Name = "SyncXMLTree";
            menuItem4.Text = "Synchronize in XML Node Explorer";
            menuItem4.Click += new EventHandler(SyncTree_Click);
            menuItem5.Name = "Close";
            menuItem5.Text = "Close This";
            menuItem5.Click += new EventHandler(closeThis_Click);
        }

        private void createDTS_Click(object sender, EventArgs e)
        {
            OnReCreateDTS();
        }

        private void closeThis_Click(object sender, EventArgs e)
        {
            // Close doc
            Close();
        }

        private void SyncTree_Click(object sender, EventArgs e)
        {
            // setup for xmlnodeexplorer synchronize
            // This will handle the fact that loading another explorer control or 
            // resyncing when this window gets made active is a time expensive operation

            // TODO: check for doc changes to save first
            OnSyncWithTree();
        }

        private void SelectCompare_Click(object sender, EventArgs e)
        {
            //  setup for schemacompare
            menuItem3.Checked = !menuItem3.Checked;
            IsSelectedCompare = menuItem3.Checked;
        }

        public override bool Open(DockPanel dockPanel)
        {
            m_DialogTitle = "Open XML File...";
            m_DialogTypeFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
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
                        backgroundWorker1.RunWorkerAsync();
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
            m_DialogTitle = "Save XML File As...";
            m_DialogTypeFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            base.Save(showDialog);
        }

        public bool IsDatabaseSchema
        {
            get
            {
                // persist return value on the first time
                //if (_isXMLSchema == null)
                {
                    _isXMLSchema = false;
                    if (txtEditCtl.Text.Trim().Length > 0)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        try
                        {
                            xmlDoc.LoadXml(txtEditCtl.Text);
                            XmlNodeList xNodeList = xmlDoc.SelectNodes("/DataBase_Schema/Database");
                            _isXMLSchema = (xNodeList.Count > 0);
                            xNodeList = null;
                        }
                        catch { }
                        xmlDoc = null;
                    }
                }
                return (bool)_isXMLSchema;
            }
        }

        public bool IsDatabaseDiff
        {
            get
            {
                // persist return value on the first time
                //if (_isXMLDiff == null)
                {
                    _isXMLDiff = false;
                    if (txtEditCtl.Text.Trim().Length > 0)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        try
                        {
                            xmlDoc.LoadXml(txtEditCtl.Text);
                            XmlNodeList xNodeList = xmlDoc.SelectNodes("/DataBase_Schema");
                            if (xNodeList.Count > 0)
                            {
                                _isXMLDiff = xNodeList[0].FirstChild.Value.ToLower().Trim().Replace("\n", "") == "diffdata" ? true : false; //expect in node "DiffData\n   "
                            }
                            xNodeList = null;
                        }
                        catch { }
                        xmlDoc = null;
                    }
                }
                return (bool)_isXMLDiff;
            }
        }

        public bool IsDTSPackage
        {
            get
            {
                // persist return value on the first time
                //if (_isDTS == null)
                {
                    _isDTS = false;
                    if (txtEditCtl.Text.Trim().Length > 0)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        try
                        {
                            xmlDoc.LoadXml(txtEditCtl.Text);
                            XmlNodeList xNodeList = xmlDoc.SelectNodes("/DTS_File/Package");
                            _isDTS = (xNodeList.Count > 0);
                            xNodeList = null;
                        }
                        catch { }
                        xmlDoc = null;
                    }
                }
                return (bool)_isDTS;
            }
        }

        public bool IsSelectedCompare
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value;
                if (!_isSelected && IsDatabaseSchema)
                {
                    if (menuItem3.Checked)
                        menuItem3.Checked = false;
                    if (!SelectedCompareDocs.ContainsKey(TabText)) return;
                    int index = ((string)SelectedCompareDocs[TabText]).Equals("Source") ? 0 : 1;
                    SelectedCompareDocs.Remove(TabText);
                    index = index >= 0 ? index : Tag != null ? (int)Tag : -1;
                    OnUnSelectedForCompare(index);
                    txtEditCtl.Document.ReadOnly = false;
                    Tag = null;
                }
                if (_isSelected && IsDatabaseSchema)
                {
                    int count = SelectedCompareDocs.Count;
                    if (count > 1)
                    {
                        _isSelected = false;
                        if (menuItem3.Checked)
                            menuItem3.Checked = false;
                        MessageBox.Show("You can only select two XML Snapshots to compare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (!menuItem3.Checked)
                        menuItem3.Checked = true;
                    if (count == 0 || !SelectedCompareDocs.ContainsKey(TabText))
                    {
                        // TODO: check for databases already selected
                        SelectedCompareDocs.Add(TabText, count == 0 ? "Source" : SelectedCompareDocs.ContainsValue("Target") ? "Source" : "Target");
                    }
                    int index = ((string)SelectedCompareDocs[TabText]).Equals("Source") ? 0 : 1;
                    Tag = index;
                    txtEditCtl.Document.ReadOnly = true;
                    // add eventhandler to this class to expose down in main window class
                    OnSelectedForCompare(index);
                }
            }
        }

        public void ReHydrateDTS(string[] servers)
        {
            if (IsDTSPackage)
            {
                string PackageName = null;
                string Logfile = null;
                string PackageDescription = null;
                string _UID = null;
                string _PWD = null;
                string _SQLServer = null;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(txtEditCtl.Text);
                XmlNodeList xNodeList = xmlDoc.SelectNodes("/DTS_File/Package/Name");
                PackageName = xNodeList.Count > 0 ? ((XmlNode)xNodeList.Item(0)).InnerText : "";
                xNodeList = xmlDoc.SelectNodes("/DTS_File/Package/Description");
                PackageDescription = xNodeList.Count > 0 ? ((XmlNode)xNodeList.Item(0)).InnerText : "";
                xNodeList = xmlDoc.SelectNodes("/DTS_File/Package/LogFileName");
                Logfile = xNodeList.Count > 0 ? ((XmlNode)xNodeList.Item(0)).InnerText : "";

                xNodeList = null;
                xmlDoc = null;

                // create new DTS package object in memory
                DTSPackage2 oPackage = new DTSPackage2(PackageName, Logfile, PackageDescription);

                SQLSecuritySettings sss = new SQLSecuritySettings(servers);
                if (servers != null && servers.Length > 0)
                {
                    sss.SetServerName(servers[0]);
                }

                DialogResult dr = sss.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    try
                    {
                        _UID = sss.User;
                        _PWD = sss.PWD;
                        _SQLServer = sss.SelectedSQLServer;

                        // set up the DTS package authentication type
                        if (sss.SecurityMode() == Lewis.SST.SQLMethods.SecurityType.Mixed)
                        {
                            oPackage.Authentication = DTSPackage2.authTypeFlags.Default;
                        }
                        else
                        {
                            oPackage.Authentication = DTSPackage2.authTypeFlags.Trusted;
                        }
                        // TODO: make this an async operation for larger dts files
                        Cursor.Current = Cursors.WaitCursor;
                        oPackage.Load(FileName);
                        oPackage.Save(_SQLServer, _UID, _PWD, null);
                    }
                    catch (Exception ex)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        oPackage = null;
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }
    }

    public class XMLDocEventArgs : EventArgs
    {
        public string [] selectedFileAndIndex;
    }

}